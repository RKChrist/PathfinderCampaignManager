namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfSpell : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> Traditions { get; set; } = new(); // Arcane, Divine, Occult, Primal
    public string School { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    
    // Casting
    public string ActionCost { get; set; } = string.Empty; // "1", "2", "3", "reaction", "1 minute", etc.
    public string Range { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Targets { get; set; }
    public string Duration { get; set; } = string.Empty;
    public bool IsSustained { get; set; }
    public bool IsDismissible { get; set; }
    
    // Components
    public List<string> Components { get; set; } = new(); // "somatic", "verbal", "material", "focus"
    public string? MaterialComponent { get; set; } // Specific material component if any
    public string? FocusComponent { get; set; } // Specific focus component if any
    
    // Defense and saves
    public string? SavingThrow { get; set; } // "Fortitude", "Reflex", "Will", "basic Fortitude", etc.
    public bool HasAttackRoll { get; set; }
    public string? AttackType { get; set; } // "spell", "ranged", "melee"
    
    // Effects and mechanics
    public List<PfSpellDamage> Damage { get; set; } = new();
    public List<PfSpellHealing> Healing { get; set; } = new();
    public int? TempHp { get; set; }
    public List<string> ConditionsApplied { get; set; } = new();
    public List<string> Afflictions { get; set; } = new();
    public string? CounterActLevel { get; set; }
    
    // Heightening effects
    public List<PfSpellHeightening> Heightening { get; set; } = new();
    
    // Requirements and restrictions
    public string? Requirements { get; set; }
    public string? Trigger { get; set; } // For reaction spells
    
    // Metadata
    public string Source { get; set; } = "Core Rulebook";
    public string Rarity { get; set; } = "Common";
    public bool IsCantrip { get; set; }
    public bool IsFocus { get; set; }
    public bool IsRitual { get; set; }
    public string? SpellList { get; set; } // For spells on specific spell lists
    
    // Additional categorization for browsing
    public List<string> Tags { get; set; } = new(); // "utility", "offense", "defense", "healing", etc.
}

public class PfSpellDamage
{
    public string DiceFormula { get; set; } = string.Empty; // "1d6", "2d4+2", etc.
    public string DamageType { get; set; } = string.Empty; // "fire", "cold", "piercing", etc.
    public string? Condition { get; set; } // When this damage applies
    public bool IsPersistent { get; set; }
}

public class PfSpellHealing
{
    public string DiceFormula { get; set; } = string.Empty; // "1d8", "2d8+4", etc.
    public string Type { get; set; } = "Healing"; // "Healing", "Fast Healing", etc.
    public string? Condition { get; set; } // When this healing applies
}

public class PfSpellHeightening
{
    public string Level { get; set; } = string.Empty; // "+1", "3rd", "5th", etc.
    public string Effect { get; set; } = string.Empty;
    public List<PfSpellDamage>? AdditionalDamage { get; set; }
    public List<PfSpellHealing>? AdditionalHealing { get; set; }
    public string? AdditionalTargets { get; set; }
    public string? AdditionalArea { get; set; }
    public string? AdditionalRange { get; set; }
    public Dictionary<string, object>? AdditionalValues { get; set; } // For any other spell parameter modifications
}