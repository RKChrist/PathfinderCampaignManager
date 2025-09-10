using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Presentation.Client.Components.Pathfinder.Classes;

public partial class ClericComponent : ComponentBase
{
    [Parameter] public bool IsSelected { get; set; }
    [Parameter] public bool ShowDetails { get; set; }
    [Parameter] public EventCallback OnClassSelected { get; set; }
    [Parameter] public CharacterBuilder? Character { get; set; }

    private async Task SelectClass()
    {
        if (Character != null)
        {
            Character.SelectedClass = GetClassDefinition();
        }

        await OnClassSelected.InvokeAsync();
    }

    private void ToggleDetails()
    {
        ShowDetails = !ShowDetails;
    }

    public static PathfinderClass GetClassDefinition()
    {
        return new PathfinderClass
        {
            Id = "cleric",
            Name = "Cleric",
            Description = "Deities work their will upon the world in infinite ways, and you serve as one of their most stalwart mortal servants.",
            HitPoints = 8,
            KeyAbilities = new List<AbilityScore> { AbilityScore.Wisdom },
            SkillPoints = 2,
            Source = "Core Rulebook",
            Rarity = "Common",
            Traits = new List<string>(),
            IsSpellcaster = true,
            SpellcastingTradition = "Divine",
            SpellcastingAbility = "Wisdom",
            InitialProficiencies = new ClassProficiencies
            {
                Perception = ProficiencyRank.Trained,
                FortitudeSave = ProficiencyRank.Expert,
                ReflexSave = ProficiencyRank.Trained,
                WillSave = ProficiencyRank.Expert,
                Skills = new Dictionary<string, ProficiencyRank>
                {
                    ["Religion"] = ProficiencyRank.Trained,
                    ["Medicine"] = ProficiencyRank.Untrained
                },
                Weapons = new Dictionary<string, ProficiencyRank>
                {
                    ["Simple"] = ProficiencyRank.Trained,
                    ["Martial"] = ProficiencyRank.Untrained,
                    ["Advanced"] = ProficiencyRank.Untrained
                },
                Armor = new Dictionary<string, ProficiencyRank>
                {
                    ["Unarmored"] = ProficiencyRank.Trained,
                    ["Light"] = ProficiencyRank.Trained,
                    ["Medium"] = ProficiencyRank.Trained,
                    ["Heavy"] = ProficiencyRank.Untrained
                },
                SpellAttacks = ProficiencyRank.Trained,
                SpellDCs = ProficiencyRank.Expert
            },
            ClassFeatures = new List<string>
            {
                "Deity",
                "Divine Spellcasting",
                "Divine Font",
                "Domain Spells",
                "Anathema"
            }
        };
    }
}