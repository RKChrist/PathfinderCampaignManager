using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Application.CharacterCreation.Models;

public class CharacterCreationSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid CampaignId { get; set; }
    public CharacterCreationStep CurrentStep { get; set; } = CharacterCreationStep.ClassSelection;
    public CharacterBuilder CharacterData { get; set; } = new();
    public Dictionary<string, object> StepData { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; }
}

public class CharacterBuilder
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; } = 1;

    // Core Choices
    public PathfinderClass? SelectedClass { get; set; }
    public PathfinderAncestry? SelectedAncestry { get; set; }
    public PathfinderBackground? SelectedBackground { get; set; }
    
    // Ability Scores
    public Dictionary<AbilityScore, int> AbilityScores { get; set; } = new();
    public AbilityScoreMethod? AbilityScoreMethod { get; set; }
    
    // Skills
    public List<string> SelectedSkills { get; set; } = new();
    public Dictionary<string, int> SkillRanks { get; set; } = new();
    
    // Feats
    public List<PathfinderFeat> SelectedFeats { get; set; } = new();
    
    // Equipment
    public List<PathfinderItem> StartingEquipment { get; set; } = new();
    public int StartingGold { get; set; }
    
    // Spells (for casters)
    public List<PathfinderSpell> KnownSpells { get; set; } = new();
    
    // Class-specific choices
    public Dictionary<string, object> ClassSpecificChoices { get; set; } = new();
    
    // Validation
    public List<string> ValidationErrors { get; set; } = new();
    public bool IsValid => !ValidationErrors.Any();
    
    public int GetAbilityModifier(AbilityScore ability)
    {
        if (!AbilityScores.TryGetValue(ability, out var score))
            return 0;
            
        return (score - 10) / 2;
    }
    
    public void AddValidationError(string error)
    {
        if (!ValidationErrors.Contains(error))
        {
            ValidationErrors.Add(error);
        }
    }
    
    public void ClearValidationErrors()
    {
        ValidationErrors.Clear();
    }
}

public enum CharacterCreationStep
{
    ClassSelection = 1,
    AncestrySelection = 2,
    BackgroundSelection = 3,
    AbilityScores = 4,
    SkillSelection = 5,
    FeatSelection = 6,
    EquipmentSelection = 7,
    SpellSelection = 8,
    Finalization = 9,
    Review = 10
}

public enum AbilityScore
{
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Wisdom,
    Charisma
}

public enum AbilityScoreMethod
{
    PointBuy,
    StandardArray,
    Manual,
    Rolled
}

// Pathfinder Game Data Models
public class PathfinderClass
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    public string Rarity { get; set; } = "Common";
    public string Source { get; set; } = string.Empty;
    
    // Class Mechanics
    public int HitPoints { get; set; }
    public List<AbilityScore> KeyAbilities { get; set; } = new();
    public int SkillPoints { get; set; }
    public List<string> ClassSkills { get; set; } = new();
    public string SavingThrowProgression { get; set; } = string.Empty;
    public ClassProficiencies InitialProficiencies { get; set; } = new();
    public List<string> ClassFeatures { get; set; } = new();
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    
    // Spellcasting
    public bool IsSpellcaster { get; set; }
    public string? SpellcastingTradition { get; set; }
    public string? SpellcastingAbility { get; set; }
    
    // Visual
    public string IconClass { get; set; } = string.Empty;
    public string ColorTheme { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    
    // Roleplay
    public List<string> RoleplayTips { get; set; } = new();
    public List<string> BuildSuggestions { get; set; } = new();
}

