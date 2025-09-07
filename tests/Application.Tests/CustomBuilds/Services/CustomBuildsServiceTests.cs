using Moq;
using PathfinderCampaignManager.Application.CustomBuilds.Models;
using PathfinderCampaignManager.Application.CustomBuilds.Services;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Services;
using Xunit;

namespace PathfinderCampaignManager.Application.Tests.CustomBuilds.Services;

public class CustomBuildsServiceTests
{
    private readonly Mock<ICustomBuildsRepository> _repositoryMock;
    private readonly Mock<IModifierEngine> _modifierEngineMock;
    private readonly CustomBuildsService _service;

    public CustomBuildsServiceTests()
    {
        _repositoryMock = new Mock<ICustomBuildsRepository>();
        _modifierEngineMock = new Mock<IModifierEngine>();
        _service = new CustomBuildsService(_repositoryMock.Object, _modifierEngineMock.Object);
    }

    [Fact]
    public async Task GetCustomDefinitionAsync_WhenDefinitionExists_ReturnsDto()
    {
        var definitionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customDefinition = CreateTestCustomDefinition(definitionId, userId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(definitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customDefinition);

        var result = await _service.GetCustomDefinitionAsync(definitionId, userId);

        Assert.NotNull(result);
        Assert.Equal(definitionId, result.Id);
        Assert.Equal(userId, result.OwnerUserId);
        Assert.Equal("Test Item", result.Name);
    }

    [Fact]
    public async Task GetCustomDefinitionAsync_WhenDefinitionNotFound_ReturnsNull()
    {
        var definitionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(definitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomDefinition?)null);

        var result = await _service.GetCustomDefinitionAsync(definitionId, userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomDefinitionAsync_WhenUserCannotView_ReturnsNull()
    {
        var definitionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var customDefinition = CreateTestCustomDefinition(definitionId, ownerId);
        
        // Make it private so requesting user cannot view
        customDefinition.GetType()
            .GetProperty("IsPublic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(customDefinition, false);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(definitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customDefinition);

        var result = await _service.GetCustomDefinitionAsync(definitionId, requestingUserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateMagicItemAsync_WithValidInput_CreatesAndReturnsItem()
    {
        var ownerId = Guid.NewGuid();
        var modifiers = new List<ModifierDto>
        {
            new()
            {
                Target = ModifierTarget.Strength,
                Value = 2,
                ModifierType = ModifierType.Enhancement,
                Condition = null,
                IsActive = true,
                Priority = 0
            }
        };

        var result = await _service.CreateMagicItemAsync("Belt of Giant Strength", "Grants +2 Enhancement bonus to Strength", modifiers, ownerId);

        Assert.NotNull(result);
        Assert.Equal("Belt of Giant Strength", result.Name);
        Assert.Equal("Grants +2 Enhancement bonus to Strength", result.Description);
        Assert.Equal(CustomDefinitionType.Item, result.Type);
        Assert.Equal(ownerId, result.OwnerUserId);
        Assert.Single(result.Modifiers);
        Assert.Equal(ModifierTarget.Strength, result.Modifiers[0].Target);
        Assert.Equal(2, result.Modifiers[0].Value);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<CustomDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CalculateCharacterStatsWithCustomItemsAsync_WithValidInputs_ReturnsCalculatedStats()
    {
        var characterId = Guid.NewGuid();
        var customItemIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var character = new Character { Id = characterId, Name = "Test Character", Level = 1 };
        var customItems = new List<CustomDefinition>
        {
            CreateTestCustomDefinition(customItemIds[0], Guid.NewGuid()),
            CreateTestCustomDefinition(customItemIds[1], Guid.NewGuid())
        };

        var expectedStats = new CalculatedCharacterStats
        {
            CharacterId = characterId,
            BaseStats = new Dictionary<ModifierTarget, int> { { ModifierTarget.Strength, 10 } },
            FinalStats = new Dictionary<ModifierTarget, int> { { ModifierTarget.Strength, 14 } }
        };

        _repositoryMock
            .Setup(r => r.GetCharacterAsync(characterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(character);

        _repositoryMock
            .Setup(r => r.GetCustomDefinitionsByIdsAsync(customItemIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customItems);

        _modifierEngineMock
            .Setup(m => m.CalculateCharacterStats(character, It.IsAny<List<CustomDefinitionModifier>>()))
            .Returns(expectedStats);

        var result = await _service.CalculateCharacterStatsWithCustomItemsAsync(characterId, customItemIds);

        Assert.NotNull(result);
        Assert.Equal(characterId, result.CharacterId);
        Assert.Equal(14, result.FinalStats[ModifierTarget.Strength]);

        _modifierEngineMock.Verify(m => m.CalculateCharacterStats(character, It.IsAny<List<CustomDefinitionModifier>>()), Times.Once);
    }

    [Theory]
    [InlineData("", false)] // Empty name
    [InlineData("A", true)] // Valid short name
    [InlineData("This is a very long name that exceeds the maximum length allowed for custom definition names in the system", false)] // Too long name
    public async Task ValidateCustomDefinitionAsync_WithVariousNames_ReturnsExpectedResult(string name, bool expected)
    {
        var definition = new CustomDefinitionDto
        {
            Name = name,
            Description = "Valid description",
            JsonData = "{}",
            Modifiers = new List<ModifierDto>()
        };

        var result = await _service.ValidateCustomDefinitionAsync(definition);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ValidateCustomDefinitionAsync_WithTooManyModifiers_ReturnsFalse()
    {
        var modifiers = new List<ModifierDto>();
        for (int i = 0; i < CustomBuildConstants.MAX_MODIFIERS_PER_ITEM + 1; i++)
        {
            modifiers.Add(new ModifierDto
            {
                Target = ModifierTarget.Strength,
                Value = 1,
                ModifierType = ModifierType.Enhancement
            });
        }

        var definition = new CustomDefinitionDto
        {
            Name = "Valid Name",
            Description = "Valid description",
            JsonData = "{}",
            Modifiers = modifiers
        };

        var result = await _service.ValidateCustomDefinitionAsync(definition);

        Assert.False(result);
    }

    [Theory]
    [InlineData(CustomBuildConstants.MAX_MODIFIER_VALUE, true)]
    [InlineData(CustomBuildConstants.MAX_MODIFIER_VALUE + 1, false)]
    [InlineData(CustomBuildConstants.MIN_MODIFIER_VALUE, true)]
    [InlineData(CustomBuildConstants.MIN_MODIFIER_VALUE - 1, false)]
    public async Task ValidateCustomDefinitionAsync_WithVariousModifierValues_ReturnsExpectedResult(int modifierValue, bool expected)
    {
        var definition = new CustomDefinitionDto
        {
            Name = "Valid Name",
            Description = "Valid description",
            JsonData = "{}",
            Modifiers = new List<ModifierDto>
            {
                new()
                {
                    Target = ModifierTarget.Strength,
                    Value = modifierValue,
                    ModifierType = ModifierType.Enhancement
                }
            }
        };

        var result = await _service.ValidateCustomDefinitionAsync(definition);

        Assert.Equal(expected, result);
    }

    private static CustomDefinition CreateTestCustomDefinition(Guid id, Guid ownerId)
    {
        var definition = CustomDefinition.Create(
            ownerId,
            CustomDefinitionType.Item,
            "Test Item",
            "Test Description",
            "{}",
            "Test Category");

        // Set the Id using reflection since it's likely protected
        typeof(CustomDefinition)
            .GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
            .SetValue(definition, id);

        definition.AddModifier(ModifierTarget.Strength, 2, ModifierType.Enhancement);

        return definition;
    }
}

// Test character entity for unit tests
public class Character
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
}