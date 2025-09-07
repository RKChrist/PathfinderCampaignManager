using PathfinderCampaignManager.Domain.ValueObjects;

namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfFeat : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Type { get; set; } = string.Empty; // "General", "Skill", "Class", "Ancestry", "Archetype"
    public List<PfPrerequisite> Prerequisites { get; set; } = new();
    public List<string> Traits { get; set; } = new();
    public string? ActionCost { get; set; } // null, "Free", "1", "2", "3", "Reaction"
    public string? Frequency { get; set; }
    public string? Trigger { get; set; }
    public string? Requirements { get; set; }
    public string Source { get; set; } = "Core Rulebook";
    public string Rarity { get; set; } = "Common";
    
    // Additional categorization
    public List<string> Tags { get; set; } = new(); // "utility", "offense", "defense", "movement", "skill"
    public bool IsArchetypeFeat { get; set; }
    public string? ArchetypeId { get; set; }
    public bool IsMulticlassFeat { get; set; }
    
    // Mechanical effects
    public List<PfFeatEffect> Effects { get; set; } = new();
    
    // Special feat properties
    public bool IsSpecial { get; set; } // Has special text that can't be captured in effects
    public string? SpecialText { get; set; }
    
    // For feats with choices (like Skill Training)
    public List<PfFeatChoice> Choices { get; set; } = new();
}

public class PfPrerequisite
{
    public string Type { get; set; } = string.Empty; // "Level", "Proficiency", "Feat", "Class", "Ancestry", "Skill", etc.
    public string Target { get; set; } = string.Empty; // The specific thing being checked
    public string Operator { get; set; } = string.Empty; // ">=", "=", "!=", "contains", etc.
    public string Value { get; set; } = string.Empty; // The required value
    public string? Alternative { get; set; } // For "or" conditions
}

public class PfFeatEffect
{
    public string Type { get; set; } = string.Empty; // "Modifier", "Grant", "Allow", "Replace", etc.
    public string Target { get; set; } = string.Empty; // What is being affected
    public string Value { get; set; } = string.Empty; // The effect value
    public string? Condition { get; set; } // When this effect applies
    public Dictionary<string, object> Parameters { get; set; } = new(); // Additional parameters
}

public class PfFeatChoice
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Skill", "Language", "Terrain", etc.
    public List<string> Options { get; set; } = new(); // Available choices
    public int MaxSelections { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
    public string? Filter { get; set; } // Additional filtering criteria
}