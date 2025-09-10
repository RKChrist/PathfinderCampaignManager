namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfMonster : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Rarity { get; set; } = "Common";
    public List<string> Traits { get; set; } = new();
    public string Size { get; set; } = "Medium";
    public string Alignment { get; set; } = "N";
    
    // Abilities
    public Dictionary<string, int> AbilityScores { get; set; } = new()
    {
        ["Strength"] = 10,
        ["Dexterity"] = 10,
        ["Constitution"] = 10,
        ["Intelligence"] = 10,
        ["Wisdom"] = 10,
        ["Charisma"] = 10
    };
    
    // Defenses
    public int ArmorClass { get; set; }
    public List<string> ArmorClassNotes { get; set; } = new();
    public int HitPoints { get; set; }
    public string HitPointsFormula { get; set; } = string.Empty;
    public List<string> Immunities { get; set; } = new();
    public List<string> Resistances { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    
    // Saves
    public int FortitudeSave { get; set; }
    public int ReflexSave { get; set; }
    public int WillSave { get; set; }
    public List<string> SaveNotes { get; set; } = new();
    
    // Skills & Perception
    public int Perception { get; set; }
    public List<string> PerceptionNotes { get; set; } = new();
    public Dictionary<string, int> Skills { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<string> Senses { get; set; } = new();
    
    // Movement
    public Dictionary<string, int> Speeds { get; set; } = new() { ["land"] = 25 };
    public List<string> SpeedNotes { get; set; } = new();
    
    // Combat - Strikes/Attacks
    public List<PfStrike> Strikes { get; set; } = new();
    
    // Actions
    public List<PfMonsterAction> Actions { get; set; } = new();
    public List<PfMonsterAction> Reactions { get; set; } = new();
    public List<PfMonsterAction> PassiveAbilities { get; set; } = new();
    
    // Spells
    public List<PfMonsterSpellcasting> Spellcasting { get; set; } = new();
    
    // Items
    public List<string> Items { get; set; } = new();
    public string Treasure { get; set; } = string.Empty;
    
    // Metadata
    public string Source { get; set; } = "Bestiary";
    public string License { get; set; } = "OGL";
    
    // Derived Properties
    public Dictionary<string, int> AbilityModifiers => AbilityScores.ToDictionary(
        kvp => kvp.Key,
        kvp => (kvp.Value - 10) / 2
    );
    
    public string CreatureType => Traits.FirstOrDefault(t => IsCreatureTypeTrait(t)) ?? "Humanoid";
    
    public bool IsSpellcaster => Spellcasting.Any();
    
    private static bool IsCreatureTypeTrait(string trait)
    {
        var creatureTypes = new[]
        {
            "Aberration", "Animal", "Astral", "Beast", "Celestial", "Construct", "Dragon",
            "Elemental", "Fey", "Fiend", "Fungus", "Giant", "Humanoid", "Monitor", 
            "Ooze", "Plant", "Spirit", "Undead"
        };
        return creatureTypes.Contains(trait, StringComparer.OrdinalIgnoreCase);
    }
}

public class PfStrike
{
    public string Name { get; set; } = string.Empty;
    public int AttackBonus { get; set; }
    public List<string> Traits { get; set; } = new();
    public string DamageFormula { get; set; } = string.Empty;
    public string DamageType { get; set; } = string.Empty;
    public List<PfStrikeDamage> AdditionalDamage { get; set; } = new();
    public List<string> AdditionalEffects { get; set; } = new();
    public string Range { get; set; } = "melee"; // "melee", "ranged", or specific range
    public int? RangeIncrement { get; set; }
    public int? MaxRange { get; set; }
    public int? Reach { get; set; }
    public List<string> Effects { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

public class PfStrikeDamage
{
    public string Formula { get; set; } = string.Empty;
    public string DamageType { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public bool IsPersistent { get; set; }
}

public class PfMonsterAction
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ActionType ActionType { get; set; } = ActionType.Single;
    public string? ActionCost { get; set; }
    public List<string> Traits { get; set; } = new();
    public string? Frequency { get; set; }
    public string? Trigger { get; set; }
    public string? Requirements { get; set; }
    public string? Cost { get; set; }
    public string Range { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Targets { get; set; }
    public string? SavingThrow { get; set; }
    public bool IsAttack { get; set; }
    public List<string> Effects { get; set; } = new();
}

public class PfMonsterSpellcasting
{
    public string Name { get; set; } = string.Empty;
    public string Tradition { get; set; } = string.Empty;
    public int SpellAttack { get; set; }
    public int SpellDC { get; set; }
    public Dictionary<int, List<string>> SpellsKnown { get; set; } = new();
    public Dictionary<int, int> SpellSlots { get; set; } = new();
    public List<string> Cantrips { get; set; } = new();
    public List<string> ConstantSpells { get; set; } = new();
    public List<string> Rituals { get; set; } = new();
    public List<string> Notes { get; set; } = new();
}

public enum ActionType
{
    Free,
    Reaction,
    Single,
    Double,
    Triple,
    Action,
    Variable,
    Passive
}