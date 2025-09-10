using Xunit;
using Moq;
using FluentAssertions;
using PathfinderCampaignManager.Infrastructure.Services;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Errors;

namespace PathfinderCampaignManager.Domain.Tests;

public class ArchetypeServiceTests
{
    private readonly Mock<IArchetypeRepository> _mockRepository;
    private readonly ArchetypeService _archetypeService;

    public ArchetypeServiceTests()
    {
        _mockRepository = new Mock<IArchetypeRepository>();
        _archetypeService = new ArchetypeService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAvailableArchetypesAsync_ValidCharacter_ReturnsArchetypes()
    {
        // Arrange
        var character = CreateTestCharacter();
        var archetypes = new List<PfArchetype>
        {
            CreateBarbarianArchetype(),
            CreateWizardArchetype()
        };

        _mockRepository.Setup(r => r.GetAllArchetypesAsync())
            .ReturnsAsync(Result<IEnumerable<PfArchetype>>.Success(archetypes));

        // Act
        var result = await _archetypeService.GetAvailableArchetypesAsync(character);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(a => a.Name == "Barbarian");
        result.Value.Should().Contain(a => a.Name == "Wizard");
    }

    [Fact]
    public async Task ValidateArchetypeSelectionAsync_MeetsPrerequisites_ReturnsValid()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.AbilityScores.Strength = 14; // Meets Barbarian prerequisite

        var barbarian = CreateBarbarianArchetype();
        _mockRepository.Setup(r => r.GetByIdAsync("barbarian"))
            .ReturnsAsync(Result<PfArchetype>.Success(barbarian));

        // Act
        var result = await _archetypeService.ValidateArchetypeSelectionAsync(character, "barbarian");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty(); // No validation issues
    }

    [Fact]
    public async Task ValidateArchetypeSelectionAsync_FailsPrerequisites_ReturnsErrors()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.AbilityScores.Strength = 10; // Fails Barbarian prerequisite (needs 13+)

        var barbarian = CreateBarbarianArchetype();
        _mockRepository.Setup(r => r.GetByIdAsync("barbarian"))
            .ReturnsAsync(Result<PfArchetype>.Success(barbarian));

