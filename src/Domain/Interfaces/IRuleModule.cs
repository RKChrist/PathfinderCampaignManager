using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Interfaces;

public interface IRuleModule
{
    string Name { get; }
    int Priority { get; }
    VariantRuleType RuleType { get; }
    
    void OnScores(Character character, CalculatedCharacter calculated);
    void OnProficiency(Character character, CalculatedCharacter calculated);
    void OnFeats(Character character, CalculatedCharacter calculated);
    void OnSlots(Character character, CalculatedCharacter calculated);
    void OnEncumbrance(Character character, CalculatedCharacter calculated);
    void OnValidation(Character character, CalculatedCharacter calculated);
}

public interface ICalculatedCharacter
{
    Guid Id { get; }
    string Name { get; }
    int Level { get; }
    string ClassName { get; }
    string? SubclassName { get; }
    string? BackgroundName { get; }
    string? AncestryName { get; }
    
    // Ability Scores (after all modifiers)
    Dictionary<string, int> AbilityScores { get; }
    Dictionary<string, int> AbilityModifiers { get; }
    
    // Proficiencies
    Dictionary<string, ProficiencyRank> Proficiencies { get; }
    Dictionary<string, int> ProficiencyBonuses { get; }
    Dictionary<string, int> Skills { get; }
    
    // Feat Slots
    Dictionary<string, List<FeatSlot>> FeatSlots { get; }
    List<string> AvailableFeats { get; }
    List<PfFeat> SelectedFeats { get; }
    IEnumerable<PfSpell> Spells { get; }
    
    // Combat Stats
    int ArmorClass { get; }
    int HitPoints { get; }
    int Initiative { get; }
    int FortitudeSave { get; }
    int ReflexSave { get; }
    int WillSave { get; }
    
    // Encumbrance
    int BulkLimit { get; }
    int CurrentBulk { get; }
    bool IsEncumbered { get; }
    IEnumerable<PfItem> Equipment { get; }
    decimal CarriedWeight { get; }
    decimal CarryingCapacity { get; }
    
    int Perception { get; }
    int Speed { get; }
    
    // Validation
    List<ValidationIssue> ValidationIssues { get; }
    bool IsValid { get; }
}

public class CalculatedCharacter : ICalculatedCharacter
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? SubclassName { get; set; }
    public string? BackgroundName { get; set; }
    public string? AncestryName { get; set; }
    
    public Dictionary<string, int> AbilityScores { get; set; } = new();
    public Dictionary<string, int> AbilityModifiers { get; set; } = new();
    
    public Dictionary<string, ProficiencyRank> Proficiencies { get; set; } = new();
    public Dictionary<string, int> ProficiencyBonuses { get; set; } = new();
    public Dictionary<string, int> Skills { get; set; } = new();
    
    public Dictionary<string, List<FeatSlot>> FeatSlots { get; set; } = new();
    public List<string> AvailableFeats { get; set; } = new();
    public List<PfFeat> SelectedFeats { get; set; } = new();
    public IEnumerable<PfSpell> Spells { get; set; } = new List<PfSpell>();
    
    public int ArmorClass { get; set; }
    public int HitPoints { get; set; }
    public int Initiative { get; set; }
    public int FortitudeSave { get; set; }
    public int ReflexSave { get; set; }
    public int WillSave { get; set; }
    
    public int BulkLimit { get; set; }
    public int CurrentBulk { get; set; }
    public bool IsEncumbered { get; set; }
    public IEnumerable<PfItem> Equipment { get; set; } = new List<PfItem>();
    public decimal CarriedWeight { get; set; }
    public decimal CarryingCapacity { get; set; }
    
    public int Perception { get; set; }
    public int Speed { get; set; }
    
    public List<ValidationIssue> ValidationIssues { get; set; } = new();
    public bool IsValid => ValidationIssues.Count == 0;
}

public class FeatSlot
{
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SelectedFeatId { get; set; }
    public bool IsRequired { get; set; }
}

public class ValidationIssue
{
    public ValidationSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? FixAction { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}