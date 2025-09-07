using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Presentation.Client.Components.HoverCards;

public partial class SpellHoverCard : ComponentBase, IAsyncDisposable
{
    [Parameter] public PfSpell? Spell { get; set; }
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public bool ShowViewDetailsButton { get; set; } = true;
    [Parameter] public EventCallback<PfSpell> OnViewDetails { get; set; }
    [Parameter] public EventCallback OnMouseEnter { get; set; }
    [Parameter] public EventCallback OnMouseLeave { get; set; }

    private (double X, double Y) _position = (0, 0);
    private DotNetObjectReference<SpellHoverCard>? _objRef;

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
            const int cardWidth = 400;
            const int cardHeight = 300; // Approximate
            
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
        if (Spell != null)
        {
            await OnViewDetails.InvokeAsync(Spell);
        }
    }

    private string GetSpellLevelText()
    {
        if (Spell == null) return "";
        
        return Spell.Level switch
        {
            0 => "Cantrip",
            _ => $"Level {Spell.Level}"
        };
    }

    private string GetCastingTime()
    {
        if (Spell == null) return "";
        
        var components = new List<string>();
        
        if (!string.IsNullOrEmpty(Spell.ActionCost))
        {
            components.Add(Spell.ActionCost);
        }
        
        if (Spell.Components.Any())
        {
            var componentStr = string.Join(", ", Spell.Components);
            components.Add($"({componentStr})");
        }
        
        return string.Join(" ", components);
    }

    private string FormatDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            return "";
        
        // Truncate long descriptions for hover cards
        const int maxLength = 300;
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