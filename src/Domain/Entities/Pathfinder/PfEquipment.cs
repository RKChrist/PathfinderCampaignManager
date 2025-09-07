namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PfItemType Type { get; set; }
    public int Level { get; set; }
    public string Price { get; set; } = string.Empty;
    public string Bulk { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    public string Rarity { get; set; } = "Common"; // Common, Uncommon, Rare, Unique
    public string Source { get; set; } = string.Empty;
}

public class PfWeapon : PfItem
{
    public string Category { get; set; } = string.Empty; // Simple, Martial, Advanced
    public string Group { get; set; } = string.Empty; // Sword, Bow, etc.
    public string Damage { get; set; } = string.Empty; // "1d8 S"
    public string DamageType { get; set; } = string.Empty; // Slashing, Piercing, Bludgeoning
    public int Range { get; set; } = 0; // 0 for melee
    public string Reload { get; set; } = string.Empty;
    public List<string> WeaponTraits { get; set; } = new();
    public bool IsRanged => Range > 0;
}

// PfArmor and PfShield classes are defined in PfArmor.cs to avoid duplication

public class PfConsumable : PfItem
{
    public string Usage { get; set; } = string.Empty;
    public string Activate { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
}

public enum PfItemType
{
    Weapon,
    Armor,
    Shield,
    Consumable,
    Tool,
    Gear,
    Treasure,
    Accessory,
    Rune,
    Material,
    Service
}