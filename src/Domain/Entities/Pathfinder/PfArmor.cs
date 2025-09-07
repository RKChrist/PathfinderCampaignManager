namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfArmor
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PfItemType Type { get; set; } = PfItemType.Armor;
    public int Level { get; set; }
    public string Price { get; set; } = string.Empty;
    public string Bulk { get; set; } = string.Empty;
    public ArmorCategory Category { get; set; }
    public int ArmorClass { get; set; }
    public int DexCap { get; set; }
    public int CheckPenalty { get; set; }
    public int SpeedPenalty { get; set; }
    public int Strength { get; set; }
    public string Group { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    public List<string> ArmorTraits { get; set; } = new();
    public string Source { get; set; } = "Core Rulebook";
}

public enum ArmorCategory
{
    Unarmored,
    Light,
    Medium,
    Heavy
}

// Removed - now using unified PfProficiency system

public class PfShield
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PfItemType Type { get; set; } = PfItemType.Shield;
    public int Level { get; set; }
    public string Price { get; set; } = string.Empty;
    public string Bulk { get; set; } = string.Empty;
    public int ArmorClass { get; set; }
    public int HardnessPoints { get; set; }
    public int BrokenThreshold { get; set; }
    public int HitPoints { get; set; }
    public List<string> Traits { get; set; } = new();
    public string Source { get; set; } = "Core Rulebook";
}

public class PfEquipment
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PfItemType Type { get; set; }
    public int Level { get; set; }
    public string Price { get; set; } = string.Empty;
    public string Bulk { get; set; } = string.Empty;
    public EquipmentCategory Category { get; set; }
    public List<string> Traits { get; set; } = new();
    public string Usage { get; set; } = string.Empty;
    public string Source { get; set; } = "Core Rulebook";
}

public enum EquipmentCategory
{
    AdventuringGear,
    Alchemical,
    Consumable,
    Held,
    Worn,
    Other
}