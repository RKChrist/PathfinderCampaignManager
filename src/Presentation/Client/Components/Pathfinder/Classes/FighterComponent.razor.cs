using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Presentation.Client.Components.Pathfinder.Classes;

public partial class FighterComponent : ComponentBase
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
            Id = "fighter",
            Name = "Fighter",
            Description = "Fighting for honor, greed, loyalty, or simply the thrill of battle, you are an undisputed master of weaponry and combat techniques.",
            HitPoints = 10,
            KeyAbilities = new List<AbilityScore> { AbilityScore.Strength, AbilityScore.Dexterity },
            SkillPoints = 3,
            Source = "Core Rulebook",
            Rarity = "Common",
            Traits = new List<string>(),
            InitialProficiencies = new ClassProficiencies
            {
                Perception = ProficiencyLevel.Trained,
                FortitudeSave = ProficiencyLevel.Expert,
                ReflexSave = ProficiencyLevel.Trained,
                WillSave = ProficiencyLevel.Trained,
                Skills = new Dictionary<string, ProficiencyLevel>
                {
                    ["Acrobatics"] = ProficiencyLevel.Untrained,
                    ["Athletics"] = ProficiencyLevel.Trained
                },
                Weapons = new Dictionary<string, ProficiencyLevel>
                {
                    ["Simple"] = ProficiencyLevel.Trained,
                    ["Martial"] = ProficiencyLevel.Expert,
                    ["Advanced"] = ProficiencyLevel.Untrained
                },
                Armor = new Dictionary<string, ProficiencyLevel>
                {
                    ["Unarmored"] = ProficiencyLevel.Trained,
                    ["Light"] = ProficiencyLevel.Trained,
                    ["Medium"] = ProficiencyLevel.Trained,
                    ["Heavy"] = ProficiencyLevel.Trained
                }
            },
            ClassFeatures = new List<string>
            {
                "Attack of Opportunity",
                "Fighter Feats",
                "Shield Block",
                "Bravery",
                "Combat Flexibility"
            }
        };
    }
}