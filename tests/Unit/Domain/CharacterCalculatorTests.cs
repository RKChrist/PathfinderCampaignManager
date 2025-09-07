using Xunit;
using FluentAssertions;
using PathfinderCampaignManager.Infrastructure.Services;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Domain.Tests;

public class CharacterCalculatorTests
{
    private readonly CharacterCalculator _calculator;

    public CharacterCalculatorTests()
    {
        _calculator = new CharacterCalculator();
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(8, 0)]
    [InlineData(9, 0)]
    [InlineData(10, 0)]
    [InlineData(11, 0)]
    [InlineData(12, 1)]
    [InlineData(13, 1)]
    [InlineData(14, 2)]
    [InlineData(15, 2)]
    [InlineData(16, 3)]
    [InlineData(17, 3)]
    [InlineData(18, 4)]
    [InlineData(20, 5)]
    [InlineData(30, 10)]
    public void CalculateModifier_VariousScores_ReturnsCorrectModifier(int score, int expectedModifier)
    {
        // Act
        var result = _calculator.CalculateModifier(score);

        // Assert
        result.Should().Be(expectedModifier);
    }

    [Fact]
    public void CalculateHitPoints_Level1Fighter_ReturnsCorrectHP()
    {
        // Arrange
        var character = CreateFighterCharacter();
        character.Level = 1;

        // Act
        var result = _calculator.CalculateHitPoints(character);

        // Assert
        // Fighter: 10 base + Con modifier (1) = 11
        result.Should().Be(11);
    }

    [Fact]
    public void CalculateHitPoints_Level3Fighter_ReturnsCorrectHP()
    {
        // Arrange
        var character = CreateFighterCharacter();
        character.Level = 3;

        // Act
        var result = _calculator.CalculateHitPoints(character);

        // Assert
        // Fighter: 10 base + 2*(6 + Con mod) + Con modifier = 10 + 2*7 + 1 = 25
        result.Should().Be(25);
    }

    [Fact]
    public void CalculateArmorClass_NoArmor_ReturnsBaseAC()
    {
        // Arrange
        var character = CreateFighterCharacter();
        character.Equipment.Clear(); // No armor

        // Act
        var result = _calculator.CalculateArmorClass(character);

        // Assert
        // Base AC: 10 + Dex modifier (2) = 12
        result.Should().Be(12);
    }

    [Fact]
    public void CalculateArmorClass_WithLeatherArmor_ReturnsCorrectAC()
    {
        // Arrange
        var character = CreateFighterCharacter();
        character.Equipment.Add("Leather Armor"); // +1 AC, max dex +4

        // Act
        var result = _calculator.CalculateArmorClass(character);

        // Assert
        // Leather Armor: 11 + Dex modifier (2) = 13
        result.Should().Be(13);
    }

