using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Presentation.Client.Pages.CharacterWizard;

public partial class CharacterWizardAncestryStep : ComponentBase
{
    [Parameter] public CharacterWizard.CharacterDraft Value { get; set; } = new();
    [Parameter] public EventCallback<CharacterWizard.CharacterDraft> ValueChanged { get; set; }
    [Parameter] public List<object> AvailableOptions { get; set; } = new();
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }

    private bool IsSelected(PfAncestry ancestry)
    {
        return Value.Ancestry == ancestry.Name;
    }

    private async Task SelectAncestry(PfAncestry ancestry)
    {
        Value.Ancestry = ancestry.Name;
        await ValueChanged.InvokeAsync(Value);
        await OnChanged.InvokeAsync();
    }

    private string GetAncestryIcon(string ancestryName)
    {
        return ancestryName.ToLower() switch
        {
            "human" => "fas fa-user",
            "elf" => "fas fa-leaf",
            "dwarf" => "fas fa-hammer",
            "halfling" => "fas fa-home",
            "gnome" => "fas fa-hat-wizard",
            _ => "fas fa-user-circle"
        };
    }

    private string GetAncestrySummary(PfAncestry ancestry)
    {
        return ancestry.Name.ToLower() switch
        {
            "human" => "Versatile and ambitious, humans excel in any role with their adaptability.",
            "elf" => "Long-lived and magical, elves are graceful and attuned to nature and magic.",
            "dwarf" => "Hardy mountain folk known for their craftsmanship and resilience.",
            "halfling" => "Small but brave, halflings bring luck and optimism wherever they go.",
            "gnome" => "Curious and eccentric, gnomes are driven by wonder and magical innovation.",
            _ => ancestry.Description ?? "A unique ancestry with its own strengths and traditions."
        };
    }
}