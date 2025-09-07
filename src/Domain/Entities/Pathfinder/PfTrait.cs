namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfTrait : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Magical", "Attack", "Emotion", "Aura", etc.
    public List<string> AppliesTo { get; set; } = new(); // "Spells", "Feats", "Equipment", etc.
    public string Source { get; set; } = "Core Rulebook";
    
    // Special trait properties
    public bool HasMechanicalEffect { get; set; } = false;
    public string? MechanicalEffect { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public static class TraitRegistry
{
    // Common trait categories
    public const string MAGICAL = "magical";
    public const string ATTACK = "attack";
    public const string EMOTION = "emotion";
    public const string FEAR = "fear";
    public const string MENTAL = "mental";
    public const string VISUAL = "visual";
    public const string AUDITORY = "auditory";
    public const string LINGUISTIC = "linguistic";
    public const string AURA = "aura";
    public const string INCAPACITATION = "incapacitation";
    public const string DEATH = "death";
    public const string HEALING = "healing";
    public const string NECROMANCY = "necromancy";
    public const string DIVINATION = "divination";
    public const string ENCHANTMENT = "enchantment";
    public const string EVOCATION = "evocation";
    public const string ILLUSION = "illusion";
    public const string CONJURATION = "conjuration";
    public const string TRANSMUTATION = "transmutation";
    public const string ABJURATION = "abjuration";
    
    // Damage type traits
    public const string FIRE = "fire";
    public const string COLD = "cold";
    public const string ELECTRICITY = "electricity";
    public const string ACID = "acid";
    public const string SONIC = "sonic";
    public const string POSITIVE = "positive";
    public const string NEGATIVE = "negative";
    public const string FORCE = "force";
    
    // Physical damage traits
    public const string PIERCING = "piercing";
    public const string SLASHING = "slashing";
    public const string BLUDGEONING = "bludgeoning";
    
    // Size traits
    public const string TINY = "tiny";
    public const string SMALL = "small";
    public const string MEDIUM = "medium";
    public const string LARGE = "large";
    public const string HUGE = "huge";
    public const string GARGANTUAN = "gargantuan";
    
    // Ancestry traits
    public const string HUMAN = "human";
    public const string ELF = "elf";
    public const string DWARF = "dwarf";
    public const string HALFLING = "halfling";
    public const string GNOME = "gnome";
    public const string GOBLIN = "goblin";
    public const string ORC = "orc";
    public const string HALFELF = "half-elf";
    public const string HALFORC = "half-orc";
    public const string CATFOLK = "catfolk";
    public const string KOBOLD = "kobold";
    
    // Class traits
    public const string ALCHEMIST = "alchemist";
    public const string BARBARIAN = "barbarian";
    public const string BARD = "bard";
    public const string CHAMPION = "champion";
    public const string CLERIC = "cleric";
    public const string DRUID = "druid";
    public const string FIGHTER = "fighter";
    public const string MONK = "monk";
    public const string RANGER = "ranger";
    public const string ROGUE = "rogue";
    public const string SORCERER = "sorcerer";
    public const string WIZARD = "wizard";
    
    // Feat type traits
    public const string GENERAL = "general";
    public const string SKILL = "skill";
    public const string ARCHETYPE = "archetype";
    public const string CLASS_FEAT = "class";
    public const string ANCESTRY_FEAT = "ancestry";
}