using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Presentation.Client.Components.HoverCards;

namespace PathfinderCampaignManager.Presentation.Client.Services;

public class HoverCardService : IHoverCardService
{
    public event Func<string, double, double, ICalculatedCharacter?, Task>? ShowSpellCard;
    public event Func<string, double, double, ICalculatedCharacter?, Task>? ShowFeatCard;
    public event Func<Task>? HideAllCards;
    public event Func<int, Task>? ScheduleHide;

    public async Task ShowSpellCardAsync(string spellId, double x, double y, ICalculatedCharacter? character = null)
    {
        if (ShowSpellCard != null)
        {
            await ShowSpellCard.Invoke(spellId, x, y, character);
        }
    }

    public async Task ShowFeatCardAsync(string featId, double x, double y, ICalculatedCharacter? character = null)
    {
        if (ShowFeatCard != null)
        {
            await ShowFeatCard.Invoke(featId, x, y, character);
        }
    }

    public async Task HideAllCardsAsync()
    {
        if (HideAllCards != null)
        {
            await HideAllCards.Invoke();
        }
    }

    public async Task ScheduleHideAsync(int delayMs = 200)
    {
        if (ScheduleHide != null)
        {
            await ScheduleHide.Invoke(delayMs);
        }
    }

    // Non-async wrapper methods for components that need them
    public void ShowSpellCardSync(string spellId, double x, double y, ICalculatedCharacter? character = null)
    {
        _ = Task.Run(() => ShowSpellCardAsync(spellId, x, y, character));
    }

    public void ShowFeatCardSync(string featId, double x, double y, ICalculatedCharacter? character = null)
    {
        _ = Task.Run(() => ShowFeatCardAsync(featId, x, y, character));
    }

    public void HideAllCardsSync()
    {
        _ = Task.Run(() => HideAllCardsAsync());
    }

    public void ScheduleHideSync(int delayMs = 200)
    {
        _ = Task.Run(() => ScheduleHideAsync(delayMs));
    }

    public void Dispose()
    {
        ShowSpellCard = null;
        ShowFeatCard = null;
        HideAllCards = null;
        ScheduleHide = null;
    }
}