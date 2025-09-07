using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Domain.Interfaces;

public interface IArchetypeRepository
{
    // Core archetype operations
    Task<Result<PfArchetype>> GetArchetypeByIdAsync(string id);
    Task<Result<IEnumerable<PfArchetype>>> GetArchetypesAsync();
    Task<Result<IEnumerable<PfArchetype>>> GetArchetypesByTypeAsync(ArchetypeType type);
    
    // Multiclass archetypes
    Task<Result<IEnumerable<PfArchetype>>> GetMulticlassArchetypesAsync();
    Task<Result<PfArchetype>> GetMulticlassArchetypeForClassAsync(string classId);
    
    // Class archetypes
    Task<Result<IEnumerable<PfArchetype>>> GetClassArchetypesAsync(string classId);
    
    // General archetypes
    Task<Result<IEnumerable<PfArchetype>>> GetGeneralArchetypesAsync();
    
    // Dedication feats
    Task<Result<PfFeat>> GetDedicationFeatAsync(string archetypeId);
    Task<Result<IEnumerable<PfFeat>>> GetArchetypeFeatsAsync(string archetypeId);
    
    // Archetype prerequisites and validation
    Task<Result<bool>> ValidatePrerequisitesAsync(string archetypeId, CalculatedCharacter character);
    Task<Result<IEnumerable<PfFeat>>> GetAvailableArchetypeFeatsAsync(string archetypeId, CalculatedCharacter character);
    
    // Search and filtering
    Task<Result<IEnumerable<PfArchetype>>> SearchArchetypesAsync(string searchTerm);
    Task<Result<IEnumerable<PfArchetype>>> GetArchetypesBySourceAsync(string source);
    Task<Result<IEnumerable<PfArchetype>>> GetArchetypesByTraitAsync(string trait);
}