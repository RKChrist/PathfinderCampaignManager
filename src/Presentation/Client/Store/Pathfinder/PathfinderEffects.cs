using Fluxor;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using System.Net.Http.Json;

namespace PathfinderCampaignManager.Presentation.Client.Store.Pathfinder;

public class PathfinderEffects
{
    private readonly HttpClient _httpClient;

    public PathfinderEffects(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [EffectMethod]
    public async Task HandleLoadAllPathfinderDataAction(LoadAllPathfinderDataAction action, IDispatcher dispatcher)
    {
        // Load all data concurrently
        var tasks = new[]
        {
            LoadDataAsync<List<PfClass>>("api/pathfinder/classes", classes => dispatcher.Dispatch(new LoadClassesSuccessAction(classes))),
            LoadDataAsync<List<PfAncestry>>("api/pathfinder/ancestries", ancestries => dispatcher.Dispatch(new LoadAncestriesSuccessAction(ancestries))),
            LoadDataAsync<List<PfBackground>>("api/pathfinder/backgrounds", backgrounds => dispatcher.Dispatch(new LoadBackgroundsSuccessAction(backgrounds))),
            LoadDataAsync<List<PfSkill>>("api/pathfinder/skills", skills => dispatcher.Dispatch(new LoadSkillsSuccessAction(skills))),
            LoadDataAsync<List<PfFeat>>("api/pathfinder/feats", feats => dispatcher.Dispatch(new LoadFeatsSuccessAction(feats))),
            LoadDataAsync<List<PfSpell>>("api/pathfinder/spells", spells => dispatcher.Dispatch(new LoadSpellsSuccessAction(spells))),
            LoadDataAsync<List<PfWeapon>>("api/pathfinder/weapons", weapons => dispatcher.Dispatch(new LoadWeaponsSuccessAction(weapons))),
            LoadDataAsync<List<PfMonster>>("api/pathfinder/monsters", monsters => dispatcher.Dispatch(new LoadMonstersSuccessAction(monsters)))
        };

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadPathfinderDataFailureAction($"Failed to load Pathfinder data: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleLoadClassesAction(LoadClassesAction action, IDispatcher dispatcher)
    {
        await LoadDataAsync<List<PfClass>>("api/pathfinder/classes", 
            classes => dispatcher.Dispatch(new LoadClassesSuccessAction(classes)),
            error => dispatcher.Dispatch(new LoadPathfinderDataFailureAction(error)));
    }

    [EffectMethod]
    public async Task HandleLoadAncestriesAction(LoadAncestriesAction action, IDispatcher dispatcher)
    {
        await LoadDataAsync<List<PfAncestry>>("api/pathfinder/ancestries", 
            ancestries => dispatcher.Dispatch(new LoadAncestriesSuccessAction(ancestries)),
            error => dispatcher.Dispatch(new LoadPathfinderDataFailureAction(error)));
    }

    [EffectMethod]
    public async Task HandleLoadBackgroundsAction(LoadBackgroundsAction action, IDispatcher dispatcher)
    {
        await LoadDataAsync<List<PfBackground>>("api/pathfinder/backgrounds", 
            backgrounds => dispatcher.Dispatch(new LoadBackgroundsSuccessAction(backgrounds)),
            error => dispatcher.Dispatch(new LoadPathfinderDataFailureAction(error)));
    }

    [EffectMethod]
    public async Task HandleLoadSkillsAction(LoadSkillsAction action, IDispatcher dispatcher)
    {
        await LoadDataAsync<List<PfSkill>>("api/pathfinder/skills", 
            skills => dispatcher.Dispatch(new LoadSkillsSuccessAction(skills)),
            error => dispatcher.Dispatch(new LoadPathfinderDataFailureAction(error)));
    }

    [EffectMethod]
    public async Task HandleLoadFeatsAction(LoadFeatsAction action, IDispatcher dispatcher)
    {
        await LoadDataAsync<List<PfFeat>>("api/pathfinder/feats", 
            feats => dispatcher.Dispatch(new LoadFeatsSuccessAction(feats)),
            error => dispatcher.Dispatch(new LoadPathfinderDataFailureAction(error)));
    }

    [EffectMethod]
    public async Task HandleLoadSpellsAction(LoadSpellsAction action, IDispatcher dispatcher)
    {
        await LoadDataAsync<List<PfSpell>>("api/pathfinder/spells", 
            spells => dispatcher.Dispatch(new LoadSpellsSuccessAction(spells)),
            error => dispatcher.Dispatch(new LoadPathfinderDataFailureAction(error)));
    }

    [EffectMethod]
    public async Task HandleLoadWeaponsAction(LoadWeaponsAction action, IDispatcher dispatcher)
    {
        await LoadDataAsync<List<PfWeapon>>("api/pathfinder/weapons", 
            weapons => dispatcher.Dispatch(new LoadWeaponsSuccessAction(weapons)),
            error => dispatcher.Dispatch(new LoadPathfinderDataFailureAction(error)));
    }

    [EffectMethod]
    public async Task HandleLoadMonstersAction(LoadMonstersAction action, IDispatcher dispatcher)
    {
        await LoadDataAsync<List<PfMonster>>("api/pathfinder/monsters", 
            monsters => dispatcher.Dispatch(new LoadMonstersSuccessAction(monsters)),
            error => dispatcher.Dispatch(new LoadPathfinderDataFailureAction(error)));
    }

    private async Task LoadDataAsync<T>(string endpoint, Action<T> onSuccess, Action<string>? onError = null)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<T>(endpoint);
            if (response != null)
            {
                onSuccess(response);
            }
            else
            {
                onError?.Invoke($"No data received from {endpoint}");
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Failed to load data from {endpoint}: {ex.Message}");
        }
    }
}