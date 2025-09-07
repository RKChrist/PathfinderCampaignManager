namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfAncestry : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HitPoints { get; set; }
    public string Size { get; set; } = string.Empty; // "Small", "Medium", "Large"
    public Dictionary<string, int> Speeds { get; set; } = new(); // "land": 25, "fly": 25, etc.
    
    // Ability score adjustments
    public List<string> AbilityBoosts { get; set; } = new(); // "Strength", "Dexterity", etc.
    public List<string> AbilityFlaws { get; set; } = new(); // "Constitution", etc.
    public List<string> FreeAbilityBoosts { get; set; } = new(); // Number of free boosts to any ability
    
    // Languages and senses
    public List<string> Languages { get; set; } = new();
    public int AdditionalLanguages { get; set; } = 0;
    public List<string> Senses { get; set; } = new(); // "darkvision", "low-light vision", etc.
    public Dictionary<string, int> SenseRanges { get; set; } = new(); // "darkvision": 60, etc.
    
    // Traits and features
    public List<string> Traits { get; set; } = new();
    public List<PfAncestryFeature> Features { get; set; } = new();
    
    // Heritage options
    public List<PfHeritage> Heritages { get; set; } = new();
    
    // Ancestry feats by level
    public Dictionary<int, List<string>> AncestryFeatLevels { get; set; } = new();
    
    // Metadata
    public string Source { get; set; } = "Core Rulebook";
    public string Rarity { get; set; } = "Common";
    
    // Backward compatibility properties for existing UI
    public int Speed => Speeds.GetValueOrDefault("land", 25);
}

public class PfHeritage
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    public string AncestryId { get; set; } = string.Empty;
    public List<PfAncestryFeature> Features { get; set; } = new();
    public string Source { get; set; } = "Core Rulebook";
    public string Rarity { get; set; } = "Common";
    
    // Heritage-specific modifiers
    public List<string> AbilityBoosts { get; set; } = new();
    public List<string> AbilityFlaws { get; set; } = new();
    public Dictionary<string, int> AdditionalSpeeds { get; set; } = new();
    public List<string> AdditionalSenses { get; set; } = new();
    public List<string> AdditionalLanguages { get; set; } = new();
}

public class PfAncestryFeature
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    public bool IsOptional { get; set; } = false;
    public List<PfAncestryFeatureChoice> Choices { get; set; } = new();
}

public class PfAncestryFeatureChoice
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Language", "Skill", "Resistance", etc.
    public List<string> Options { get; set; } = new();
    public int MaxSelections { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
}