namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfBackground : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Ability score boosts
    public List<string> AbilityBoosts { get; set; } = new(); // Usually 2 specific boosts
    public int FreeAbilityBoosts { get; set; } = 0; // Number of free choice boosts
    
    // Skills and lore
    public List<string> SkillTraining { get; set; } = new(); // Skills trained by this background
    public string? LoreSkill { get; set; } // Specific lore skill (e.g., "Academia Lore")
    
    // Feat granted
    public string? GrantedFeat { get; set; } // Feat ID granted by this background
    public List<string> FeatChoices { get; set; } = new(); // If multiple feats to choose from
    
    // Languages and equipment
    public List<string> Languages { get; set; } = new();
    public List<string> Equipment { get; set; } = new();
    
    // Additional features
    public List<PfBackgroundFeature> Features { get; set; } = new();
    
    // Metadata
    public string Source { get; set; } = "Core Rulebook";
    public string Rarity { get; set; } = "Common";
    public List<string> Traits { get; set; } = new();
    public string Category { get; set; } = string.Empty; // "General", "Regional", "Campaign", etc.
    
    // Backward compatibility properties for existing UI
    public List<string> SkillProficiencies => SkillTraining;
}

public class PfBackgroundFeature
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Feature", "Choice", "Equipment", etc.
    public List<string> Options { get; set; } = new(); // For choice-based features
    public bool IsOptional { get; set; } = false;
}