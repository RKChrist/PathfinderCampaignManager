using Xunit;
using Moq;
using FluentAssertions;
using PathfinderCampaignManager.Infrastructure.Validation;
using PathfinderCampaignManager.Domain.Validation;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Common;

namespace PathfinderCampaignManager.Domain.Tests;

public class ValidationServiceTests
{
    private readonly Mock<IPathfinderDataRepository> _mockDataRepository;
    private readonly Mock<IArchetypeService> _mockArchetypeService;
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _mockDataRepository = new Mock<IPathfinderDataRepository>();
        _mockArchetypeService = new Mock<IArchetypeService>();
        _validationService = new ValidationService(_mockDataRepository.Object, _mockArchetypeService.Object);
    }

    [Fact]
    public async Task ValidateCharacterAsync_ValidCharacter_ReturnsValidReport()
    {
        // Arrange
        var character = CreateValidCharacter();

        // Act
        var result = await _validationService.ValidateCharacterAsync(character);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.ValidatedEntityType.Should().Be(nameof(PfCharacter));
        result.Value.ValidatedEntityId.Should().Be(character.Id.ToString());
    }

    [Fact]
    public async Task ValidateCharacterAsync_MissingName_ReturnsError()
    {
        // Arrange
        var character = CreateValidCharacter();
        character.Name = "";

        // Act
        var result = await _validationService.ValidateCharacterAsync(character);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.HasCriticalIssues.Should().BeTrue();
        result.Value.Issues.Should().Contain(i => i.Category == "Character" && i.Message.Contains("name"));
    }

    [Fact]
    public async Task ValidateCharacterAsync_InvalidLevel_ReturnsError()
    {
        // Arrange
        var character = CreateValidCharacter();
        character.Level = 25; // Invalid level (max is 20)

        // Act
        var result = await _validationService.ValidateCharacterAsync(character);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Issues.Should().Contain(i => 
            i.Category == "Character" && 
            i.Message.Contains("level") && 
            i.Message.Contains("20"));
    }

    [Fact]
    public async Task ValidateAbilityScoreArrayAsync_ValidScores_ReturnsValid()
    {
        // Arrange
        var abilityScores = new Dictionary<string, int>
        {
            ["Strength"] = 16,
            ["Dexterity"] = 14,
            ["Constitution"] = 13,
            ["Intelligence"] = 12,
            ["Wisdom"] = 10,
            ["Charisma"] = 8
        };

        // Act
        var result = await _validationService.ValidateAbilityScoreArrayAsync(abilityScores, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAbilityScoreArrayAsync_MissingAbility_ReturnsError()
    {
        // Arrange
        var abilityScores = new Dictionary<string, int>
        {
            ["Strength"] = 16,
            ["Dexterity"] = 14,
            ["Constitution"] = 13,
            ["Intelligence"] = 12,
            ["Wisdom"] = 10
            // Missing Charisma
        };

        // Act
        var result = await _validationService.ValidateAbilityScoreArrayAsync(abilityScores, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Issues.Should().Contain(i => 
            i.Category == "AbilityScores" && 
            i.Message.Contains("Charisma"));
    }

    [Fact]
    public async Task ValidateAbilityScoreArrayAsync_ScoreTooLow_ReturnsError()
    {
        // Arrange
        var abilityScores = new Dictionary<string, int>
        {
            ["Strength"] = 0, // Too low
            ["Dexterity"] = 14,
            ["Constitution"] = 13,
            ["Intelligence"] = 12,
            ["Wisdom"] = 10,
            ["Charisma"] = 8
        };

        // Act
        var result = await _validationService.ValidateAbilityScoreArrayAsync(abilityScores, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Issues.Should().Contain(i => 
            i.Category == "AbilityScores" && 
            i.Message.Contains("Strength") &&
            i.Message.Contains("cannot be less than 1"));
    }

    [Fact]
    public async Task ValidateAbilityScoreArrayAsync_ScoreTooHigh_ReturnsError()
    {
        // Arrange
        var abilityScores = new Dictionary<string, int>
        {
            ["Strength"] = 35, // Too high
            ["Dexterity"] = 14,
            ["Constitution"] = 13,
            ["Intelligence"] = 12,
            ["Wisdom"] = 10,
            ["Charisma"] = 8
        };

        // Act
        var result = await _validationService.ValidateAbilityScoreArrayAsync(abilityScores, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Issues.Should().Contain(i => 
            i.Category == "AbilityScores" && 
            i.Message.Contains("Strength") &&
            i.Message.Contains("cannot exceed 30"));
    }

    [Fact]
    public async Task ValidateAbilityScoreArrayAsync_UnusuallyLowScore_ReturnsWarning()
    {
        // Arrange
        var abilityScores = new Dictionary<string, int>
        {
            ["Strength"] = 6, // Unusually low for level 1
            ["Dexterity"] = 14,
            ["Constitution"] = 13,
            ["Intelligence"] = 12,
            ["Wisdom"] = 10,
            ["Charisma"] = 8
        };

        // Act
        var result = await _validationService.ValidateAbilityScoreArrayAsync(abilityScores, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HasWarnings.Should().BeTrue();
        result.Value.Warnings.Should().Contain(w => 
            w.Category == "AbilityScores" && 
            w.Message.Contains("Strength") &&
            w.Message.Contains("unusually low"));
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_MeetsAllPrerequisites_ReturnsValid()
    {
        // Arrange
        var character = CreateMockCalculatedCharacter();
        character.Setup(c => c.AbilityScores).Returns(new Dictionary<string, int>
        {
            ["Strength"] = 16,
            ["Dexterity"] = 14
        });
        character.Setup(c => c.Level).Returns(5);
        character.Setup(c => c.Proficiencies).Returns(new Dictionary<string, ProficiencyRank>
        {
            ["Athletics"] = ProficiencyRank.Trained
        });
        character.Setup(c => c.AvailableFeats).Returns(new List<string> { "Power Attack" });

        var prerequisites = new List<PfPrerequisite>
        {
            new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "13" },
            new() { Type = "Level", Value = "3" },
            new() { Type = "Skill", Target = "Athletics", Value = "Trained" },
            new() { Type = "Feat", Target = "Power Attack" }
        };

        // Act
        var result = await _validationService.ValidatePrerequisitesAsync(prerequisites, character.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Issues.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidatePrerequisitesAsync_FailsAbilityScore_ReturnsError()
    {
        // Arrange
        var character = CreateMockCalculatedCharacter();
        character.Setup(c => c.AbilityScores).Returns(new Dictionary<string, int>
        {
            ["Strength"] = 10 // Too low
        });

        var prerequisites = new List<PfPrerequisite>
        {
            new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "13" }
        };

        // Act
        var result = await _validationService.ValidatePrerequisitesAsync(prerequisites, character.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Issues.Should().Contain(i => 
            i.Category == "Prerequisite" && 
            i.Message.Contains("not met"));
    }

    [Fact]
    public async Task ValidateEquipmentLoadAsync_WithinLimits_ReturnsValid()
    {
        // Arrange
        var character = CreateMockCalculatedCharacter();
        character.Setup(c => c.IsEncumbered).Returns(false);
        character.Setup(c => c.CurrentBulk).Returns(3);
        character.Setup(c => c.BulkLimit).Returns(5);

        // Act
        var result = await _validationService.ValidateEquipmentLoadAsync(character.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateEquipmentLoadAsync_Encumbered_ReturnsWarning()
    {
        // Arrange
        var character = CreateMockCalculatedCharacter();
        character.Setup(c => c.IsEncumbered).Returns(true);
        character.Setup(c => c.CurrentBulk).Returns(7);
        character.Setup(c => c.BulkLimit).Returns(5);

        // Act
        var result = await _validationService.ValidateEquipmentLoadAsync(character.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HasWarnings.Should().BeTrue();
        result.Value.Warnings.Should().Contain(w => 
            w.Category == "Equipment" && 
            w.Message.Contains("encumbered"));
    }

    [Fact]
    public async Task ValidateEquipmentLoadAsync_OverMaximum_ReturnsError()
    {
        // Arrange
        var character = CreateMockCalculatedCharacter();
        character.Setup(c => c.IsEncumbered).Returns(true);
        character.Setup(c => c.CurrentBulk).Returns(12);
        character.Setup(c => c.BulkLimit).Returns(5);

        // Act
        var result = await _validationService.ValidateEquipmentLoadAsync(character.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HasCriticalIssues.Should().BeTrue();
        result.Value.Issues.Should().Contain(i => 
            i.Category == "Equipment" && 
            i.Message.Contains("over maximum bulk limit"));
    }

    [Fact]
    public async Task ValidateEquipmentLoadAsync_NearLimit_ReturnsSuggestion()
    {
        // Arrange
        var character = CreateMockCalculatedCharacter();
        character.Setup(c => c.IsEncumbered).Returns(false);
        character.Setup(c => c.CurrentBulk).Returns(4.5); // 90% of limit
        character.Setup(c => c.BulkLimit).Returns(5);

        // Act
        var result = await _validationService.ValidateEquipmentLoadAsync(character.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Should().Contain(s => 
            s.Category == "Equipment" && 
            s.Suggestion.Contains("reducing carried equipment"));
    }

    private PfCharacter CreateValidCharacter()
    {
        return new PfCharacter
        {
            Id = Guid.NewGuid(),
            Name = "Test Character",
            Level = 1,
            Ancestry = "Human",
            Background = "Acolyte",
            ClassName = "Fighter",
            AbilityScores = new PfAbilityScores
            {
                Strength = 16,
                Dexterity = 14,
                Constitution = 13,
                Intelligence = 12,
                Wisdom = 10,
                Charisma = 8
            },
            Skills = new List<PfCharacterSkill>
            {
                new() { SkillName = "Athletics", Proficiency = new PfProficiency { Rank = ProficiencyRank.Trained } }
            },
            Equipment = new List<string> { "Longsword", "Scale Mail", "Adventurer's Pack" }
        };
    }

    private Mock<ICalculatedCharacter> CreateMockCalculatedCharacter()
    {
        var mock = new Mock<ICalculatedCharacter>();
        mock.Setup(c => c.Id).Returns(Guid.NewGuid());
        mock.Setup(c => c.Level).Returns(1);
        mock.Setup(c => c.AbilityScores).Returns(new Dictionary<string, int>());
        mock.Setup(c => c.Proficiencies).Returns(new Dictionary<string, ProficiencyRank>());
        mock.Setup(c => c.AvailableFeats).Returns(new List<string>());
        mock.Setup(c => c.ValidationIssues).Returns(new List<ValidationIssue>());
        mock.Setup(c => c.IsEncumbered).Returns(false);
        mock.Setup(c => c.CurrentBulk).Returns(0);
        mock.Setup(c => c.BulkLimit).Returns(5);
        mock.Setup(c => c.HitPoints).Returns(10);
        mock.Setup(c => c.ArmorClass).Returns(16);
        return mock;
    }
}