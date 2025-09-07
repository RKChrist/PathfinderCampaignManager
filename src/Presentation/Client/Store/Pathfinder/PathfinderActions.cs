using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Presentation.Client.Store.Pathfinder;

public abstract class PathfinderAction { }

// Load All Data
public class LoadAllPathfinderDataAction : PathfinderAction { }

// Classes
public class LoadClassesAction : PathfinderAction { }
public class LoadClassesSuccessAction : PathfinderAction
{
    public List<PfClass> Classes { get; }
    
    public LoadClassesSuccessAction(List<PfClass> classes)
    {
        Classes = classes;
    }
}

// Ancestries
public class LoadAncestriesAction : PathfinderAction { }
public class LoadAncestriesSuccessAction : PathfinderAction
{
    public List<PfAncestry> Ancestries { get; }
    
    public LoadAncestriesSuccessAction(List<PfAncestry> ancestries)
    {
        Ancestries = ancestries;
    }
}

// Backgrounds
public class LoadBackgroundsAction : PathfinderAction { }
public class LoadBackgroundsSuccessAction : PathfinderAction
{
    public List<PfBackground> Backgrounds { get; }
    
    public LoadBackgroundsSuccessAction(List<PfBackground> backgrounds)
    {
        Backgrounds = backgrounds;
    }
}

// Skills
public class LoadSkillsAction : PathfinderAction { }
public class LoadSkillsSuccessAction : PathfinderAction
{
    public List<PfSkill> Skills { get; }
    
    public LoadSkillsSuccessAction(List<PfSkill> skills)
    {
        Skills = skills;
    }
}

// Feats
public class LoadFeatsAction : PathfinderAction { }
public class LoadFeatsSuccessAction : PathfinderAction
{
    public List<PfFeat> Feats { get; }
    
    public LoadFeatsSuccessAction(List<PfFeat> feats)
    {
        Feats = feats;
    }
}

// Spells
public class LoadSpellsAction : PathfinderAction { }
public class LoadSpellsSuccessAction : PathfinderAction
{
    public List<PfSpell> Spells { get; }
    
    public LoadSpellsSuccessAction(List<PfSpell> spells)
    {
        Spells = spells;
    }
}

// Weapons
public class LoadWeaponsAction : PathfinderAction { }
public class LoadWeaponsSuccessAction : PathfinderAction
{
    public List<PfWeapon> Weapons { get; }
    
    public LoadWeaponsSuccessAction(List<PfWeapon> weapons)
    {
        Weapons = weapons;
    }
}

// Monsters
public class LoadMonstersAction : PathfinderAction { }
public class LoadMonstersSuccessAction : PathfinderAction
{
    public List<PfMonster> Monsters { get; }
    
    public LoadMonstersSuccessAction(List<PfMonster> monsters)
    {
        Monsters = monsters;
    }
}

// Error Actions
public class LoadPathfinderDataFailureAction : PathfinderAction
{
    public string ErrorMessage { get; }
    
    public LoadPathfinderDataFailureAction(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}

// Loading Actions
public class SetPathfinderLoadingAction : PathfinderAction
{
    public bool IsLoading { get; }
    
    public SetPathfinderLoadingAction(bool isLoading)
    {
        IsLoading = isLoading;
    }
}