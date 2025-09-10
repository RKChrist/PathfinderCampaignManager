using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

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
                Perception = ProficiencyRank.Trained,
                FortitudeSave = ProficiencyRank.Expert,
                ReflexSave = ProficiencyRank.Trained,
                WillSave = ProficiencyRank.Trained,
                Skills = new Dictionary<string, ProficiencyRank>
                {
                    ["Acrobatics"] = ProficiencyRank.Untrained,
                    ["Athletics"] = ProficiencyRank.Trained
                },
                Weapons = new Dictionary<string, ProficiencyRank>
                {
                    ["Simple"] = ProficiencyRank.Trained,
                    ["Martial"] = ProficiencyRank.Expert,
                    ["Advanced"] = ProficiencyRank.Untrained
                },
                Armor = new Dictionary<string, ProficiencyRank>
                {
                    ["Unarmored"] = ProficiencyRank.Trained,
                    ["Light"] = ProficiencyRank.Trained,
                    ["Medium"] = ProficiencyRank.Trained,
                    ["Heavy"] = ProficiencyRank.Trained
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