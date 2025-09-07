namespace PathfinderCampaignManager.Domain.Enums;

/// <summary>
/// Types of custom definitions that can be created
/// </summary>
public enum CustomDefinitionType
{
    Class = 1,
    Archetype = 2,
    Feat = 3,
    Spell = 4,
    Item = 5,
    Weapon = 6,
    Armor = 7,
    Operation = 8,
    Background = 9,
    Ancestry = 10,
    Heritage = 11,
    Trait = 12
}

/// <summary>
/// What attribute or stat a modifier targets
/// </summary>
public enum ModifierTarget
{
    // Ability Scores
    Strength = 1,
    Dexterity = 2,
    Constitution = 3,
    Intelligence = 4,
    Wisdom = 5,
    Charisma = 6,
    
    // Derived Stats
    ArmorClass = 10,
    HitPoints = 11,
    Initiative = 12,
    Speed = 13,
    
    // Saving Throws
    FortitudeSave = 20,
    ReflexSave = 21,
    WillSave = 22,
    
    // Skills (common ones)
    Acrobatics = 30,
    Arcana = 31,
    Athletics = 32,
    Crafting = 33,
    Deception = 34,
    Diplomacy = 35,
    Intimidation = 36,
    Lore = 37,
    Medicine = 38,
    Nature = 39,
    Occultism = 40,
    Perception = 41,
    Performance = 42,
    Religion = 43,
    Society = 44,
    Stealth = 45,
    Survival = 46,
    Thievery = 47,
    
    // Attack Stats
    AttackBonus = 50,
    DamageBonus = 51,
    SpellAttackBonus = 52,
    SpellDC = 53,
    
    // Resistances/Immunities
    AllDamageResistance = 60,
    PhysicalResistance = 61,
    FireResistance = 62,
    ColdResistance = 63,
    ElectricityResistance = 64,
    AcidResistance = 65,
    SonicResistance = 66,
    
    // Other
    CarryingCapacity = 70,
    LandSpeed = 71,
    SwimSpeed = 72,
    ClimbSpeed = 73,
    FlySpeed = 74
}

/// <summary>
/// Type of modifier for stacking rules
/// </summary>
public enum ModifierType
{
    Untyped = 1,
    Item = 2,
    Enhancement = 3,
    Status = 4,
    Circumstance = 5,
    Competence = 6,
    Deflection = 7,
    Dodge = 8,
    Insight = 9,
    Luck = 10,
    Morale = 11,
    Natural = 12,
    Profane = 13,
    Racial = 14,
    Resistance = 15,
    Sacred = 16,
    Size = 17,
    Alchemical = 18
}

/// <summary>
/// Rarity levels for custom content
/// </summary>
public enum CustomRarity
{
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Unique = 4
}

/// <summary>
/// Categories for organizing custom content
/// </summary>
public static class CustomCategories
{
    // Class Categories
    public const string CoreClass = "Core Class";
    public const string BaseClass = "Base Class";
    public const string HybridClass = "Hybrid Class";
    public const string PrestigeClass = "Prestige Class";
    
    // Item Categories
    public const string MagicItem = "Magic Item";
    public const string Weapon = "Weapon";
    public const string Armor = "Armor";
    public const string Shield = "Shield";
    public const string Consumable = "Consumable";
    public const string Tool = "Tool";
    public const string Adventuring_Gear = "Adventuring Gear";
    
    // Feat Categories
    public const string GeneralFeat = "General";
    public const string SkillFeat = "Skill";
    public const string AncestryFeat = "Ancestry";
    public const string ClassFeat = "Class";
    public const string ArchetypeFeat = "Archetype";
    
    // Spell Categories
    public const string Cantrip = "Cantrip";
    public const string Spell = "Spell";
    public const string Focus_Spell = "Focus Spell";
    public const string Ritual = "Ritual";
    
    // Operation Categories
    public const string Downtime = "Downtime";
    public const string Exploration = "Exploration";
    public const string Encounter = "Encounter";
    public const string Environment = "Environment";
}

/// <summary>
/// Common trait tags for custom content
/// </summary>
public static class CustomTraits
{
    // Magic Schools
    public const string Abjuration = "Abjuration";
    public const string Conjuration = "Conjuration";
    public const string Divination = "Divination";
    public const string Enchantment = "Enchantment";
    public const string Evocation = "Evocation";
    public const string Illusion = "Illusion";
    public const string Necromancy = "Necromancy";
    public const string Transmutation = "Transmutation";
    
    // Damage Types
    public const string Fire = "Fire";
    public const string Cold = "Cold";
    public const string Electricity = "Electricity";
    public const string Acid = "Acid";
    public const string Sonic = "Sonic";
    public const string Force = "Force";
    public const string Positive = "Positive";
    public const string Negative = "Negative";
    
    // Action Types
    public const string Attack = "Attack";
    public const string Concentrate = "Concentrate";
    public const string Manipulate = "Manipulate";
    public const string Move = "Move";
    public const string Exploration = "Exploration";
    public const string Downtime = "Downtime";
    
    // Item Traits
    public const string Magical = "Magical";
    public const string Invested = "Invested";
    public const string Consumable = "Consumable";
    public const string Alchemical = "Alchemical";
    
    // Weapon Traits
    public const string Agile = "Agile";
    public const string Deadly = "Deadly";
    public const string Fatal = "Fatal";
    public const string Finesse = "Finesse";
    public const string Reach = "Reach";
    public const string Thrown = "Thrown";
    public const string Versatile = "Versatile";
    
    // Armor Traits
    public const string Bulwark = "Bulwark";
    public const string Flexible = "Flexible";
    public const string Noisy = "Noisy";
    
    // Rarity Traits
    public const string Common = "Common";
    public const string Uncommon = "Uncommon";
    public const string Rare = "Rare";
    public const string Unique = "Unique";
}

/// <summary>
/// Validation constants for custom builds
/// </summary>
public static class CustomBuildConstants
{
    public const int MAX_NAME_LENGTH = 100;
    public const int MAX_DESCRIPTION_LENGTH = 2000;
    public const int MAX_JSON_SIZE = 50000; // 50KB
    public const int MAX_TRAITS_COUNT = 20;
    public const int MAX_TAGS_COUNT = 10;
    public const int MAX_MODIFIERS_PER_ITEM = 10;
    public const int MIN_LEVEL = 0;
    public const int MAX_LEVEL = 20;
    public const int MAX_MODIFIER_VALUE = 50;
    public const int MIN_MODIFIER_VALUE = -20;
}