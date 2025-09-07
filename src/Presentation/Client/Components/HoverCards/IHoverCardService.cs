using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Presentation.Client.Components.HoverCards;

public interface IHoverCardService : IDisposable
{
    Task ShowSpellCardAsync(string spellId, double x, double y, ICalculatedCharacter? character = null);
    Task ShowFeatCardAsync(string featId, double x, double y, ICalculatedCharacter? character = null);
    Task HideAllCardsAsync();
    Task ScheduleHideAsync(int delayMs = 200);
}