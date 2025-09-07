namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfAffliction
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AfflictionType Type { get; set; }
    public int Level { get; set; }
    public List<string> Traits { get; set; } = new();
    public string SavingThrow { get; set; } = string.Empty; // "Fortitude DC 20"
    public string Onset { get; set; } = string.Empty;
    public string MaxDuration { get; set; } = string.Empty;
    public List<PfAfflictionStage> Stages { get; set; } = new();
    public string Source { get; set; } = "Core Rulebook";
    public string Rarity { get; set; } = "Common";
}

public class PfAfflictionStage
{
    public int Stage { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public List<string> Conditions { get; set; } = new();
    public string DamageFormula { get; set; } = string.Empty;
    public string DamageType { get; set; } = string.Empty;
    public bool IsRecovery { get; set; } = false;
}

public enum AfflictionType
{
    Disease,
    Poison,
    Curse,
    Drug,
    Fear,
    Emotion,
    Mental,
    Other
}

public class PfCurse : PfAffliction
{
    public PfCurse()
    {
        Type = AfflictionType.Curse;
    }
    
    public string CurseType { get; set; } = string.Empty; // "Misfortune", "Transformation", etc.
    public List<string> RemovalMethods { get; set; } = new();
    public bool IsPermanent { get; set; } = false;
    public string TriggerCondition { get; set; } = string.Empty;
}

public class PfPoison : PfAffliction
{
    public PfPoison()
    {
        Type = AfflictionType.Poison;
    }
    
    public string DeliveryMethod { get; set; } = string.Empty; // "Contact", "Ingested", "Inhaled", "Injury"
    public bool IsVirulent { get; set; } = false;
    public string PoisonType { get; set; } = string.Empty;
}

public class PfDisease : PfAffliction
{
    public PfDisease()
    {
        Type = AfflictionType.Disease;
    }
    
    public bool IsContagious { get; set; } = false;
    public string TransmissionMethod { get; set; } = string.Empty;
    public string IncubationPeriod { get; set; } = string.Empty;
}

public class PfCondition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ConditionType Type { get; set; }
    public List<string> Traits { get; set; } = new();
    public bool HasValue { get; set; } = false; // For conditions like Clumsy 1, Enfeebled 2, etc.
    public int? MaxValue { get; set; }
    public bool Overrides { get; set; } = false; // Does this condition override others of same type?
    public List<string> OverriddenBy { get; set; } = new();
    public List<string> ImmunityTraits { get; set; } = new(); // Traits that grant immunity
    public string Source { get; set; } = "Core Rulebook";
}

public enum ConditionType
{
    Debuff,
    Buff,
    Status,
    Circumstance,
    Persistent,
    Death,
    Senses
}