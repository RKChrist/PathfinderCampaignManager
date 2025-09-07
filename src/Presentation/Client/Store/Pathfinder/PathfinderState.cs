using Fluxor;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Presentation.Client.Store.Pathfinder;

[FeatureState]
public class PathfinderState
{
    public bool IsLoading { get; }
    public string? ErrorMessage { get; }

    // PF2e Data
    public List<PfClass> Classes { get; }
    public List<PfAncestry> Ancestries { get; }
    public List<PfBackground> Backgrounds { get; }
    public List<PfSkill> Skills { get; }
    public List<PfFeat> Feats { get; }
    public List<PfSpell> Spells { get; }
    public List<PfWeapon> Weapons { get; }
    public List<PfMonster> Monsters { get; }

    // Data loaded flags
    public bool ClassesLoaded { get; }
    public bool AncestriesLoaded { get; }
    public bool BackgroundsLoaded { get; }
    public bool SkillsLoaded { get; }
    public bool FeatsLoaded { get; }
    public bool SpellsLoaded { get; }
    public bool WeaponsLoaded { get; }
    public bool MonstersLoaded { get; }

    private PathfinderState() { } // Required by Fluxor

    public PathfinderState(
        bool isLoading,
        string? errorMessage,
        List<PfClass> classes,
        List<PfAncestry> ancestries,
        List<PfBackground> backgrounds,
        List<PfSkill> skills,
        List<PfFeat> feats,
        List<PfSpell> spells,
        List<PfWeapon> weapons,
        List<PfMonster> monsters,
        bool classesLoaded,
        bool ancestriesLoaded,
        bool backgroundsLoaded,
        bool skillsLoaded,
        bool featsLoaded,
        bool spellsLoaded,
        bool weaponsLoaded,
        bool monstersLoaded)
    {
        IsLoading = isLoading;
        ErrorMessage = errorMessage;
        Classes = classes;
        Ancestries = ancestries;
        Backgrounds = backgrounds;
        Skills = skills;
        Feats = feats;
        Spells = spells;
        Weapons = weapons;
        Monsters = monsters;
        ClassesLoaded = classesLoaded;
        AncestriesLoaded = ancestriesLoaded;
        BackgroundsLoaded = backgroundsLoaded;
        SkillsLoaded = skillsLoaded;
        FeatsLoaded = featsLoaded;
        SpellsLoaded = spellsLoaded;
        WeaponsLoaded = weaponsLoaded;
        MonstersLoaded = monstersLoaded;
    }

    public static PathfinderState InitialState => new(
        isLoading: false,
        errorMessage: null,
        classes: new List<PfClass>(),
        ancestries: new List<PfAncestry>(),
        backgrounds: new List<PfBackground>(),
        skills: new List<PfSkill>(),
        feats: new List<PfFeat>(),
        spells: new List<PfSpell>(),
        weapons: new List<PfWeapon>(),
        monsters: new List<PfMonster>(),
        classesLoaded: false,
        ancestriesLoaded: false,
        backgroundsLoaded: false,
        skillsLoaded: false,
        featsLoaded: false,
        spellsLoaded: false,
        weaponsLoaded: false,
        monstersLoaded: false);

    public bool AllDataLoaded => ClassesLoaded && AncestriesLoaded && BackgroundsLoaded && 
                                SkillsLoaded && FeatsLoaded && SpellsLoaded && 
                                WeaponsLoaded && MonstersLoaded;
}