    [Fact]
    public void CalculateProficiencyBonus_Level1Trained_ReturnsCorrectBonus()
    {
        // Arrange
        var level = 1;
        var rank = ProficiencyRank.Trained;

        // Act
        var result = _calculator.CalculateProficiencyBonus(level, rank);

        // Assert
        // Level 1 + Trained (+2) = 3
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateProficiencyBonus_Level10Expert_ReturnsCorrectBonus()
    {
        // Arrange
        var level = 10;
        var rank = ProficiencyRank.Expert;

        // Act
        var result = _calculator.CalculateProficiencyBonus(level, rank);

        // Assert
        // Level 10 + Expert (+4) = 14
        result.Should().Be(14);
    }

    [Fact]
    public void CalculateProficiencyBonus_Level20Legendary_ReturnsCorrectBonus()
    {
        // Arrange
        var level = 20;
        var rank = ProficiencyRank.Legendary;

        // Act
        var result = _calculator.CalculateProficiencyBonus(level, rank);

        // Assert
        // Level 20 + Legendary (+8) = 28
        result.Should().Be(28);
    }

    [Fact]
    public void CalculateSkillBonus_TrainedAthletics_ReturnsCorrectBonus()
    {
        // Arrange
        var character = CreateFighterCharacter();
        character.Skills.Add(new PfCharacterSkill
        {
            SkillName = "Athletics",
            Proficiency = new PfProficiency
            {
                Name = "Athletics",
                Rank = ProficiencyRank.Trained,
                Type = ProficiencyType.Skill
            }
        });

        // Act
        var result = _calculator.CalculateSkillBonus(character, "Athletics");

        // Assert
        // Strength modifier (3) + Level (1) + Trained (2) = 6
        result.Should().Be(6);
    }

    [Fact]
    public void CalculateSkillBonus_UntrainedSkill_ReturnsCorrectBonus()
    {
        // Arrange
        var character = CreateFighterCharacter();

        // Act
        var result = _calculator.CalculateSkillBonus(character, "Medicine"); // Wisdom-based, untrained

        // Assert
        // Wisdom modifier (0) + Level (1) + Untrained (0) = 1
        result.Should().Be(1);
    }

    [Fact]
    public void CalculateAttackBonus_WeaponProficiency_ReturnsCorrectBonus()
    {
        // Arrange
        var character = CreateFighterCharacter();
        var weapon = new PfWeapon
        {
            Name = "Longsword",
            Category = "Martial",
            Damage = "1d8",
            DamageType = "Slashing",
            Traits = new List<string> { "Versatile P" },
            Range = 0
        };

        // Act
        var result = _calculator.CalculateAttackBonus(character, weapon);

        // Assert
        // Strength modifier (3) + Level (1) + Martial Weapons (2) = 6
        result.Should().Be(6);
    }

    [Fact]
    public void CalculateSpellAttackBonus_WizardSpells_ReturnsCorrectBonus()
    {
        // Arrange
        var character = CreateWizardCharacter();

        // Act
        var result = _calculator.CalculateSpellAttackBonus(character, "Arcane");

        // Assert
        // Intelligence modifier (4) + Level (1) + Spell Attack (2) = 7
        result.Should().Be(7);
    }

    [Fact]
    public void CalculateSpellDC_WizardSpells_ReturnsCorrectDC()
    {
        // Arrange
        var character = CreateWizardCharacter();

        // Act
        var result = _calculator.CalculateSpellDC(character, "Arcane");

        // Assert
        // 10 + Intelligence modifier (4) + Level (1) + Spell DC (2) = 17
        result.Should().Be(17);
    }

    [Fact]
    public void CalculateSavingThrow_FortitudeSave_ReturnsCorrectBonus()
    {
        // Arrange
        var character = CreateFighterCharacter();

        // Act
        var result = _calculator.CalculateSavingThrow(character, "Fortitude");

        // Assert
        // Constitution modifier (1) + Level (1) + Expert (4) = 6
        result.Should().Be(6);
    }

    [Fact]
    public void CalculateBulkCapacity_StrengthBased_ReturnsCorrectCapacity()
    {
        // Arrange
        var character = CreateFighterCharacter(); // Strength 16

        // Act
        var result = _calculator.CalculateBulkCapacity(character);

        // Assert
        // Base 5 + Strength modifier (3) = 8
        result.Should().Be(8);
    }

    [Fact]
    public void CalculateSpeed_BaseSpeed_ReturnsCorrectSpeed()
    {
        // Arrange
        var character = CreateHumanFighter(); // Human base speed 25

        // Act
        var result = _calculator.CalculateSpeed(character);

        // Assert
        result.Should().Be(25);
    }

    [Fact]
    public void CalculateInitiative_DexterityBased_ReturnsCorrectBonus()
    {
        // Arrange
        var character = CreateFighterCharacter();

        // Act
        var result = _calculator.CalculateInitiative(character);

        // Assert
        // Dexterity modifier (2) + Level (1) + Perception rank (2) = 5
        result.Should().Be(5);
    }

    private PfCharacter CreateFighterCharacter()
    {
        return new PfCharacter
        {
            Id = Guid.NewGuid(),
            Name = "Test Fighter",
            Level = 1,
            ClassName = "Fighter",
            Ancestry = "Human",
            Background = "Soldier",
            AbilityScores = new PfAbilityScores
            {
                Strength = 16,    // +3 modifier
                Dexterity = 14,   // +2 modifier
                Constitution = 13, // +1 modifier
                Intelligence = 10, // +0 modifier
                Wisdom = 12,      // +1 modifier
                Charisma = 8      // -1 modifier
            },
            Skills = new List<PfCharacterSkill>(),
            Equipment = new List<string> { "Scale Mail", "Longsword", "Shield" }
        };
    }

    private PfCharacter CreateWizardCharacter()
    {
        return new PfCharacter
        {
            Id = Guid.NewGuid(),
            Name = "Test Wizard",
            Level = 1,
            ClassName = "Wizard",
            Ancestry = "Elf",
            Background = "Scholar",
            AbilityScores = new PfAbilityScores
            {
                Strength = 8,     // -1 modifier
                Dexterity = 14,   // +2 modifier
                Constitution = 12, // +1 modifier
                Intelligence = 18, // +4 modifier
                Wisdom = 13,      // +1 modifier
                Charisma = 10     // +0 modifier
            },
            Skills = new List<PfCharacterSkill>(),
            Equipment = new List<string> { "Spellbook", "Dagger", "Scholar's Pack" }
        };
    }

    private PfCharacter CreateHumanFighter()
    {
        var fighter = CreateFighterCharacter();
        fighter.Ancestry = "Human";
        return fighter;
    }
}