namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfClassProgression
{
    public string ClassName { get; set; } = string.Empty;
    public List<PfLevelProgression> Levels { get; set; } = new();
    public List<string> Subclasses { get; set; } = new(); // Schools, Domains, Instincts, etc.
    public PfSpellcastingProgression? SpellcastingProgression { get; set; }
    public List<string> FeatCategories { get; set; } = new(); // What feat types this class grants
}

public class PfLevelProgression
{
    public int Level { get; set; }
    public List<string> Features { get; set; } = new(); // Features gained at this level
    public List<string> FeatsGranted { get; set; } = new(); // What feat slots are granted
    public ProficiencyProgression Proficiencies { get; set; } = new();
    public int SkillIncreasesGranted { get; set; } = 0;
    public List<string> AbilityBoosts { get; set; } = new(); // At levels 5, 10, 15, 20
}

public class ProficiencyProgression
{
    public Dictionary<string, ProficiencyRank> WeaponProficiencies { get; set; } = new();
    public Dictionary<string, ProficiencyRank> ArmorProficiencies { get; set; } = new();
    public ProficiencyRank Perception { get; set; }
    public ProficiencyRank Fortitude { get; set; }
    public ProficiencyRank Reflex { get; set; }
    public ProficiencyRank Will { get; set; }
    public ProficiencyRank ClassDC { get; set; }
    public Dictionary<string, ProficiencyRank> SpellAttack { get; set; } = new();
    public Dictionary<string, ProficiencyRank> SpellDC { get; set; } = new();
}

public class PfSpellcastingProgression
{
    public string Tradition { get; set; } = string.Empty; // Arcane, Divine, Occult, Primal
    public string SpellcastingAbility { get; set; } = string.Empty; // INT, WIS, CHA
    public string Type { get; set; } = string.Empty; // Full, Half, Focus, etc.
    public Dictionary<int, Dictionary<int, int>> SpellSlotsPerDay { get; set; } = new(); // [Level][SpellLevel] = Slots
    public Dictionary<int, int> SpellsKnown { get; set; } = new(); // [SpellLevel] = Known
    public Dictionary<int, List<string>> BonusSpells { get; set; } = new(); // Domain spells, bloodline spells, etc.
    public Dictionary<int, int> FocusSpells { get; set; } = new(); // [Level] = Focus Points
    public int CantripsKnown { get; set; }
    public bool PreparedCaster { get; set; } = false;
}

// Subclass systems for each class
public class PfWizardSchool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SchoolSpells { get; set; } = new();
    public List<string> FocusSpells { get; set; } = new();
    public string CurriculumSpell { get; set; } = string.Empty;
}

public class PfClericDomain
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> DomainSpells { get; set; } = new();
    public string InitialDomainSpell { get; set; } = string.Empty;
    public string AdvancedDomainSpell { get; set; } = string.Empty;
}

public class PfBarbarianInstinct
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> InstinctAbilities { get; set; } = new();
    public List<string> Anathema { get; set; } = new();
    public Dictionary<string, string> RageAbilities { get; set; } = new();
}

public class PfRogueRacket
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RacketAbilities { get; set; } = new();
    public List<string> SkillProficiencies { get; set; } = new();
}

public class PfChampionCause
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Alignment { get; set; } = string.Empty;
    public string DeityRequirement { get; set; } = string.Empty;
    public string ChampionReaction { get; set; } = string.Empty;
    public List<string> DevotionSpells { get; set; } = new();
    public List<string> Anathema { get; set; } = new();
}

public class PfSorcererBloodline
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Tradition { get; set; } = string.Empty;
    public string BloodlineSkill { get; set; } = string.Empty;
    public List<string> BloodlineSpells { get; set; } = new();
    public List<string> BloodlineFocusSpells { get; set; } = new();
    public Dictionary<int, string> GrantedSpells { get; set; } = new(); // [Level] = Spell
}