        // Act
        var result = await _archetypeService.ValidateArchetypeSelectionAsync(character, "barbarian");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().Contain(issue => 
            issue.Category == "Prerequisites" && 
            issue.Message.Contains("Strength"));
    }

    [Fact]
    public async Task ValidateArchetypeSelectionAsync_NonexistentArchetype_ReturnsError()
    {
        // Arrange
        var character = CreateTestCharacter();
        _mockRepository.Setup(r => r.GetByIdAsync("nonexistent"))
            .ReturnsAsync(Result<PfArchetype>.Failure(GeneralErrors.NotFound));

        // Act
        var result = await _archetypeService.ValidateArchetypeSelectionAsync(character, "nonexistent");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetArchetypeFeatsByLevelAsync_ValidArchetype_ReturnsFeats()
    {
        // Arrange
        var barbarian = CreateBarbarianArchetype();
        _mockRepository.Setup(r => r.GetByIdAsync("barbarian"))
            .ReturnsAsync(Result<PfArchetype>.Success(barbarian));

        // Act
        var result = await _archetypeService.GetArchetypeFeatsByLevelAsync("barbarian", 2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().Contain(feat => feat.Name == "Barbarian Dedication");
    }

    [Fact]
    public async Task GetArchetypeFeatsByLevelAsync_HigherLevel_ReturnsMultipleFeats()
    {
        // Arrange
        var barbarian = CreateBarbarianArchetype();
        _mockRepository.Setup(r => r.GetByIdAsync("barbarian"))
            .ReturnsAsync(Result<PfArchetype>.Success(barbarian));

        // Act
        var result = await _archetypeService.GetArchetypeFeatsByLevelAsync("barbarian", 6);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCountGreaterThan(1);
        result.Value.Should().Contain(feat => feat.Level <= 6);
    }

    [Fact]
    public async Task ValidateMulticlassProgressionAsync_ValidProgression_ReturnsValid()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Level = 4;

        var selectedArchetypes = new List<string> { "barbarian" };
        var barbarian = CreateBarbarianArchetype();

        _mockRepository.Setup(r => r.GetByIdAsync("barbarian"))
            .ReturnsAsync(Result<PfArchetype>.Success(barbarian));

        // Act
        var result = await _archetypeService.ValidateMulticlassProgressionAsync(character, selectedArchetypes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateMulticlassProgressionAsync_TooManyArchetypes_ReturnsWarning()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Level = 4;

        var selectedArchetypes = new List<string> { "barbarian", "wizard", "fighter" }; // Too many for low level

        var barbarian = CreateBarbarianArchetype();
        var wizard = CreateWizardArchetype();
        var fighter = CreateFighterArchetype();

        _mockRepository.Setup(r => r.GetByIdAsync("barbarian"))
            .ReturnsAsync(Result<PfArchetype>.Success(barbarian));
        _mockRepository.Setup(r => r.GetByIdAsync("wizard"))
            .ReturnsAsync(Result<PfArchetype>.Success(wizard));
        _mockRepository.Setup(r => r.GetByIdAsync("fighter"))
            .ReturnsAsync(Result<PfArchetype>.Success(fighter));

        // Act
        var result = await _archetypeService.ValidateMulticlassProgressionAsync(character, selectedArchetypes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(issue => 
            issue.Category == "Multiclass" && 
            issue.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public async Task CalculateSpellcastingProgressionAsync_WizardArchetype_ReturnsSpellProgression()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Level = 8;

        var wizard = CreateWizardArchetype();
        _mockRepository.Setup(r => r.GetByIdAsync("wizard"))
            .ReturnsAsync(Result<PfArchetype>.Success(wizard));

        // Act
        var result = await _archetypeService.CalculateSpellcastingProgressionAsync(character, "wizard");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MaxSpellLevel.Should().BeGreaterThan(0);
        result.Value.SpellsPerDay.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CalculateSpellcastingProgressionAsync_NonSpellcaster_ReturnsNull()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Level = 8;

        var barbarian = CreateBarbarianArchetype();
        _mockRepository.Setup(r => r.GetByIdAsync("barbarian"))
            .ReturnsAsync(Result<PfArchetype>.Success(barbarian));

        // Act
        var result = await _archetypeService.CalculateSpellcastingProgressionAsync(character, "barbarian");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    private CalculatedCharacter CreateTestCharacter()
    {
        return new CalculatedCharacter
        {
            Id = Guid.NewGuid(),
            Level = 1,
            ClassName = "Fighter",
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 16,
                ["Dexterity"] = 14,
                ["Constitution"] = 13,
                ["Intelligence"] = 12,
                ["Wisdom"] = 10,
                ["Charisma"] = 8
            },
            Proficiencies = new Dictionary<string, ProficiencyRank>
            {
                ["Simple Weapons"] = ProficiencyRank.Trained,
                ["Martial Weapons"] = ProficiencyRank.Trained
            },
            AvailableFeats = new List<string>(),
            ValidationIssues = new List<ValidationIssue>()
        };
    }

    private PfArchetype CreateBarbarianArchetype()
    {
        return new PfArchetype
        {
            Id = "barbarian",
            Name = "Barbarian",
            Type = ArchetypeType.Multiclass,
            Description = "Barbarian multiclass archetype",
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "13" }
            },
            Feats = new List<PfArchetypeFeat>
            {
                new() { 
                    Name = "Barbarian Dedication", 
                    Level = 2, 
                    Description = "Gain barbarian abilities",
                    Prerequisites = new List<PfPrerequisite>()
                },
                new() { 
                    Name = "Barbarian Resiliency", 
                    Level = 4, 
                    Description = "Gain barbarian resilience",
                    Prerequisites = new List<PfPrerequisite>()
                },
                new() { 
                    Name = "Basic Fury", 
                    Level = 6, 
                    Description = "Gain basic barbarian feat",
                    Prerequisites = new List<PfPrerequisite>()
                }
            },
            SpellcastingProgression = null
        };
    }

    private PfArchetype CreateWizardArchetype()
    {
        return new PfArchetype
        {
            Id = "wizard",
            Name = "Wizard",
            Type = ArchetypeType.Multiclass,
            Description = "Wizard multiclass archetype",
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Intelligence", Operator = ">=", Value = "13" }
            },
            Feats = new List<PfArchetypeFeat>
            {
                new() { 
                    Name = "Wizard Dedication", 
                    Level = 2, 
                    Description = "Gain wizard spellcasting",
                    Prerequisites = new List<PfPrerequisite>()
                }
            },
            SpellcastingProgression = new PfSpellcastingProgression
            {
                Tradition = "Arcane",
                Type = SpellcastingType.Prepared,
                KeyAbility = "Intelligence",
                SpellsPerDay = new Dictionary<int, Dictionary<int, int>>
                {
                    [2] = new() { [1] = 1 }, // Level 2: 1 first-level spell
                    [4] = new() { [1] = 2 }, // Level 4: 2 first-level spells
                    [6] = new() { [1] = 2, [2] = 1 }, // Level 6: 2 first, 1 second
                    [8] = new() { [1] = 3, [2] = 2 }, // Level 8: 3 first, 2 second
                }
            }
        };
    }

    private PfArchetype CreateFighterArchetype()
    {
        return new PfArchetype
        {
            Id = "fighter",
            Name = "Fighter",
            Type = ArchetypeType.Multiclass,
            Description = "Fighter multiclass archetype",
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "13" }
            },
            Feats = new List<PfArchetypeFeat>
            {
                new() { 
                    Name = "Fighter Dedication", 
                    Level = 2, 
                    Description = "Gain fighter abilities",
                    Prerequisites = new List<PfPrerequisite>()
                }
            },
            SpellcastingProgression = null
        };
    }
}