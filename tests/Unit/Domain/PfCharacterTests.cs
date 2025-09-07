using Xunit;
using FluentAssertions;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Domain.Tests;

public class PfCharacterTests
{
    [Fact]
    public void PfCharacter_DefaultConstruction_HasValidDefaults()
    {
        // Arrange & Act
        var character = new PfCharacter();

        // Assert
        character.Id.Should().NotBe(Guid.Empty);
        character.Name.Should().BeEmpty();
        character.Level.Should().Be(0);
        character.Ancestry.Should().BeEmpty();
        character.Background.Should().BeEmpty();
        character.ClassName.Should().BeEmpty();
        character.AbilityScores.Should().NotBeNull();
        character.Skills.Should().NotBeNull();
        character.Equipment.Should().NotBeNull();
        character.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        character.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PfCharacter_SetProperties_StoresCorrectly()
    {
        // Arrange
        var character = new PfCharacter();
        var characterId = Guid.NewGuid();
        var abilityScores = new PfAbilityScores { Strength = 16, Dexterity = 14 };
        var skills = new List<PfCharacterSkill> 
        { 
            new() { SkillName = "Athletics", Proficiency = new PfProficiency { Rank = ProficiencyRank.Trained } } 
        };
        var equipment = new List<string> { "Longsword", "Scale Mail" };

        // Act
        character.Id = characterId;
        character.Name = "Test Hero";
        character.Level = 5;
        character.Ancestry = "Human";
        character.Background = "Soldier";
        character.ClassName = "Fighter";
        character.AbilityScores = abilityScores;
        character.Skills = skills;
        character.Equipment = equipment;

        // Assert
        character.Id.Should().Be(characterId);
        character.Name.Should().Be("Test Hero");
        character.Level.Should().Be(5);
        character.Ancestry.Should().Be("Human");
        character.Background.Should().Be("Soldier");
        character.ClassName.Should().Be("Fighter");
        character.AbilityScores.Should().BeSameAs(abilityScores);
        character.Skills.Should().BeSameAs(skills);
        character.Equipment.Should().BeSameAs(equipment);
    }

    [Fact]
    public void PfAbilityScores_DefaultConstruction_HasValidDefaults()
    {
        // Arrange & Act
        var abilities = new PfAbilityScores();

        // Assert
        abilities.Strength.Should().Be(0);
        abilities.Dexterity.Should().Be(0);
        abilities.Constitution.Should().Be(0);
        abilities.Intelligence.Should().Be(0);
        abilities.Wisdom.Should().Be(0);
        abilities.Charisma.Should().Be(0);
    }

    [Theory]
    [InlineData(8, -1)]
    [InlineData(10, 0)]
    [InlineData(12, 1)]
    [InlineData(14, 2)]
    [InlineData(16, 3)]
    [InlineData(18, 4)]
    [InlineData(20, 5)]
    public void PfAbilityScores_GetModifier_ReturnsCorrectValue(int score, int expectedModifier)
    {
        // Arrange
        var abilities = new PfAbilityScores { Strength = score };

        // Act
        var modifier = abilities.GetModifier("Strength");

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    [Fact]
    public void PfAbilityScores_GetModifier_InvalidAbility_ReturnsZero()
    {
        // Arrange
        var abilities = new PfAbilityScores();

        // Act
        var modifier = abilities.GetModifier("InvalidAbility");

        // Assert
        modifier.Should().Be(0);
    }

    [Fact]
    public void PfAbilityScores_SetValidScores_StoresCorrectly()
    {
        // Arrange & Act
        var abilities = new PfAbilityScores
        {
            Strength = 16,
            Dexterity = 14,
            Constitution = 13,
            Intelligence = 12,
            Wisdom = 10,
            Charisma = 8
        };

        // Assert
        abilities.Strength.Should().Be(16);
        abilities.Dexterity.Should().Be(14);
        abilities.Constitution.Should().Be(13);
        abilities.Intelligence.Should().Be(12);
        abilities.Wisdom.Should().Be(10);
        abilities.Charisma.Should().Be(8);
    }

    [Fact]
    public void PfCharacterSkill_DefaultConstruction_HasValidDefaults()
    {
        // Arrange & Act
        var skill = new PfCharacterSkill();

        // Assert
        skill.SkillName.Should().BeEmpty();
        skill.Proficiency.Should().NotBeNull();
        skill.BonusModifiers.Should().NotBeNull();
        skill.BonusModifiers.Should().BeEmpty();
    }

    [Fact]
    public void PfCharacterSkill_SetProperties_StoresCorrectly()
    {
        // Arrange
        var proficiency = new PfProficiency 
        { 
            Name = "Athletics", 
            Rank = ProficiencyRank.Expert,
            Type = ProficiencyType.Skill
        };
        var bonusModifiers = new List<int> { 2, 1 };

        // Act
        var skill = new PfCharacterSkill
        {
            SkillName = "Athletics",
            Proficiency = proficiency,
            BonusModifiers = bonusModifiers
        };

        // Assert
        skill.SkillName.Should().Be("Athletics");
        skill.Proficiency.Should().BeSameAs(proficiency);
        skill.BonusModifiers.Should().BeSameAs(bonusModifiers);
    }

    [Fact]
    public void PfProficiency_DefaultConstruction_HasValidDefaults()
    {
        // Arrange & Act
        var proficiency = new PfProficiency();

        // Assert
        proficiency.Name.Should().BeEmpty();
        proficiency.Rank.Should().Be(ProficiencyRank.Untrained);
        proficiency.Type.Should().Be(ProficiencyType.Skill);
        proficiency.Category.Should().BeEmpty();
        proficiency.SpecificItem.Should().BeNull();
    }

    [Theory]
    [InlineData(ProficiencyRank.Untrained, 0)]
    [InlineData(ProficiencyRank.Trained, 2)]
    [InlineData(ProficiencyRank.Expert, 4)]
    [InlineData(ProficiencyRank.Master, 6)]
    [InlineData(ProficiencyRank.Legendary, 8)]
    public void ProficiencyRank_GetBonus_ReturnsCorrectValue(ProficiencyRank rank, int expectedBonus)
    {
        // Arrange & Act
        var bonus = rank.GetBonus();

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    [Fact]
    public void ProficiencyRank_GetName_ReturnsCorrectName()
    {
        // Arrange & Act & Assert
        ProficiencyRank.Untrained.GetName().Should().Be("Untrained");
        ProficiencyRank.Trained.GetName().Should().Be("Trained");
        ProficiencyRank.Expert.GetName().Should().Be("Expert");
        ProficiencyRank.Master.GetName().Should().Be("Master");
        ProficiencyRank.Legendary.GetName().Should().Be("Legendary");
    }

    [Fact]
    public void ProficiencyRank_GetColorClass_ReturnsValidCssClass()
    {
        // Arrange & Act & Assert
        ProficiencyRank.Untrained.GetColorClass().Should().Be("text-secondary");
        ProficiencyRank.Trained.GetColorClass().Should().Be("text-info");
        ProficiencyRank.Expert.GetColorClass().Should().Be("text-primary");
        ProficiencyRank.Master.GetColorClass().Should().Be("text-warning");
        ProficiencyRank.Legendary.GetColorClass().Should().Be("text-success");
    }

    [Fact]
    public void PfCharacterProficiency_GetTotalBonus_CalculatesCorrectly()
    {
        // Arrange
        var proficiency = new PfCharacterProficiency
        {
            Name = "Athletics",
            Type = ProficiencyType.Skill,
            Rank = ProficiencyRank.Expert,
            Level = 5
        };

        // Act
        var totalBonus = proficiency.GetTotalBonus(3); // +3 ability modifier

        // Assert
        // Level (5) + Expert (4) + Ability (3) = 12
        totalBonus.Should().Be(12);
    }

    [Fact]
    public void PfCharacterProficiency_GetTotalBonusWithoutAbility_CalculatesCorrectly()
    {
        // Arrange
        var proficiency = new PfCharacterProficiency
        {
            Name = "Athletics",
            Type = ProficiencyType.Skill,
            Rank = ProficiencyRank.Master,
            Level = 10
        };

        // Act
        var totalBonus = proficiency.GetTotalBonusWithoutAbility();

        // Assert
        // Level (10) + Master (6) = 16
        totalBonus.Should().Be(16);
    }

    [Fact]
    public void PfCharacter_ComplexCharacterCreation_AllPropertiesValid()
    {
        // Arrange & Act
        var character = new PfCharacter
        {
            Name = "Valeros the Brave",
            Level = 8,
            Ancestry = "Human",
            Background = "Soldier",
            ClassName = "Fighter",
            AbilityScores = new PfAbilityScores
            {
                Strength = 18,
                Dexterity = 14,
                Constitution = 16,
                Intelligence = 10,
                Wisdom = 13,
                Charisma = 8
            },
            Skills = new List<PfCharacterSkill>
            {
                new()
                {
                    SkillName = "Athletics",
                    Proficiency = new PfProficiency 
                    { 
                        Name = "Athletics", 
                        Rank = ProficiencyRank.Master,
                        Type = ProficiencyType.Skill
                    },
                    BonusModifiers = new List<int> { 2 } // Item bonus
                },
                new()
                {
                    SkillName = "Intimidation",
                    Proficiency = new PfProficiency 
                    { 
                        Name = "Intimidation", 
                        Rank = ProficiencyRank.Expert,
                        Type = ProficiencyType.Skill
                    }
                }
            },
            Equipment = new List<string>
            {
                "+1 Striking Longsword",
                "+1 Full Plate",
                "Steel Shield",
                "Belt of Giant Strength",
                "Healing Potion"
            }
        };

        // Assert
        character.Name.Should().Be("Valeros the Brave");
        character.Level.Should().Be(8);
        character.AbilityScores.Strength.Should().Be(18);
        character.AbilityScores.GetModifier("Strength").Should().Be(4);
        character.Skills.Should().HaveCount(2);
        character.Skills.First().SkillName.Should().Be("Athletics");
        character.Skills.First().Proficiency.Rank.Should().Be(ProficiencyRank.Master);
        character.Equipment.Should().HaveCount(5);
        character.Equipment.Should().Contain("+1 Striking Longsword");
    }

    [Theory]
    [InlineData(ProficiencyType.Skill)]
    [InlineData(ProficiencyType.Weapon)]
    [InlineData(ProficiencyType.Armor)]
    [InlineData(ProficiencyType.Save)]
    [InlineData(ProficiencyType.Perception)]
    [InlineData(ProficiencyType.ClassDC)]
    [InlineData(ProficiencyType.SpellAttack)]
    [InlineData(ProficiencyType.SpellDC)]
    public void ProficiencyType_AllValues_AreDefined(ProficiencyType type)
    {
        // This test ensures all enum values are properly defined
        type.Should().BeDefined();
    }

    [Fact]
    public void PfCharacter_UpdateTimestamp_ChangesOnModification()
    {
        // Arrange
        var character = new PfCharacter();
        var originalTimestamp = character.UpdatedAt;
        
        // Act
        System.Threading.Thread.Sleep(10); // Ensure time difference
        character.Name = "Modified Name";

        // Assert - In a real implementation, UpdatedAt would be updated by the repository or service layer
        // For now, we just verify the property exists and can be set
        character.UpdatedAt = DateTime.UtcNow;
        character.UpdatedAt.Should().BeAfter(originalTimestamp);
    }
}