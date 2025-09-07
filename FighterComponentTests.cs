using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace StandalonePF2eTests;

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
        Assert.Contains("Martial Warrior", component.Markup);
        Assert.Contains("10 + Con modifier", component.Markup);
        Assert.Contains("Str or Dex", component.Markup);
        Assert.Contains("fas fa-sword", component.Markup);
    }

    [Fact]
    public void FighterComponent_ShowsCorrectTraits()
    {
        // Act
        var component = RenderComponent<FighterComponent>();

        // Assert
        Assert.Contains("pf2e-class-component", component.Markup);
        Assert.Contains("fighter-class", component.Markup);
        Assert.Contains("Common", component.Markup);
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
        Assert.Equal(3, character.SelectedClass.SkillPoints);
        Assert.Contains(AbilityScore.Strength, character.SelectedClass.KeyAbilities);
        Assert.Contains(AbilityScore.Dexterity, character.SelectedClass.KeyAbilities);
        Assert.True(selectionChangedCalled);
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
    public void FighterComponent_GetClassDefinition_ReturnsCorrectClass()
    {
        // Act
        var fighterClass = FighterComponent.GetClassDefinition();

        // Assert
        Assert.Equal("fighter", fighterClass.Id);
        Assert.Equal("Fighter", fighterClass.Name);
        Assert.Equal(10, fighterClass.HitPoints);
        Assert.Equal(3, fighterClass.SkillPoints);
        Assert.Equal("Core Rulebook", fighterClass.Source);
        Assert.Equal("Common", fighterClass.Rarity);
        Assert.Contains(AbilityScore.Strength, fighterClass.KeyAbilities);
        Assert.Contains(AbilityScore.Dexterity, fighterClass.KeyAbilities);
        
        // Check proficiencies
        Assert.Equal(ProficiencyLevel.Expert, fighterClass.InitialProficiencies.Perception);
        Assert.Equal(ProficiencyLevel.Expert, fighterClass.InitialProficiencies.FortitudeSave);
        Assert.Equal(ProficiencyLevel.Expert, fighterClass.InitialProficiencies.ReflexSave);
        Assert.Equal(ProficiencyLevel.Trained, fighterClass.InitialProficiencies.WillSave);
        
        // Check weapon proficiencies
        Assert.Equal(ProficiencyLevel.Expert, fighterClass.InitialProficiencies.Weapons["Simple"]);
        Assert.Equal(ProficiencyLevel.Expert, fighterClass.InitialProficiencies.Weapons["Martial"]);
        Assert.Equal(ProficiencyLevel.Trained, fighterClass.InitialProficiencies.Weapons["Advanced"]);
        
        // Check class features
        Assert.Contains("Attack of Opportunity", fighterClass.ClassFeatures);
        Assert.Contains("Fighter Feats", fighterClass.ClassFeatures);
        Assert.Contains("Shield Block", fighterClass.ClassFeatures);
        Assert.Contains("Bravery", fighterClass.ClassFeatures);
        Assert.Contains("Combat Flexibility", fighterClass.ClassFeatures);
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