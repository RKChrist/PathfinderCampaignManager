using Fluxor;

namespace PathfinderCampaignManager.Presentation.Client.Store.Pathfinder;

public static class PathfinderReducers
{
    [ReducerMethod]
    public static PathfinderState ReduceLoadAllPathfinderDataAction(PathfinderState state, LoadAllPathfinderDataAction action) =>
        new(isLoading: true, state.ErrorMessage, state.Classes, state.Ancestries, state.Backgrounds, state.Skills, 
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadClassesAction(PathfinderState state, LoadClassesAction action) =>
        new(isLoading: true, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadClassesSuccessAction(PathfinderState state, LoadClassesSuccessAction action) =>
        new(isLoading: false, errorMessage: null, action.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, classesLoaded: true, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadAncestriesAction(PathfinderState state, LoadAncestriesAction action) =>
        new(isLoading: true, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadAncestriesSuccessAction(PathfinderState state, LoadAncestriesSuccessAction action) =>
        new(isLoading: false, errorMessage: null, state.Classes, action.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, ancestriesLoaded: true,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadBackgroundsAction(PathfinderState state, LoadBackgroundsAction action) =>
        new(isLoading: true, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadBackgroundsSuccessAction(PathfinderState state, LoadBackgroundsSuccessAction action) =>
        new(isLoading: false, errorMessage: null, state.Classes, state.Ancestries, action.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            backgroundsLoaded: true, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadSkillsAction(PathfinderState state, LoadSkillsAction action) =>
        new(isLoading: true, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadSkillsSuccessAction(PathfinderState state, LoadSkillsSuccessAction action) =>
        new(isLoading: false, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, action.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, skillsLoaded: true, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadFeatsAction(PathfinderState state, LoadFeatsAction action) =>
        new(isLoading: true, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadFeatsSuccessAction(PathfinderState state, LoadFeatsSuccessAction action) =>
        new(isLoading: false, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            action.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, featsLoaded: true, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadSpellsAction(PathfinderState state, LoadSpellsAction action) =>
        new(isLoading: true, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadSpellsSuccessAction(PathfinderState state, LoadSpellsSuccessAction action) =>
        new(isLoading: false, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, action.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, spellsLoaded: true, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadWeaponsAction(PathfinderState state, LoadWeaponsAction action) =>
        new(isLoading: true, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadWeaponsSuccessAction(PathfinderState state, LoadWeaponsSuccessAction action) =>
        new(isLoading: false, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, action.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, weaponsLoaded: true, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadMonstersAction(PathfinderState state, LoadMonstersAction action) =>
        new(isLoading: true, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceLoadMonstersSuccessAction(PathfinderState state, LoadMonstersSuccessAction action) =>
        new(isLoading: false, errorMessage: null, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, action.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, monstersLoaded: true);

    [ReducerMethod]
    public static PathfinderState ReduceLoadPathfinderDataFailureAction(PathfinderState state, LoadPathfinderDataFailureAction action) =>
        new(isLoading: false, action.ErrorMessage, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);

    [ReducerMethod]
    public static PathfinderState ReduceSetPathfinderLoadingAction(PathfinderState state, SetPathfinderLoadingAction action) =>
        new(action.IsLoading, state.ErrorMessage, state.Classes, state.Ancestries, state.Backgrounds, state.Skills,
            state.Feats, state.Spells, state.Weapons, state.Monsters, state.ClassesLoaded, state.AncestriesLoaded,
            state.BackgroundsLoaded, state.SkillsLoaded, state.FeatsLoaded, state.SpellsLoaded, state.WeaponsLoaded, state.MonstersLoaded);
}