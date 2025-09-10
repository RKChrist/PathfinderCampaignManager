using PathfinderCampaignManager.Domain.ValueObjects;
using System.Linq;

namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfClass : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> KeyAbilities { get; set; } = new(); // Multiple key abilities (e.g., Str or Dex for Fighter)
    public int HitPoints { get; set; }
    public int SkillRanks { get; set; } = 2;
    public string Source { get; set; } = "Core Rulebook";
    public string Rarity { get; set; } = "Common";
    public List<string> Traits { get; set; } = new();
    
    // Spellcasting info (if applicable)
    public bool IsSpellcaster { get; set; }
    public string? SpellcastingTradition { get; set; }
    public string? SpellcastingAbility { get; set; }
    public int? SpellsPerDay { get; set; }
    public bool IsPreparedCaster { get; set; }
    public bool IsSpontaneousCaster { get; set; }
    
    // Progressions using the generic system
    public Progression<SaveProgression>? FortitudeProgression { get; set; }
    public Progression<SaveProgression>? ReflexProgression { get; set; }
    public Progression<SaveProgression>? WillProgression { get; set; }
    public Dictionary<string, List<ProficiencyRank>> SaveProgressions { get; set; } = new();
    public Progression<PerceptionProgression>? PerceptionProgression { get; set; }
    public List<Progression<SkillProgression>> SkillProgressions { get; set; } = new();
    public List<Progression<WeaponProgression>> WeaponProgressions { get; set; } = new();
    public List<Progression<ArmorProgression>> ArmorProgressions { get; set; } = new();
    public Progression<SpellcastingProgression>? SpellAttackProgression { get; set; }
    public Progression<SpellcastingProgression>? SpellDcProgression { get; set; }
    public Progression<SpellcastingProgression>? ClassDcProgression { get; set; }
    
    // Class features by level (1-20)
    public Dictionary<int, List<PfClassFeature>> ClassFeaturesByLevel { get; set; } = new();
    
    // Class feat levels (when this class grants class feats)
    public List<int> ClassFeatLevels { get; set; } = new();
    
    // Subclass options (doctrines, orders, instincts, etc.)
    public List<PfSubclass> Subclasses { get; set; } = new();
    
    // Backward compatibility properties for existing UI
    public string KeyAbility => KeyAbilities.Count > 0 ? KeyAbilities[0] : "Strength";
    public List<string> ClassSkills => new List<string>(); // TODO: Implement from skill progressions
    public List<string> ClassFeatures => ClassFeaturesByLevel.Values.SelectMany(features => features.Select(f => f.Name)).ToList();
}

public class PfSubclass
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Doctrine", "Order", "Instinct", etc.
    public Dictionary<int, List<PfClassFeature>> AdditionalFeatures { get; set; } = new();
    public List<string> Traits { get; set; } = new();
}

public class PfClassFeature
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Type { get; set; } = string.Empty; // "Class Feature", "Feat", etc.
    public List<string> Prerequisites { get; set; } = new();
    public string? ActionCost { get; set; } // null, "Free", "1", "2", "3", "Reaction"
    public string? Trigger { get; set; }
    public string? Frequency { get; set; }
    public string? Requirements { get; set; }
    public List<string> Traits { get; set; } = new();
    public string Source { get; set; } = "Core Rulebook";
    
    // For choice-based features
    public List<PfClassFeatureChoice> Choices { get; set; } = new();
}

public class PfClassFeatureChoice
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Choice", "Selection", "Option"
    public List<string> Options { get; set; } = new();
    public int MaxSelections { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
}