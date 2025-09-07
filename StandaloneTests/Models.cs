namespace StandalonePF2eTests;

public enum AbilityScore
{
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Wisdom,
    Charisma
}

public enum ProficiencyLevel
{
    Untrained = 0,
    Trained = 2,
    Expert = 4,
    Master = 6,
    Legendary = 8
}

public class CharacterBuilder
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public PathfinderClass? SelectedClass { get; set; }
    public Dictionary<AbilityScore, int> AbilityScores { get; set; } = new();
    public List<PathfinderFeat> SelectedFeats { get; set; } = new();
    
    public int GetAbilityModifier(AbilityScore ability)
    {
        if (!AbilityScores.TryGetValue(ability, out var score))
            return 0;
            
        return (score - 10) / 2;
    }
}

public class PathfinderClass
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HitPoints { get; set; }
    public List<AbilityScore> KeyAbilities { get; set; } = new();
    public int SkillPoints { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public List<string> Traits { get; set; } = new();
    public bool IsSpellcaster { get; set; }
    public string? SpellcastingTradition { get; set; }
    public string? SpellcastingAbility { get; set; }
    public ClassProficiencies InitialProficiencies { get; set; } = new();
    public List<string> ClassFeatures { get; set; } = new();
}

public class PathfinderFeat
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public string FeatType { get; set; } = string.Empty;
    public int? ActionCost { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public List<string> Traits { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public List<string> Benefits { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
}

public class PathfinderSkill
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AbilityScore KeyAbility { get; set; }
    public string SkillType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public List<SkillAction> Actions { get; set; } = new();
}

public class SkillAction
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ActionCost { get; set; } = 1;
    public bool RequiresTrained { get; set; }
    public Dictionary<string, int> CommonDCs { get; set; } = new();
    public List<string> Traits { get; set; } = new();
}

public class ClassProficiencies
{
    public ProficiencyLevel Perception { get; set; } = ProficiencyLevel.Untrained;
    public ProficiencyLevel FortitudeSave { get; set; } = ProficiencyLevel.Untrained;
    public ProficiencyLevel ReflexSave { get; set; } = ProficiencyLevel.Untrained;
    public ProficiencyLevel WillSave { get; set; } = ProficiencyLevel.Untrained;
    public Dictionary<string, ProficiencyLevel> Skills { get; set; } = new();
    public Dictionary<string, ProficiencyLevel> Weapons { get; set; } = new();
    public Dictionary<string, ProficiencyLevel> Armor { get; set; } = new();
    public ProficiencyLevel SpellAttacks { get; set; } = ProficiencyLevel.Untrained;
    public ProficiencyLevel SpellDCs { get; set; } = ProficiencyLevel.Untrained;
}