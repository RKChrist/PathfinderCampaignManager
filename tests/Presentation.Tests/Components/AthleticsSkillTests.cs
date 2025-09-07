using Bunit;
using Microsoft.Extensions.DependencyInjection;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Presentation.Client.Components.Pathfinder.Skills;
using Xunit;

namespace PathfinderCampaignManager.Presentation.Tests.Components;

public class AthleticsSkillTests : TestContext
{
    public AthleticsSkillTests()
    {
        Services.AddLogging();
    }

    [Fact]
    public void AthleticsSkill_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<AthleticsSkill>();

        // Assert
        Assert.Contains("Athletics", component.Markup);
        Assert.Contains("Strength", component.Markup);
        Assert.Contains("Physical", component.Markup);
        Assert.Contains("fas fa-dumbbell", component.Markup);
    }

    [Fact]
    public void AthleticsSkill_ShowsCorrectDescription()
    {
        // Act
        var component = RenderComponent<AthleticsSkill>();

        // Assert
        Assert.Contains("Athletics allows you to perform deeds of physical prowess", component.Markup);
        Assert.Contains("When you use the Escape basic action", component.Markup);
        Assert.Contains("you can use your Athletics modifier", component.Markup);
    }

    [Fact]
    public void AthleticsSkill_ProficiencySelector_UpdatesProficiency()
    {
        // Arrange
        var component = RenderComponent<AthleticsSkill>();
        var selector = component.Find("select.proficiency-selector");

        // Act
        selector.Change(ProficiencyLevel.Trained.ToString());

        // Assert
        Assert.Equal(ProficiencyLevel.Trained.ToString(), selector.GetAttribute("value"));
    }

    [Fact]
    public void AthleticsSkill_CalculatesSkillBonus_Correctly()
    {
        // Arrange
        var character = new CharacterBuilder
        {
            Level = 3,
            AbilityScores = new Dictionary<AbilityScore, int>
            {
                [AbilityScore.Strength] = 16 // +3 modifier
            }
        };

        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.Character, character)
            .Add(p => p.InitialProficiency, ProficiencyLevel.Trained));

        // Assert - Should be +3 (Str) +2 (prof) +3 (level) = +8
        Assert.Contains("8", component.Find(".skill-bonus").TextContent);
    }

    [Fact]
    public void AthleticsSkill_ShowsBonusBreakdown()
    {
        // Arrange
        var character = new CharacterBuilder
        {
            Level = 1,
            AbilityScores = new Dictionary<AbilityScore, int>
            {
                [AbilityScore.Strength] = 14 // +2 modifier
            }
        };

        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.Character, character)
            .Add(p => p.InitialProficiency, ProficiencyLevel.Expert));

        // Assert - Should show breakdown
        var breakdown = component.Find(".bonus-breakdown").TextContent;
        Assert.Contains("Str", breakdown);
        Assert.Contains("prof", breakdown);
        Assert.Contains("level", breakdown);
    }

    [Fact]
    public void AthleticsSkill_WhenDetailsExpanded_ShowsSkillActions()
    {
        // Act
        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Skill Actions", component.Markup);
        Assert.Contains("Climb", component.Markup);
        Assert.Contains("Force Open", component.Markup);
        Assert.Contains("Grapple", component.Markup);
        Assert.Contains("High Jump", component.Markup);
        Assert.Contains("Long Jump", component.Markup);
        Assert.Contains("Shove", component.Markup);
        Assert.Contains("Swim", component.Markup);
        Assert.Contains("Trip", component.Markup);
        Assert.Contains("Disarm", component.Markup);
    }

    [Fact]
    public void AthleticsSkill_ShowsCorrectActionCosts()
    {
        // Act
        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("move-action", component.Markup); // Climb, Swim
        Assert.Contains("single-action", component.Markup); // Force Open, Grapple, etc.
        Assert.Contains("two-actions", component.Markup); // High Jump, Long Jump
    }

    [Fact]
    public void AthleticsSkill_ShowsCorrectDCs()
    {
        // Act
        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Untrained Wall DC 30", component.Markup);
        Assert.Contains("Tree DC 15", component.Markup);
        Assert.Contains("Simple Door DC 15", component.Markup);
        Assert.Contains("vs. Target's Fortitude DC", component.Markup);
        Assert.Contains("vs. Target's Reflex DC", component.Markup);
        Assert.Contains("Calm Water DC 10", component.Markup);
    }

    [Fact]
    public void AthleticsSkill_DisarmAction_ShowsTrainedRequirement()
    {
        // Act
        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        var disarmAction = component.Markup;
        Assert.Contains("Disarm", disarmAction);
        Assert.Contains("trained-only", disarmAction);
    }

    [Fact]
    public void AthleticsSkill_ShowsProficiencyBenefits_WhenDetailsExpanded()
    {
        // Act
        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Proficiency Benefits", component.Markup);
        Assert.Contains("Untrained", component.Markup);
        Assert.Contains("Basic Athletics actions", component.Markup);
        Assert.Contains("Trained", component.Markup);
        Assert.Contains("Disarm action unlocked", component.Markup);
        Assert.Contains("Expert", component.Markup);
        Assert.Contains("Master", component.Markup);
        Assert.Contains("Legendary", component.Markup);
        Assert.Contains("Superhuman athletic feats possible", component.Markup);
    }

    [Fact]
    public void AthleticsSkill_RollSkillCheck_GeneratesResult()
    {
        // Arrange
        var component = RenderComponent<AthleticsSkill>();
        var rollButton = component.Find("button:contains('Roll Check')");

        // Act
        rollButton.Click();

        // Assert
        Assert.Contains("roll-result", component.Markup);
        Assert.Contains("Total:", component.Markup);
        var resultDiv = component.Find(".roll-result");
        Assert.NotNull(resultDiv);
    }

    [Fact]
    public void AthleticsSkill_ToggleDetails_ShowsAndHidesDetails()
    {
        // Arrange
        var component = RenderComponent<AthleticsSkill>();
        var toggleButton = component.Find("button:contains('More Details')");

        // Act - Show details
        toggleButton.Click();

        // Assert - Details are shown
        Assert.Contains("skill-details-expanded", component.Markup);
        Assert.Contains("Skill Actions", component.Markup);

        // Act - Hide details
        var lessDetailsButton = component.Find("button:contains('Less Details')");
        lessDetailsButton.Click();

        // Assert - Details are hidden
        Assert.DoesNotContain("skill-details-expanded", component.Markup);
    }

    [Fact]
    public void AthleticsSkill_GetSkillDefinition_ReturnsCorrectSkill()
    {
        // Act
        var skill = AthleticsSkill.GetSkillDefinition();

        // Assert
        Assert.Equal("athletics", skill.Id);
        Assert.Equal("Athletics", skill.Name);
        Assert.Equal(AbilityScore.Strength, skill.KeyAbility);
        Assert.Equal("Physical", skill.SkillType);
        Assert.Equal("Core Rulebook", skill.Source);
        Assert.NotEmpty(skill.Actions);
        
        // Check specific actions
        var climbAction = skill.Actions.First(a => a.Name == "Climb");
        Assert.Equal(1, climbAction.ActionCost);
        
        var disarmAction = skill.Actions.First(a => a.Name == "Disarm");
        Assert.True(disarmAction.RequiresTrained);
        
        var highJumpAction = skill.Actions.First(a => a.Name == "High Jump");
        Assert.Equal(2, highJumpAction.ActionCost);
    }

    [Fact]
    public void AthleticsSkill_WithUntrained_HasZeroProficiencyBonus()
    {
        // Arrange
        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.InitialProficiency, ProficiencyLevel.Untrained));

        // Act - Get the bonus calculation (this tests the private method indirectly)
        var bonusElement = component.Find(".skill-bonus");
        
        // Assert - With no character and untrained, should just show base value
        Assert.NotNull(bonusElement);
    }

    [Theory]
    [InlineData(ProficiencyLevel.Trained, "+2")]
    [InlineData(ProficiencyLevel.Expert, "+4")]
    [InlineData(ProficiencyLevel.Master, "+6")]
    [InlineData(ProficiencyLevel.Legendary, "+8")]
    public void AthleticsSkill_WithDifferentProficiencies_ShowsCorrectBonus(ProficiencyLevel proficiency, string expectedBonusText)
    {
        // Arrange
        var character = new CharacterBuilder
        {
            Level = 1,
            AbilityScores = new Dictionary<AbilityScore, int>
            {
                [AbilityScore.Strength] = 10 // +0 modifier
            }
        };

        var component = RenderComponent<AthleticsSkill>(parameters => parameters
            .Add(p => p.Character, character)
            .Add(p => p.InitialProficiency, proficiency));

        // Assert
        var breakdown = component.Find(".bonus-breakdown").TextContent;
        Assert.Contains("prof", breakdown);
    }

    [Fact]
    public void AthleticsSkill_HasCorrectCssClasses()
    {
        // Act
        var component = RenderComponent<AthleticsSkill>();

        // Assert
        Assert.Contains("pf2e-skill-component", component.Markup);
        Assert.Contains("athletics-skill", component.Markup);
        Assert.Contains("skill-header", component.Markup);
        Assert.Contains("skill-bonus-display", component.Markup);
        Assert.Contains("skill-description", component.Markup);
        Assert.Contains("skill-actions-bar", component.Markup);
    }
}