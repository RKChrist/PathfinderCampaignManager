namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfArchetype : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ArchetypeType Type { get; set; } = ArchetypeType.Multiclass;
    public List<PfPrerequisite> Prerequisites { get; set; } = new();
    
    // Dedication feat (required first feat)
    public string DedicationFeatId { get; set; } = string.Empty;
    
    // All archetype feats (including dedication)
    public List<string> ArchetypeFeatIds { get; set; } = new();
    
    // For multiclass archetypes
    public string? AssociatedClassId { get; set; }
    public PfMulticlassSpellcasting? SpellcastingProgression { get; set; }
    
    // General archetype properties
    public List<string> Traits { get; set; } = new();
    public string Source { get; set; } = "Core Rulebook";
    public string Rarity { get; set; } = "Common";
    
    // Special rules
    public bool RequiresTwoFeatsBeforeNewArchetype { get; set; } = true;
    public List<string> SpecialRules { get; set; } = new();
}

public enum ArchetypeType
{
    Multiclass,
    Class,
    General
}

public class PfMulticlassSpellcasting
{
    public string Tradition { get; set; } = string.Empty;
    public string SpellcastingAbility { get; set; } = string.Empty;
    public Dictionary<int, int> SpellSlotsFromMulticlass { get; set; } = new(); // [Spell Level] = Slots
    public Dictionary<int, int> SpellsKnownFromMulticlass { get; set; } = new(); // [Spell Level] = Known
    public int MaxSpellLevel { get; set; } = 0;
    public bool PreparedCasting { get; set; } = false;
}

// Dedication feat prerequisite system
public class DedicationPrerequisite
{
    public string Type { get; set; } = string.Empty; // "AbilityScore", "Skill", "Feat", "Level"
    public string Name { get; set; } = string.Empty;
    public int? MinimumValue { get; set; }
    public string? Rank { get; set; } // For skill proficiencies
}

// Advanced archetype mechanics
public class ArchetypeProgression
{
    public string ArchetypeName { get; set; } = string.Empty;
    public Dictionary<int, List<string>> FeatsByLevel { get; set; } = new(); // [Level] = Available Feats
    public Dictionary<string, List<string>> FeatPrerequisites { get; set; } = new(); // [FeatName] = Prerequisites
    public PfMulticlassSpellcasting? Spellcasting { get; set; }
}