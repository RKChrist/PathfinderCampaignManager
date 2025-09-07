using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Presentation.Client.Pages.CharacterWizard;

public partial class CharacterWizardAbilityStep : ComponentBase
{
    [Parameter] public CharacterWizard.CharacterDraft Value { get; set; } = new();
    [Parameter] public EventCallback<CharacterWizard.CharacterDraft> ValueChanged { get; set; }
    [Parameter] public CalculatedCharacter? CalculatedCharacter { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }

    private string assignmentMethod = "standard";
    private string[] freeBoosts = new string[4];

    private static readonly string[] AbilityNames = 
    {
        "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma"
    };

    private bool IsStandardMethodSelected => assignmentMethod == "standard";

    protected override void OnInitialized()
    {
        // Initialize free boosts from Value if they exist
        if (Value.AbilityBoosts.ContainsKey("Free"))
        {
            // Parse free boosts - this is a simplified version
        }
    }

    private async Task SetAssignmentMethod(string method)
    {
        assignmentMethod = method;
        await OnChanged.InvokeAsync();
    }

    private async Task SetFreeBoost(int slot, string? ability)
    {
        if (slot >= 0 && slot < freeBoosts.Length)
        {
            freeBoosts[slot] = ability ?? string.Empty;
            
            // Update the Value object
            Value.AbilityBoosts["Free"] = freeBoosts.Count(x => !string.IsNullOrEmpty(x));
            
            await ValueChanged.InvokeAsync(Value);
            await OnChanged.InvokeAsync();
        }
    }

    private string GetSelectedFreeBoost(int slot)
    {
        return slot < freeBoosts.Length ? freeBoosts[slot] : string.Empty;
    }

    private bool CanSelectFreeBoost(string ability, int currentSlot)
    {
        // Can't select the same ability more than once for free boosts
        var usedCount = freeBoosts.Where((boost, index) => index != currentSlot && boost == ability).Count();
        return usedCount == 0;
    }

    private int GetFinalScore(string ability)
    {
        if (CalculatedCharacter?.AbilityScores.TryGetValue(ability, out var score) == true)
        {
            return score;
        }
        
        // Fallback calculation
        var baseScore = 10;
        var boosts = GetBoostCount(ability, "Ancestry") + 
                    GetBoostCount(ability, "Background") + 
                    GetBoostCount(ability, "Class") + 
                    GetBoostCount(ability, "Free");
        
        return baseScore + (boosts * 2);
    }

    private int GetBoostCount(string ability, string source)
    {
        // This would need to be implemented based on selected ancestry/background/class
        // For now, return mock data
        return source switch
        {
            "Ancestry" => ability == "Strength" ? 1 : 0,
            "Background" => ability == "Intelligence" ? 1 : 0,
            "Class" => ability == "Strength" ? 1 : 0,
            "Free" => freeBoosts.Count(x => x == ability),
            _ => 0
        };
    }

    private int GetModifier(string ability)
    {
        var score = GetFinalScore(ability);
        return (score - 10) / 2;
    }

    private string FormatModifier(int modifier)
    {
        return modifier >= 0 ? $"+{modifier}" : modifier.ToString();
    }
}