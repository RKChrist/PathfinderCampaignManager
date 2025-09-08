using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using System.Net.Http.Json;
using System.Text.Json;

namespace PathfinderCampaignManager.Presentation.Client.Services;

public class PathfinderApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public PathfinderApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<Result<PfSpell>> GetSpellAsync(string spellId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/pathfinder/spells/{spellId}");
            if (response.IsSuccessStatusCode)
            {
                var spell = await response.Content.ReadFromJsonAsync<PfSpell>(_jsonOptions);
                return spell != null ? Result<PfSpell>.Success(spell) : Result.Failure<PfSpell>(new Domain.Errors.DomainError("SPELL.NOT_FOUND", "Spell not found"));
            }
            return Result.Failure<PfSpell>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<PfSpell>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<PfFeat>> GetFeatAsync(string featId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/pathfinder/feats/{featId}");
            if (response.IsSuccessStatusCode)
            {
                var feat = await response.Content.ReadFromJsonAsync<PfFeat>(_jsonOptions);
                return feat != null ? Result<PfFeat>.Success(feat) : Result.Failure<PfFeat>(new Domain.Errors.DomainError("FEAT.NOT_FOUND", "Feat not found"));
            }
            return Result.Failure<PfFeat>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<PfFeat>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<PfSpell>>> GetSpellsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/pathfinder/spells");
            if (response.IsSuccessStatusCode)
            {
                var spells = await response.Content.ReadFromJsonAsync<IEnumerable<PfSpell>>(_jsonOptions);
                return spells != null ? Result<IEnumerable<PfSpell>>.Success(spells) : Result.Failure<IEnumerable<PfSpell>>(new Domain.Errors.DomainError("SPELLS.NOT_FOUND", "Spells not found"));
            }
            return Result.Failure<IEnumerable<PfSpell>>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PfSpell>>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<PfFeat>>> GetFeatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/pathfinder/feats");
            if (response.IsSuccessStatusCode)
            {
                var feats = await response.Content.ReadFromJsonAsync<IEnumerable<PfFeat>>(_jsonOptions);
                return feats != null ? Result<IEnumerable<PfFeat>>.Success(feats) : Result.Failure<IEnumerable<PfFeat>>(new Domain.Errors.DomainError("FEATS.NOT_FOUND", "Feats not found"));
            }
            return Result.Failure<IEnumerable<PfFeat>>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PfFeat>>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<PfAncestry>>> GetAncestriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/pathfinder/ancestries");
            if (response.IsSuccessStatusCode)
            {
                var ancestries = await response.Content.ReadFromJsonAsync<IEnumerable<PfAncestry>>(_jsonOptions);
                return ancestries != null ? Result<IEnumerable<PfAncestry>>.Success(ancestries) : Result.Failure<IEnumerable<PfAncestry>>(new Domain.Errors.DomainError("ANCESTRIES.NOT_FOUND", "Ancestries not found"));
            }
            return Result.Failure<IEnumerable<PfAncestry>>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PfAncestry>>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<PfBackground>>> GetBackgroundsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/pathfinder/backgrounds");
            if (response.IsSuccessStatusCode)
            {
                var backgrounds = await response.Content.ReadFromJsonAsync<IEnumerable<PfBackground>>(_jsonOptions);
                return backgrounds != null ? Result<IEnumerable<PfBackground>>.Success(backgrounds) : Result.Failure<IEnumerable<PfBackground>>(new Domain.Errors.DomainError("BACKGROUNDS.NOT_FOUND", "Backgrounds not found"));
            }
            return Result.Failure<IEnumerable<PfBackground>>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PfBackground>>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<PfClass>>> GetClassesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/pathfinder/classes");
            if (response.IsSuccessStatusCode)
            {
                var classes = await response.Content.ReadFromJsonAsync<IEnumerable<PfClass>>(_jsonOptions);
                return classes != null ? Result<IEnumerable<PfClass>>.Success(classes) : Result.Failure<IEnumerable<PfClass>>(new Domain.Errors.DomainError("CLASSES.NOT_FOUND", "Classes not found"));
            }
            return Result.Failure<IEnumerable<PfClass>>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PfClass>>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }
}