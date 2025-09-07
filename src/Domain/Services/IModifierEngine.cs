using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Domain.Services;

public interface IModifierEngine
{
    CalculatedCharacterStats CalculateCharacterStats(Character character, List<CustomDefinitionModifier> modifiers);
}