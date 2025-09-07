namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfCharacter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    
    // Core Stats
    public PfAbilityScores AbilityScores { get; set; } = new();
    public string Ancestry { get; set; } = string.Empty;
    public string Heritage { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    
    // Combat Stats
    public int ArmorClass { get; set; }
    public int HitPoints { get; set; }
    public int CurrentHitPoints { get; set; }
    public int Initiative { get; set; }
    
    // Saves
    public PfProficiency FortitudeSave { get; set; } = new();
    public PfProficiency ReflexSave { get; set; } = new();
    public PfProficiency WillSave { get; set; } = new();
    
    // Perception
    public PfProficiency Perception { get; set; } = new();
    
    // Skills
    public List<PfCharacterSkill> Skills { get; set; } = new();
    
    // Feats
    public List<string> SelectedFeats { get; set; } = new();
    public List<string> ClassFeatures { get; set; } = new();
    
    // Equipment
    public List<string> Equipment { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class PfAbilityScores
{
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
    
    public int GetModifier(int score) => (score - 10) / 2;
}


public class PfCharacterSkill
{
    public string SkillName { get; set; } = string.Empty;
    public PfProficiency Proficiency { get; set; } = new();
    public bool IsLore { get; set; }
    public string LoreSubject { get; set; } = string.Empty;
}

// ProficiencyRank is defined in PfProficiency.cs - use unified system