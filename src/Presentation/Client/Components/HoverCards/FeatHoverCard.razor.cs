using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Presentation.Client.Components.HoverCards;

public partial class FeatHoverCard : ComponentBase, IAsyncDisposable
{
    [Parameter] public PfFeat? Feat { get; set; }
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public bool ShowViewDetailsButton { get; set; } = true;
    [Parameter] public ICalculatedCharacter? Character { get; set; }
    [Parameter] public EventCallback<PfFeat> OnViewDetails { get; set; }
    [Parameter] public EventCallback OnMouseEnter { get; set; }
    [Parameter] public EventCallback OnMouseLeave { get; set; }

    private (double X, double Y) _position = (0, 0);
    private DotNetObjectReference<FeatHoverCard>? _objRef;

    protected override async Task OnInitializedAsync()
    {
        _objRef = DotNetObjectReference.Create(this);
    }

    public async Task ShowAtPositionAsync(double x, double y)
    {
        _position = (x, y);
        
        // Adjust position to keep card on screen
        await AdjustPositionAsync();
        
        StateHasChanged();
    }

    private async Task AdjustPositionAsync()
    {
        try
        {
            var windowSize = await JSRuntime.InvokeAsync<WindowSize>("getWindowSize");
            const int cardWidth = 420;
            const int cardHeight = 350; // Approximate
            
            var x = _position.X;
            var y = _position.Y;
            
            // Adjust X position if card would go off right edge
            if (x + cardWidth > windowSize.Width)
            {
                x = windowSize.Width - cardWidth - 20;
            }
            
            // Adjust Y position if card would go off bottom edge
            if (y + cardHeight > windowSize.Height)
            {
                y = Math.Max(20, y - cardHeight - 20);
            }
            
            // Ensure minimum margins
            x = Math.Max(20, x);
            y = Math.Max(20, y);
            
            _position = (x, y);
        }
        catch (JSException)
        {
            // Fallback if JS call fails
        }
    }

    private async Task ViewDetails()
    {
        if (Feat != null)
        {
            await OnViewDetails.InvokeAsync(Feat);
        }
    }

    private string GetActionCostDisplay()
    {
        if (Feat?.ActionCost == null)
            return "";
        
        return Feat.ActionCost switch
        {
            "Free" => "Free Action",
            "Reaction" => "Reaction",
            "1" => "1 Action",
            "2" => "2 Actions", 
            "3" => "3 Actions",
            _ => Feat.ActionCost
        };
    }

    private string FormatDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            return "";
        
        // Truncate long descriptions for hover cards
        const int maxLength = 400;
        if (description.Length > maxLength)
        {
            var truncated = description.Substring(0, maxLength);
            var lastPeriod = truncated.LastIndexOf('.');
            if (lastPeriod > maxLength / 2)
            {
                return truncated.Substring(0, lastPeriod + 1) + " <em>[...]</em>";
            }
            return truncated + "... <em>[continued]</em>";
        }
        
        return description
            .Replace("\n", "<br/>")
            .Replace("**", "<strong>", StringComparison.OrdinalIgnoreCase)
            .Replace("**", "</strong>", StringComparison.OrdinalIgnoreCase);
    }

    private bool ValidatePrerequisite(PfPrerequisite prerequisite)
    {
        if (Character == null)
            return true; // Can't validate without character context
        
        return prerequisite.Type switch
        {
            "AbilityScore" => ValidateAbilityScorePrerequisite(prerequisite),
            "Skill" => ValidateSkillPrerequisite(prerequisite),
            "Level" => Character.Level >= int.Parse(prerequisite.Value),
            "Feat" => Character.AvailableFeats.Contains(prerequisite.Target),
            _ => true
        };
    }

    private bool ValidateAbilityScorePrerequisite(PfPrerequisite prerequisite)
    {
        if (Character == null || !Character.AbilityScores.TryGetValue(prerequisite.Target, out var score))
            return false;

        var requiredScore = int.Parse(prerequisite.Value);
        return prerequisite.Operator switch
        {
            ">=" => score >= requiredScore,
            ">" => score > requiredScore,
            "=" => score == requiredScore,
            "<=" => score <= requiredScore,
            "<" => score < requiredScore,
            _ => false
        };
    }

    private bool ValidateSkillPrerequisite(PfPrerequisite prerequisite)
    {
        if (Character == null || !Character.Proficiencies.TryGetValue(prerequisite.Target, out var proficiency))
            return false;

        var requiredRank = prerequisite.Value switch
        {
            "Trained" => ProficiencyLevel.Trained,
            "Expert" => ProficiencyLevel.Expert,
            "Master" => ProficiencyLevel.Master,
            "Legendary" => ProficiencyLevel.Legendary,
            _ => ProficiencyLevel.Untrained
        };

        return proficiency >= requiredRank;
    }

    private string FormatPrerequisite(PfPrerequisite prerequisite)
    {
        return prerequisite.Type switch
        {
            "AbilityScore" => $"{prerequisite.Target} {prerequisite.Value}+",
            "Skill" => $"{prerequisite.Target} ({prerequisite.Value})",
            "Level" => $"Level {prerequisite.Value}+",
            "Feat" => prerequisite.Target,
            _ => $"{prerequisite.Type}: {prerequisite.Target}"
        };
    }

    public async ValueTask DisposeAsync()
    {
        _objRef?.Dispose();
    }

    private class WindowSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}