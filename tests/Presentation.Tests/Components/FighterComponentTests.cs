using Bunit;
using Microsoft.Extensions.DependencyInjection;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Presentation.Client.Components.Pathfinder.Classes;
using Xunit;

namespace PathfinderCampaignManager.Presentation.Tests.Components;

public class FighterComponentTests : TestContext
{
    public FighterComponentTests()
    {
        Services.AddLogging();
    }

    [Fact]
    public void FighterComponent_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<FighterComponent>();

        // Assert
        Assert.Contains("Fighter", component.Markup);
        Assert.Contains("Striker/Defender", component.Markup);
        Assert.Contains("10 + Con modifier", component.Markup);
        Assert.Contains("Str or Dex", component.Markup);
        Assert.Contains("3 + Int modifier", component.Markup);
    }

    [Fact]
    public void FighterComponent_ShowsCorrectTraits()
    {
        // Act
        var component = RenderComponent<FighterComponent>();

        // Assert
        Assert.Contains("Common", component.Markup);
        Assert.Contains("fas fa-sword", component.Markup);
    }

    [Fact]
    public void FighterComponent_WhenSelected_ShowsSelectedState()
    {
        // Act
        var component = RenderComponent<FighterComponent>(parameters => parameters
            .Add(p => p.IsSelected, true));

        // Assert
        var selectButton = component.Find("button.btn-primary");
        Assert.True(selectButton.HasAttribute("disabled"));
        Assert.Contains("Selected", selectButton.TextContent);
        Assert.Contains("fas fa-check", selectButton.InnerHtml);
    }

    [Fact]
    public void FighterComponent_WhenNotSelected_ShowsSelectableState()
    {
        // Act
        var component = RenderComponent<FighterComponent>(parameters => parameters
            .Add(p => p.IsSelected, false));

        // Assert
        var selectButton = component.Find("button.btn-primary");
        Assert.False(selectButton.HasAttribute("disabled"));
        Assert.Contains("Select Fighter", selectButton.TextContent);
        Assert.Contains("fas fa-plus", selectButton.InnerHtml);
    }

    [Fact]
    public void FighterComponent_ToggleDetails_ShowsAndHidesDetails()
    {
        // Arrange
        var component = RenderComponent<FighterComponent>();
        var toggleButton = component.Find("button.btn-secondary");

        // Act - Show details
        toggleButton.Click();

        // Assert - Details are shown
        Assert.Contains("class-details-expanded", component.Markup);
        Assert.Contains("Class Features", component.Markup);
        Assert.Contains("Level Progression Preview", component.Markup);
        Assert.Contains("Sample Builds", component.Markup);
        Assert.Contains("Less Details", toggleButton.TextContent);

        // Act - Hide details
        toggleButton.Click();

        // Assert - Details are hidden
        Assert.DoesNotContain("class-details-expanded", component.Markup);
        Assert.Contains("More Details", toggleButton.TextContent);
    }

    [Fact]
    public void FighterComponent_SelectClass_UpdatesCharacterBuilder()
    {
        // Arrange
        var character = new CharacterBuilder();
        bool selectionChangedCalled = false;

        var component = RenderComponent<FighterComponent>(parameters => parameters
            .Add(p => p.Character, character)
            .Add(p => p.OnClassSelected, () => { selectionChangedCalled = true; }));

        // Act
        var selectButton = component.Find("button.btn-primary");
        selectButton.Click();

        // Assert
        Assert.NotNull(character.SelectedClass);
        Assert.Equal("fighter", character.SelectedClass.Id);
        Assert.Equal("Fighter", character.SelectedClass.Name);
        Assert.Equal(10, character.SelectedClass.HitPoints);
        Assert.Contains(AbilityScore.Strength, character.SelectedClass.KeyAbilities);
        Assert.Contains(AbilityScore.Dexterity, character.SelectedClass.KeyAbilities);
        Assert.True(selectionChangedCalled);
    }

    [Fact]
    public void FighterComponent_ShowsProgressionPreview_WhenDetailsExpanded()
    {
        // Arrange
        var component = RenderComponent<FighterComponent>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("1st", component.Markup);
        Assert.Contains("Fighter feat, Shield Block", component.Markup);
        Assert.Contains("2nd", component.Markup);
        Assert.Contains("3rd", component.Markup);
        Assert.Contains("Bravery", component.Markup);
        Assert.Contains("5th", component.Markup);
        Assert.Contains("Fighter weapon mastery", component.Markup);
    }

    [Fact]
    public void FighterComponent_ShowsSampleBuilds_WhenDetailsExpanded()
    {
        // Arrange
        var component = RenderComponent<FighterComponent>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Sample Builds", component.Markup);
        Assert.Contains("Weapon Master", component.Markup);
        Assert.Contains("Str 18, Dex 14, Con 16", component.Markup);
        Assert.Contains("Archer", component.Markup);
        Assert.Contains("Str 14, Dex 18, Con 16", component.Markup);
    }

    [Fact]
    public void FighterComponent_GetClassDefinition_ReturnsCorrectClass()
    {
        // Act
        var classDefinition = FighterComponent.GetClassDefinition();

        // Assert
        Assert.Equal("fighter", classDefinition.Id);
        Assert.Equal("Fighter", classDefinition.Name);
        Assert.Equal(10, classDefinition.HitPoints);
        Assert.Equal(3, classDefinition.SkillPoints);
        Assert.Equal("Core Rulebook", classDefinition.Source);
        Assert.Equal("Common", classDefinition.Rarity);
        Assert.Contains(AbilityScore.Strength, classDefinition.KeyAbilities);
        Assert.Contains(AbilityScore.Dexterity, classDefinition.KeyAbilities);

        // Check proficiencies
        Assert.NotNull(classDefinition.InitialProficiencies);
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.Perception);
        Assert.Equal(ProficiencyLevel.Expert, classDefinition.InitialProficiencies.FortitudeSave);
        Assert.Equal(ProficiencyLevel.Expert, classDefinition.InitialProficiencies.Weapons["Martial"]);
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.Armor["Heavy"]);
    }

    [Fact]
    public void FighterComponent_RendersClassFeatures_WhenDetailsShown()
    {
        // Arrange
        var component = RenderComponent<FighterComponent>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Ancestry and Background", component.Markup);
        Assert.Contains("Initial Proficiencies", component.Markup);
        Assert.Contains("Fighter Feat", component.Markup);
        Assert.Contains("Shield Block", component.Markup);
    }

    [Theory]
    [InlineData(true, "Less Details")]
    [InlineData(false, "More Details")]
    public void FighterComponent_ToggleButton_ShowsCorrectText(bool showDetails, string expectedText)
    {
        // Act
        var component = RenderComponent<FighterComponent>(parameters => parameters
            .Add(p => p.ShowDetails, showDetails));

        // Assert
        var toggleButton = component.Find("button.btn-secondary");
        Assert.Contains(expectedText, toggleButton.TextContent);
    }

    [Fact]
    public void FighterComponent_HasCorrectCssClasses()
    {
        // Act
        var component = RenderComponent<FighterComponent>();

        // Assert
        Assert.Contains("pf2e-class-component", component.Markup);
        Assert.Contains("fighter-class", component.Markup);
        Assert.Contains("class-header", component.Markup);
        Assert.Contains("class-stats-grid", component.Markup);
        Assert.Contains("class-description", component.Markup);
        Assert.Contains("class-actions", component.Markup);
    }
}