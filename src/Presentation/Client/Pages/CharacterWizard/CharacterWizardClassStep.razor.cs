using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Presentation.Client.Pages.CharacterWizard;

public partial class CharacterWizardClassStep : ComponentBase
{
    [Parameter] public CharacterWizard.CharacterDraft Value { get; set; } = new();
    [Parameter] public EventCallback<CharacterWizard.CharacterDraft> ValueChanged { get; set; }
    [Parameter] public List<object> AvailableOptions { get; set; } = new();
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }

    private bool IsSelected(PfClass characterClass)
    {
        return Value.Class == characterClass.Name;
    }

    private async Task SelectClass(PfClass characterClass)
    {
        Value.Class = characterClass.Name;
        await ValueChanged.InvokeAsync(Value);
        await OnChanged.InvokeAsync();
    }

    private string GetClassIcon(string className)
    {
        return className.ToLower() switch
        {
            "fighter" => "fas fa-sword",
            "wizard" => "fas fa-hat-wizard",
            "rogue" => "fas fa-mask",
            "cleric" => "fas fa-cross",
            "ranger" => "fas fa-bow-arrow",
            "barbarian" => "fas fa-axe-battle",
            "bard" => "fas fa-music",
            "champion" => "fas fa-shield",
            "druid" => "fas fa-leaf",
            "monk" => "fas fa-fist-raised",
            "sorcerer" => "fas fa-fire",
            "alchemist" => "fas fa-flask",
            _ => "fas fa-user-graduate"
        };
    }

    private string GetClassSummary(PfClass characterClass)
    {
        return characterClass.Name.ToLower() switch
        {
            "fighter" => "A versatile warrior skilled with weapons and armor, adaptable to any combat role.",
            "wizard" => "A master of arcane magic who prepares spells and studies the fundamental forces of reality.",
            "rogue" => "A skilled infiltrator who excels at precision damage and diverse skills.",
            "cleric" => "A divine spellcaster who channels the power of their deity to heal and support allies.",
            "ranger" => "A skilled tracker and hunter who combines martial prowess with nature magic.",
            "barbarian" => "A fierce warrior who channels primal rage to devastate foes in melee combat.",
            "bard" => "A charismatic performer who uses occult magic and inspiring performances to support allies.",
            "champion" => "A divine warrior bound by a code, excelling at defense and righteous combat.",
            "druid" => "A primal spellcaster connected to nature who can shapeshift and control natural forces.",
            "monk" => "A martial artist who harnesses inner power to perform incredible physical feats.",
            "sorcerer" => "A natural spellcaster with innate magical power from their bloodline or essence.",
            "alchemist" => "A master of alchemy who creates mutagens, elixirs, and bombs to support allies and harm foes.",
            _ => characterClass.Description ?? "A unique class with specialized abilities and role."
        };
    }

    private int GetClassComplexity(string className)
    {
        return className.ToLower() switch
        {
            "fighter" => 1,
            "barbarian" => 1,
            "champion" => 1,
            "ranger" => 2,
            "rogue" => 2,
            "cleric" => 2,
            "bard" => 2,
            "monk" => 2,
            "sorcerer" => 3,
            "wizard" => 3,
            "druid" => 3,
            "alchemist" => 3,
            _ => 2
        };
    }
}