using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Rules;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Rules;

public partial class RuleCard : ComponentBase
{
    [Parameter] public RuleRecord Rule { get; set; } = default!;
    [Parameter] public EventCallback<RuleRecord> OnRuleClick { get; set; }

    private bool _IsHovered = false;

    private async Task HandleClick()
    {
        if (OnRuleClick.HasDelegate)
        {
            await OnRuleClick.InvokeAsync(Rule);
        }
    }

    private string GetContentTypeCssClass()
    {
        return Rule.ContentType.ToString().ToLowerInvariant();
    }
}