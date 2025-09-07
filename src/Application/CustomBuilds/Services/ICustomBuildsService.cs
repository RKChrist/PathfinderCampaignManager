using PathfinderCampaignManager.Application.CustomBuilds.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Services;

namespace PathfinderCampaignManager.Application.CustomBuilds.Services;

public interface ICustomBuildsService
{
    Task<CustomDefinitionDto?> GetCustomDefinitionAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<List<CustomDefinitionDto>> GetUserCustomDefinitionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<CustomDefinitionDto>> SearchCustomDefinitionsAsync(string searchTerm, Guid? userId = null, bool includePublic = true, CancellationToken cancellationToken = default);
    Task<CalculatedCharacterStats> CalculateCharacterStatsWithCustomItemsAsync(Guid characterId, List<Guid> customItemIds, CancellationToken cancellationToken = default);
    Task<CustomDefinitionDto> CreateMagicItemAsync(string name, string description, List<ModifierDto> modifiers, Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> ValidateCustomDefinitionAsync(CustomDefinitionDto definition, CancellationToken cancellationToken = default);
}

// Implementation moved to Infrastructure layer