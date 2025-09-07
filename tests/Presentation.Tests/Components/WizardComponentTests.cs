using Bunit;
using Microsoft.Extensions.DependencyInjection;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Presentation.Client.Components.Pathfinder.Classes;
using Xunit;

namespace PathfinderCampaignManager.Presentation.Tests.Components;

public class WizardComponentTests : TestContext
{
    public WizardComponentTests()
    {
        Services.AddLogging();
    }

    [Fact]
    public void WizardComponent_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<WizardComponent>();

        // Assert
        Assert.Contains("Wizard", component.Markup);
        Assert.Contains("Controller/Blaster", component.Markup);
        Assert.Contains("6 + Con modifier", component.Markup);
        Assert.Contains("Intelligence", component.Markup);
        Assert.Contains("2 + Int modifier", component.Markup);
    }

    [Fact]
    public void WizardComponent_ShowsSpellcastingInfo()
    {
        // Act
        var component = RenderComponent<WizardComponent>();

        // Assert
        Assert.Contains("hat-wizard", component.Markup);
        Assert.Contains("Controller/Blaster", component.Markup);
    }

    [Fact]
    public void WizardComponent_WhenDetailsExpanded_ShowsSpellcastingDetails()
    {
        // Act
        var component = RenderComponent<WizardComponent>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Spellcasting", component.Markup);
        Assert.Contains("Spell Tradition:", component.Markup);
        Assert.Contains("Arcane", component.Markup);
        Assert.Contains("Spell Ability:", component.Markup);
        Assert.Contains("Intelligence", component.Markup);
        Assert.Contains("1st: 2, Cantrips: 5", component.Markup);
        Assert.Contains("8 cantrips, 2 1st-level", component.Markup);
    }

    [Fact]
    public void WizardComponent_ShowsArcaneSchools_WhenDetailsExpanded()
    {
        // Act
        var component = RenderComponent<WizardComponent>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Arcane Schools", component.Markup);
        Assert.Contains("School of Evocation", component.Markup);
        Assert.Contains("School of Illusion", component.Markup);
        Assert.Contains("Universalist", component.Markup);
        Assert.Contains("+1 spell slot per level for evocation spells", component.Markup);
        Assert.Contains("Additional flexibility with spell preparation", component.Markup);
    }

    [Fact]
    public void WizardComponent_SelectClass_UpdatesCharacterBuilder()
    {
        // Arrange
        var character = new CharacterBuilder();
        bool selectionChangedCalled = false;

        var component = RenderComponent<WizardComponent>(parameters => parameters
            .Add(p => p.Character, character)
            .Add(p => p.OnClassSelected, () => { selectionChangedCalled = true; }));

        // Act
        var selectButton = component.Find("button.btn-primary");
        selectButton.Click();

        // Assert
        Assert.NotNull(character.SelectedClass);
        Assert.Equal("wizard", character.SelectedClass.Id);
        Assert.Equal("Wizard", character.SelectedClass.Name);
        Assert.Equal(6, character.SelectedClass.HitPoints);
        Assert.Equal(2, character.SelectedClass.SkillPoints);
        Assert.Contains(AbilityScore.Intelligence, character.SelectedClass.KeyAbilities);
        Assert.True(character.SelectedClass.IsSpellcaster);
        Assert.Equal("Arcane", character.SelectedClass.SpellcastingTradition);
        Assert.Equal("Intelligence", character.SelectedClass.SpellcastingAbility);
        Assert.True(selectionChangedCalled);
    }

    [Fact]
    public void WizardComponent_GetClassDefinition_ReturnsCorrectSpellcaster()
    {
        // Act
        var classDefinition = WizardComponent.GetClassDefinition();

        // Assert
        Assert.True(classDefinition.IsSpellcaster);
        Assert.Equal("Arcane", classDefinition.SpellcastingTradition);
        Assert.Equal("Intelligence", classDefinition.SpellcastingAbility);
        
        // Check spell proficiencies
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.SpellAttacks);
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.SpellDCs);
        
        // Check saves (Wizard has expert Will save)
        Assert.Equal(ProficiencyLevel.Expert, classDefinition.InitialProficiencies.WillSave);
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.FortitudeSave);
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.ReflexSave);
    }

    [Fact]
    public void WizardComponent_ShowsSampleBuilds_WhenDetailsExpanded()
    {
        // Act
        var component = RenderComponent<WizardComponent>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Sample Builds", component.Markup);
        Assert.Contains("Evoker Blaster", component.Markup);
        Assert.Contains("Int 18, Dex 14, Con 14", component.Markup);
        Assert.Contains("Controller", component.Markup);
        Assert.Contains("Int 18, Wis 14, Con 14", component.Markup);
    }

    [Fact]
    public void WizardComponent_ShowsClassFeatures_WhenDetailsExpanded()
    {
        // Act
        var component = RenderComponent<WizardComponent>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Class Features", component.Markup);
        Assert.Contains("Spellbook", component.Markup);
        Assert.Contains("Arcane Spellcasting", component.Markup);
        Assert.Contains("Cantrips", component.Markup);
        Assert.Contains("Wizard Feat", component.Markup);
    }

    [Fact]
    public void WizardComponent_ShowsCorrectProgression_WhenDetailsExpanded()
    {
        // Act
        var component = RenderComponent<WizardComponent>(parameters => parameters
            .Add(p => p.ShowDetails, true));

        // Assert
        Assert.Contains("Level Progression Preview", component.Markup);
        Assert.Contains("1st", component.Markup);
        Assert.Contains("Arcane spellcasting, spellbook, wizard feat", component.Markup);
        Assert.Contains("3rd", component.Markup);
        Assert.Contains("2nd-level spells", component.Markup);
        Assert.Contains("5th", component.Markup);
        Assert.Contains("3rd-level spells, lightning reflexes", component.Markup);
    }

    [Fact]
    public void WizardComponent_HasCorrectArmorProficiencies()
    {
        // Act
        var classDefinition = WizardComponent.GetClassDefinition();

        // Assert
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.Armor["Unarmored"]);
        Assert.Equal(ProficiencyLevel.Untrained, classDefinition.InitialProficiencies.Armor["Light"]);
        Assert.Equal(ProficiencyLevel.Untrained, classDefinition.InitialProficiencies.Armor["Medium"]);
        Assert.Equal(ProficiencyLevel.Untrained, classDefinition.InitialProficiencies.Armor["Heavy"]);
    }

    [Fact]
    public void WizardComponent_HasCorrectWeaponProficiencies()
    {
        // Act
        var classDefinition = WizardComponent.GetClassDefinition();

        // Assert
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.Weapons["Simple"]);
        Assert.Equal(ProficiencyLevel.Untrained, classDefinition.InitialProficiencies.Weapons["Martial"]);
        Assert.Equal(ProficiencyLevel.Untrained, classDefinition.InitialProficiencies.Weapons["Advanced"]);
    }

    [Fact]
    public void WizardComponent_HasCorrectSkillProficiencies()
    {
        // Act
        var classDefinition = WizardComponent.GetClassDefinition();

        // Assert
        Assert.Equal(ProficiencyLevel.Trained, classDefinition.InitialProficiencies.Skills["Arcana"]);
        Assert.Equal(ProficiencyLevel.Untrained, classDefinition.InitialProficiencies.Skills["Crafting"]);
    }

    [Fact]
    public void WizardComponent_WhenSelected_DisablesSelectButton()
    {
        // Act
        var component = RenderComponent<WizardComponent>(parameters => parameters
            .Add(p => p.IsSelected, true));

        // Assert
        var selectButton = component.Find("button.btn-primary");
        Assert.True(selectButton.HasAttribute("disabled"));
        Assert.Contains("Selected", selectButton.TextContent);
    }

    [Fact]
    public void WizardComponent_HasCorrectCssClasses()
    {
        // Act
        var component = RenderComponent<WizardComponent>();

        // Assert
        Assert.Contains("pf2e-class-component", component.Markup);
        Assert.Contains("wizard-class", component.Markup);
    }
}