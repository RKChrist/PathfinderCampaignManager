using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Combat;
using PathfinderCampaignManager.Presentation.Shared.Models;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace PathfinderCampaignManager.Presentation.Client.Components.CharacterSheet;

public partial class CharacterSheetModal : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant? Character { get; set; }
    [Parameter] public bool IsPlayerCharacter { get; set; }
    [Parameter] public bool IsDM { get; set; }
    [Parameter] public EventCallback OnEditCharacter { get; set; }

    private CharacterDto? DetailedCharacter { get; set; }
    private bool _isLoading = false;

    // Tooltip state
    private bool _showTooltip = false;
    private string _tooltipTitle = string.Empty;
    private string _tooltipContent = string.Empty;
    private string _tooltipX = "0px";
    private string _tooltipY = "0px";

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible && Character != null && DetailedCharacter == null && !_isLoading)
        {
            await LoadDetailedCharacter();
        }
    }

    private async Task LoadDetailedCharacter()
    {
        if (Character?.CharacterId == null)
            return;

        _isLoading = true;
        try
        {
            var response = await Http.GetAsync($"api/characters/{Character.CharacterId}");
            if (response.IsSuccessStatusCode)
            {
                DetailedCharacter = await response.Content.ReadFromJsonAsync<CharacterDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading detailed character: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private string GetHPStatusClass()
    {
        if (Character == null)
            return "healthy";

        if (Character.CurrentHitPoints <= 0)
            return "unconscious";
        if (Character.CurrentHitPoints <= Character.HitPoints * 0.25)
            return "critical";
        if (Character.CurrentHitPoints <= Character.HitPoints * 0.5)
            return "wounded";
        return "healthy";
    }

    private string FormatModifier(int modifier)
    {
        return modifier >= 0 ? $"+{modifier}" : modifier.ToString();
    }

    private int CalculateModifier(int abilityScore)
    {
        return (abilityScore - 10) / 2;
    }

    private string GetProficiencyClass(string proficiency)
    {
        return proficiency.ToLower() switch
        {
            "untrained" => "untrained",
            "trained" => "trained",
            "expert" => "expert",
            "master" => "master",
            "legendary" => "legendary",
            _ => "untrained"
        };
    }

    // Tooltip Methods
    private async Task ShowFeatTooltip(string featName)
    {
        await ShowTooltip(featName, await GetFeatDescription(featName));
    }

    private async Task ShowEquipmentTooltip(string itemName)
    {
        await ShowTooltip(itemName, await GetEquipmentDescription(itemName));
    }

    private async Task ShowConditionTooltip(string conditionName)
    {
        await ShowTooltip(conditionName, await GetConditionDescription(conditionName));
    }

    private async Task ShowTooltip(string title, string content)
    {
        _tooltipTitle = title;
        _tooltipContent = content;
        
        // Get mouse position
        var mousePosition = await JSRuntime.InvokeAsync<MousePosition>("getMousePosition");
        _tooltipX = $"{mousePosition.X + 10}px";
        _tooltipY = $"{mousePosition.Y - 10}px";
        
        _showTooltip = true;
        StateHasChanged();
    }

    private void HideTooltip()
    {
        _showTooltip = false;
        StateHasChanged();
    }

    private string GetTooltipStyle()
    {
        return $"left: {_tooltipX}; top: {_tooltipY};";
    }

    // Content Fetching Methods
    private async Task<string> GetFeatDescription(string featName)
    {
        try
        {
            // In a real implementation, fetch from feat database
            var response = await Http.GetAsync($"api/feats/search?name={featName}");
            if (response.IsSuccessStatusCode)
            {
                var feat = await response.Content.ReadFromJsonAsync<FeatDto>();
                return feat?.Description ?? "Description not available.";
            }
        }
        catch
        {
            // Fallback for demo
        }

        return GetFallbackFeatDescription(featName);
    }

    private async Task<string> GetEquipmentDescription(string itemName)
    {
        try
        {
            // In a real implementation, fetch from equipment database
            var response = await Http.GetAsync($"api/equipment/search?name={itemName}");
            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<EquipmentDto>();
                return item?.Description ?? "Description not available.";
            }
        }
        catch
        {
            // Fallback for demo
        }

        return GetFallbackEquipmentDescription(itemName);
    }

    private async Task<string> GetConditionDescription(string conditionName)
    {
        try
        {
            // In a real implementation, fetch from conditions database
            var response = await Http.GetAsync($"api/conditions/search?name={conditionName}");
            if (response.IsSuccessStatusCode)
            {
                var condition = await response.Content.ReadFromJsonAsync<ConditionDto>();
                return condition?.Description ?? "Description not available.";
            }
        }
        catch
        {
            // Fallback for demo
        }

        return GetFallbackConditionDescription(conditionName);
    }

    // Fallback descriptions for demo purposes
    private string GetFallbackFeatDescription(string featName)
    {
        return featName.ToLower() switch
        {
            "power attack" => "You make a more powerful attack at the cost of accuracy. Make a melee Strike. This counts as two attacks when calculating your multiple attack penalty.",
            "combat reflexes" => "You can use reactions more frequently. You gain a +1 circumstance bonus to your AC against attacks made as part of an Attack of Opportunity.",
            "toughness" => "You have enhanced stamina. Increase your maximum Hit Points by your level.",
            "weapon focus" => "You've practiced extensively with a particular weapon. Choose a weapon group. You gain a +1 circumstance bonus to attack rolls with weapons in that group.",
            _ => $"A feat that provides various benefits and abilities related to {featName}."
        };
    }

    private string GetFallbackEquipmentDescription(string itemName)
    {
        return itemName.ToLower() switch
        {
            "longsword" => "A versatile martial weapon. 1d8 slashing damage. Traits: Versatile P",
            "shortbow" => "A simple ranged weapon. 1d6 piercing damage. Range 60 feet. Traits: Deadly d10",
            "leather armor" => "Light armor made from leather. AC +1, Dex Cap +4, Check Penalty -1",
            "chain mail" => "Medium armor made from interlocking metal rings. AC +4, Dex Cap +1, Check Penalty -2",
            _ => $"Equipment item: {itemName}. Provides various mechanical benefits."
        };
    }

    private string GetFallbackConditionDescription(string conditionName)
    {
        return conditionName.ToLower() switch
        {
            "blinded" => "You can't see. All terrain is difficult terrain, and you can't detect anything using vision.",
            "charmed" => "You view the creature that charmed you as a trusted friend. You can't willingly harm the charming creature.",
            "frightened" => "You're gripped by fear and have the frightened condition. You take a status penalty to all checks equal to the condition value.",
            "paralyzed" => "Your body is frozen in place. You have the paralyzed condition and can't act except to Recall Knowledge and use actions with the concentrate trait.",
            "prone" => "You're lying on the ground. You take a -2 circumstance penalty to attack rolls.",
            "stunned" => "You've become senseless. You have the stunned condition and can't act while stunned.",
            _ => $"Condition: {conditionName}. Applies various effects to the character."
        };
    }

    private async Task EditCharacter()
    {
        if (OnEditCharacter.HasDelegate)
        {
            await OnEditCharacter.InvokeAsync();
        }
    }
}

// Supporting DTOs and Classes
public class MousePosition
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class FeatDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> Traits { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
}

public class EquipmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Price { get; set; } = string.Empty;
}

public class ConditionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Effects { get; set; } = new();
}