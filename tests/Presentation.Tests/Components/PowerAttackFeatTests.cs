using Bunit;
using Microsoft.Extensions.DependencyInjection;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Presentation.Client.Components.Pathfinder.Feats;
using Xunit;

namespace PathfinderCampaignManager.Presentation.Tests.Components;

public class PowerAttackFeatTests : TestContext
{
    public PowerAttackFeatTests()
    {
        Services.AddLogging();
    }

    [Fact]
    public void PowerAttackFeat_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>();

        // Assert
        Assert.Contains("Power Attack", component.Markup);
        Assert.Contains("Level 1", component.Markup);
        Assert.Contains("Fighter", component.Markup);
        Assert.Contains("Common", component.Markup);
        Assert.Contains("two-actions", component.Markup);
        Assert.Contains("fas fa-fist-raised", component.Markup);
    }

    [Fact]
    public void PowerAttackFeat_ShowsCorrectTraits()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>();

        // Assert
        Assert.Contains("trait fighter", component.Markup);
        Assert.Contains("trait flourish", component.Markup);
    }

    [Fact]
    public void PowerAttackFeat_ShowsCorrectDescription()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>();

        // Assert
        Assert.Contains("You unleash a particularly powerful attack", component.Markup);
        Assert.Contains("Make a melee Strike", component.Markup);
        Assert.Contains("This counts as two attacks", component.Markup);
        Assert.Contains("you deal an extra die of weapon damage", component.Markup);
    }

    [Fact]
    public void PowerAttackFeat_WhenDetailsExpanded_ShowsMechanics()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Mechanics", component.Markup);
        Assert.Contains("Action Cost: 2 actions", component.Markup);
        Assert.Contains("Multiple Attack Penalty: This counts as two attacks", component.Markup);
        Assert.Contains("+1 weapon damage die", component.Markup);
        Assert.Contains("You're wielding a melee weapon", component.Markup);
    }

    [Fact]
    public void PowerAttackFeat_WhenDetailsExpanded_ShowsScaling()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Level Scaling", component.Markup);
        Assert.Contains("1st-9th", component.Markup);
        Assert.Contains("+1 weapon damage die", component.Markup);
        Assert.Contains("10th-17th", component.Markup);
        Assert.Contains("+2 weapon damage dice", component.Markup);
        Assert.Contains("18th+", component.Markup);
        Assert.Contains("+3 weapon damage dice", component.Markup);
    }

    [Fact]
    public void PowerAttackFeat_WhenDetailsExpanded_ShowsTactics()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Tactical Usage", component.Markup);
        Assert.Contains("Best used when you have advantage", component.Markup);
        Assert.Contains("Consider positioning", component.Markup);
        Assert.Contains("Excellent for finishing moves", component.Markup);
        Assert.Contains("Synergizes well with critical hit builds", component.Markup);
    }

    [Fact]
    public void PowerAttackFeat_WhenDetailsExpanded_ShowsSynergies()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Synergies", component.Markup);
        Assert.Contains("Sudden Charge", component.Markup);
        Assert.Contains("Move and Power Attack in one turn", component.Markup);
        Assert.Contains("True Strike", component.Markup);
        Assert.Contains("Ensure the Power Attack hits", component.Markup);
    }

    [Fact]
    public void PowerAttackFeat_SelectFeat_UpdatesCharacterBuilder()
    {
        // Arrange
        var character = new CharacterBuilder();
        bool selectionChangedCalled = false;

        var component = RenderComponent<PowerAttackFeat>(parameters => parameters
            .Add(p => p.Character, character)
            .Add(p => p.OnFeatSelected, () => { selectionChangedCalled = true; }));

        // Act
        var selectButton = component.Find("button.btn-primary");
        selectButton.Click();

        // Assert
        Assert.NotNull(character.SelectedFeats);
        Assert.Single(character.SelectedFeats);
        
        var feat = character.SelectedFeats.First();
        Assert.Equal("power-attack", feat.Id);
        Assert.Equal("Power Attack", feat.Name);
        Assert.Equal(1, feat.Level);
        Assert.Equal("Fighter", feat.FeatType);
        Assert.Equal(2, feat.ActionCost);
        Assert.Contains("Fighter", feat.Traits);
        Assert.Contains("Flourish", feat.Traits);
        Assert.True(selectionChangedCalled);
    }

    [Fact]
    public void PowerAttackFeat_WhenSelected_ShowsSelectedState()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>(parameters => parameters
            .Add(p => p.IsSelected, true));

        // Assert
        var selectButton = component.Find("button.btn-primary");
        Assert.True(selectButton.HasAttribute("disabled"));
        Assert.Contains("Selected", selectButton.TextContent);
        Assert.Contains("fas fa-check", selectButton.InnerHtml);
    }

    [Fact]
    public void PowerAttackFeat_ToggleDetails_ShowsAndHidesDetails()
    {
        // Arrange
        var component = RenderComponent<PowerAttackFeat>();
        var toggleButton = component.Find("button.btn-secondary");

        // Act - Show details
        toggleButton.Click();

        // Assert - Details are shown
        Assert.Contains("feat-details-expanded", component.Markup);
        Assert.Contains("Mechanics", component.Markup);
        Assert.Contains("fas fa-chevron-up", toggleButton.InnerHtml);

        // Act - Hide details  
        toggleButton.Click();

        // Assert - Details are hidden
        Assert.DoesNotContain("feat-details-expanded", component.Markup);
        Assert.Contains("fas fa-chevron-down", toggleButton.InnerHtml);
    }

    [Fact]
    public void PowerAttackFeat_ShowPrerequisites_TogglesPrereqDetails()
    {
        // Arrange
        var component = RenderComponent<PowerAttackFeat>(parameters => parameters
            .Add(p => p.ShowPrerequisites, true));

        var prereqButton = component.Find("button.btn-info");

        // Act
        prereqButton.Click();

        // Assert
        Assert.Contains("feat-prerequisites", component.Markup);
        Assert.Contains("Prerequisites", component.Markup);
        Assert.Contains("Fighter class or archetype", component.Markup);
        Assert.Contains("Must be able to make melee Strikes", component.Markup);
    }

    [Fact]
    public void PowerAttackFeat_GetFeatDefinition_ReturnsCorrectFeat()
    {
        // Act
        var feat = PowerAttackFeat.GetFeatDefinition();

        // Assert
        Assert.Equal("power-attack", feat.Id);
        Assert.Equal("Power Attack", feat.Name);
        Assert.Equal(1, feat.Level);
        Assert.Equal("Fighter", feat.FeatType);
        Assert.Equal(2, feat.ActionCost);
        Assert.Equal("Core Rulebook", feat.Source);
        Assert.Equal("Common", feat.Rarity);
        Assert.Contains("Fighter", feat.Traits);
        Assert.Contains("Flourish", feat.Traits);
        Assert.Contains("Fighter class", feat.Prerequisites);
        Assert.Contains("Make a melee Strike that deals an extra die", feat.Benefits);
        Assert.Contains("You're wielding a melee weapon", feat.Requirements);
    }

    [Fact]
    public void PowerAttackFeat_SelectFeat_DoesNotDuplicateFeats()
    {
        // Arrange
        var character = new CharacterBuilder();
        var component = RenderComponent<PowerAttackFeat>(parameters => parameters
            .Add(p => p.Character, character));

        // Act - Select feat twice
        var selectButton = component.Find("button.btn-primary");
        selectButton.Click();
        selectButton.Click();

        // Assert - Should only have one feat
        Assert.NotNull(character.SelectedFeats);
        Assert.Single(character.SelectedFeats);
    }

    [Fact]
    public void PowerAttackFeat_HasCorrectCssClasses()
    {
        // Act
        var component = RenderComponent<PowerAttackFeat>();

        // Assert
        Assert.Contains("pf2e-feat-component", component.Markup);
        Assert.Contains("power-attack-feat", component.Markup);
        Assert.Contains("feat-header", component.Markup);
        Assert.Contains("feat-traits", component.Markup);
        Assert.Contains("feat-description", component.Markup);
        Assert.Contains("feat-actions-bar", component.Markup);
    }
}