public class PathfinderAncestry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HitPoints { get; set; }
    public string Size { get; set; } = "Medium";
    public int Speed { get; set; } = 25;
    public List<AbilityBoost> AbilityBoosts { get; set; } = new();
    public AbilityScore? AbilityFlaw { get; set; }
    public List<string> Languages { get; set; } = new();
    public List<string> AdditionalLanguages { get; set; } = new();
    public List<string> Traits { get; set; } = new();
    public List<string> SpecialAbilities { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    
    // Visual
    public string ImageUrl { get; set; } = string.Empty;
    
    // Available Heritages
    public List<PathfinderHeritage> Heritages { get; set; } = new();
}

public class PathfinderHeritage
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AncestryId { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    public string Rarity { get; set; } = "Common";
}

public class PathfinderBackground
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<AbilityBoost> AbilityBoosts { get; set; } = new();
    public List<string> SkillProficiencies { get; set; } = new();
    public string? LoreSkill { get; set; }
    public string? Feat { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public List<string> Traits { get; set; } = new();
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class PathfinderFeat
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public string FeatType { get; set; } = string.Empty; // General, Skill, Class, Ancestry, etc.
    public int? ActionCost { get; set; } // 1, 2, 3, or null for passive
    public string Source { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public List<string> Traits { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public List<string> Benefits { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
    public string? Frequency { get; set; }
    public string? Trigger { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class PathfinderSpell
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> Traits { get; set; } = new();
    public string School { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public string Source { get; set; } = string.Empty;
    
    // Casting
    public List<string> Traditions { get; set; } = new();
    public string Cast { get; set; } = string.Empty;
    public List<string> Components { get; set; } = new();
    public string Range { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Targets { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string SavingThrow { get; set; } = string.Empty;
}

public class PathfinderItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public string Source { get; set; } = string.Empty;
    
    // Item Properties
    public int Level { get; set; }
    public string Price { get; set; } = string.Empty;
    public string Bulk { get; set; } = string.Empty;
    
    // Weapon Properties
    public string? Damage { get; set; }
    public string? DamageType { get; set; }
    public string? WeaponGroup { get; set; }
    
    // Armor Properties
    public int? ArmorClass { get; set; }
    public int? DexCap { get; set; }
    public int? CheckPenalty { get; set; }
    public int? SpeedPenalty { get; set; }
}

public class PathfinderSkill
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AbilityScore KeyAbility { get; set; }
    public string SkillType { get; set; } = string.Empty; // Physical, Mental, Social
    public string Source { get; set; } = string.Empty;
    public List<SkillAction> Actions { get; set; } = new();
    public Dictionary<string, string> CommonDCs { get; set; } = new();
    public List<string> UntrainedUses { get; set; } = new();
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class SkillAction
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ActionCost { get; set; } = 1;
    public bool RequiresTrained { get; set; }
    public string? Requirements { get; set; }
    public Dictionary<string, int> CommonDCs { get; set; } = new();
    public List<string> Traits { get; set; } = new();
}

public class ClassProficiencies
{
    public ProficiencyRank Perception { get; set; } = ProficiencyRank.Untrained;
    public ProficiencyRank FortitudeSave { get; set; } = ProficiencyRank.Untrained;
    public ProficiencyRank ReflexSave { get; set; } = ProficiencyRank.Untrained;
    public ProficiencyRank WillSave { get; set; } = ProficiencyRank.Untrained;
    public Dictionary<string, ProficiencyRank> Skills { get; set; } = new();
    public Dictionary<string, ProficiencyRank> Weapons { get; set; } = new();
    public Dictionary<string, ProficiencyRank> Armor { get; set; } = new();
    public ProficiencyRank SpellAttacks { get; set; } = ProficiencyRank.Untrained;
    public ProficiencyRank SpellDCs { get; set; } = ProficiencyRank.Untrained;
    public Dictionary<string, ProficiencyRank> ClassDCs { get; set; } = new();
}

public class AbilityBoost
{
    public AbilityScore? SpecificAbility { get; set; }
    public List<AbilityScore> ChoiceOptions { get; set; } = new();
    public bool IsFree { get; set; } // Any ability score
    public string Description { get; set; } = string.Empty;
}

