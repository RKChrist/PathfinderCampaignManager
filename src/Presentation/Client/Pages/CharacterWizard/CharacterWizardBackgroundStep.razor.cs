using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Presentation.Client.Pages.CharacterWizard;

public partial class CharacterWizardBackgroundStep : ComponentBase
{
    [Parameter] public CharacterWizard.CharacterDraft Value { get; set; } = new();
    [Parameter] public EventCallback<CharacterWizard.CharacterDraft> ValueChanged { get; set; }
    [Parameter] public List<object> AvailableOptions { get; set; } = new();
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }

    private bool IsSelected(PfBackground background)
    {
        return Value.Background == background.Name;
    }

    private async Task SelectBackground(PfBackground background)
    {
        Value.Background = background.Name;
        await ValueChanged.InvokeAsync(Value);
        await OnChanged.InvokeAsync();
    }

    private string GetBackgroundIcon(string backgroundName)
    {
        return backgroundName.ToLower() switch
        {
            "acolyte" => "fas fa-pray",
            "artisan" => "fas fa-hammer",
            "criminal" => "fas fa-mask",
            "entertainer" => "fas fa-theater-masks",
            "folk hero" => "fas fa-heart",
            "hermit" => "fas fa-mountain",
            "noble" => "fas fa-crown",
            "scholar" => "fas fa-book",
            "soldier" => "fas fa-shield-alt",
            _ => "fas fa-user-tag"
        };
    }

    private string GetBackgroundSummary(PfBackground background)
    {
        return background.Name.ToLower() switch
        {
            "acolyte" => "You served in a temple or shrine, gaining divine insight and social connections.",
            "artisan" => "You practiced a trade, developing skill with tools and an eye for quality.",
            "criminal" => "You lived outside the law, developing stealth and street smarts.",
            "entertainer" => "You performed for crowds, gaining charisma and stage presence.",
            "folk hero" => "You stood up for others, earning respect and leadership skills.",
            "hermit" => "You lived in solitude, gaining wisdom and self-sufficiency.",
            "noble" => "You were raised in privilege, learning etiquette and gaining connections.",
            "scholar" => "You devoted yourself to learning, mastering knowledge and research.",
            "soldier" => "You served in an army, learning discipline and combat tactics.",
            _ => background.Description ?? "Your unique background shaped your early life and skills."
        };
    }
}