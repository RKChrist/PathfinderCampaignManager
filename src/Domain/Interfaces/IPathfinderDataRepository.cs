using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Domain.Interfaces;

public interface IPathfinderDataRepository
{
    // Classes
    Task<Result<PfClass>> GetClassAsync(string id);
    Task<Result<IEnumerable<PfClass>>> GetAllClassesAsync();
    
    // Feats
    Task<Result<PfFeat>> GetFeatAsync(string id);
    Task<Result<IEnumerable<PfFeat>>> GetFeatsAsync(int? level = null, string? type = null, string? sourceId = null);
    Task<Result<IEnumerable<PfFeat>>> GetFeatsByTraitAsync(string trait);
    Task<Result<IEnumerable<PfFeat>>> GetFeatsByPrerequisiteAsync(string prerequisiteType, string prerequisiteValue);
    
    // Spells
    Task<Result<PfSpell>> GetSpellAsync(string id);
    Task<Result<IEnumerable<PfSpell>>> GetSpellsAsync(int? level = null, string? tradition = null, string? school = null);
    Task<Result<IEnumerable<PfSpell>>> GetSpellsByTraitAsync(string trait);
    Task<Result<IEnumerable<PfSpell>>> GetCantripsAsync();
    Task<Result<IEnumerable<PfSpell>>> GetFocusSpellsAsync();
    
    // Ancestries
    Task<Result<PfAncestry>> GetAncestryAsync(string id);
    Task<Result<IEnumerable<PfAncestry>>> GetAllAncestriesAsync();
    Task<Result<IEnumerable<PfHeritage>>> GetHeritagesForAncestryAsync(string ancestryId);
    
    // Backgrounds
    Task<Result<PfBackground>> GetBackgroundAsync(string id);
    Task<Result<IEnumerable<PfBackground>>> GetAllBackgroundsAsync();
    Task<Result<IEnumerable<PfBackground>>> GetBackgroundsByCategoryAsync(string category);
    
    // Archetypes
    Task<Result<PfArchetype>> GetArchetypeAsync(string id);
    Task<Result<IEnumerable<PfArchetype>>> GetAllArchetypesAsync();
    Task<Result<IEnumerable<PfArchetype>>> GetMulticlassArchetypesAsync();
    
    // Traits
    Task<Result<PfTrait>> GetTraitAsync(string id);
    Task<Result<IEnumerable<PfTrait>>> GetAllTraitsAsync();
    Task<Result<IEnumerable<PfTrait>>> GetTraitsByCategoryAsync(string category);
    
    // Monsters
    Task<Result<PfMonster>> GetMonsterByIdAsync(string id);
    Task<Result<IEnumerable<PfMonster>>> GetMonstersAsync();
    Task<Result<IEnumerable<PfMonster>>> SearchMonstersAsync(string searchTerm, int? level = null);
    
    // Validation and cross-references
    Task<Result<bool>> ValidatePrerequisitesAsync(string characterId, string featId);
    Task<Result<IEnumerable<string>>> GetMissingIds(IEnumerable<string> ids, string entityType);
    Task<Result<Dictionary<string, int>>> GetDataCompletionStats();
}

public interface IPathfinderValidationService
{
    Task<Result<bool>> ValidateFeatPrerequisites(string characterId, PfFeat feat);
    Task<Result<bool>> ValidateArchetypeProgression(string characterId, string archetypeId);
    Task<Result<bool>> ValidateSpellAccess(string characterId, string spellId, int level);
    Task<Result<IEnumerable<string>>> GetAvailableFeats(string characterId, int level, string type);
    Task<Result<IEnumerable<string>>> GetAvailableSpells(string characterId, int level, string tradition);
    Task<Result<bool>> CheckDataIntegrity();
}