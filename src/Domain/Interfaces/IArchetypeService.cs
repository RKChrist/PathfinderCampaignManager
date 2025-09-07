using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Domain.Interfaces;

public interface IArchetypeService
{
    // Archetype progression and validation
    Task<Result<bool>> CanTakeArchetypeFeatAsync(string archetypeId, string featId, CalculatedCharacter character);
    Task<Result<bool>> ValidateDedicationPrerequisitesAsync(string archetypeId, CalculatedCharacter character);
    Task<Result<bool>> HasRequiredArchetypeFeatsAsync(string currentArchetypeId, CalculatedCharacter character);
    
    // Multiclass spellcasting progression
    Task<Result<PfMulticlassSpellcasting>> CalculateMulticlassSpellcastingAsync(string archetypeId, int characterLevel);
    Task<Result<Dictionary<int, int>>> GetMulticlassSpellSlotsAsync(string archetypeId, int characterLevel);
    
    // Archetype restrictions and rules
    Task<Result<bool>> CanTakeNewArchetypeAsync(CalculatedCharacter character);
    Task<Result<IEnumerable<string>>> GetBlockedArchetypesAsync(CalculatedCharacter character);
    Task<Result<int>> GetArchetypeFeatCountAsync(string archetypeId, CalculatedCharacter character);
    
    // Archetype feat management
    Task<Result<IEnumerable<PfFeat>>> GetNextAvailableFeatsAsync(string archetypeId, CalculatedCharacter character);
    Task<Result<bool>> ValidateArchetypeProgressionAsync(string archetypeId, CalculatedCharacter character);
    
    // Special archetype rules
    Task<Result<bool>> HasConflictingArchetypesAsync(string proposedArchetypeId, CalculatedCharacter character);
    Task<Result<IEnumerable<ValidationIssue>>> ValidateAllArchetypesAsync(CalculatedCharacter character);
    
    // Archetype benefits calculation
    Task<Result<ArchetypeBenefits>> CalculateArchetypeBenefitsAsync(string archetypeId, int characterLevel);
}

public class ArchetypeBenefits
{
    public Dictionary<string, int> AbilityScoreModifiers { get; set; } = new();
    public Dictionary<string, ProficiencyRank> SkillProficiencies { get; set; } = new();
    public Dictionary<string, ProficiencyRank> WeaponProficiencies { get; set; } = new();
    public Dictionary<string, ProficiencyRank> ArmorProficiencies { get; set; } = new();
    public PfMulticlassSpellcasting? SpellcastingProgression { get; set; }
    public List<string> SpecialAbilities { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public Dictionary<string, int> SpellSlots { get; set; } = new(); // [Level] = Slots
}