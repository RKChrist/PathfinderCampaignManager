using System.Net.Http.Json;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Validation;

namespace PathfinderCampaignManager.Presentation.Client.Services;

public class ValidationClientService
{
    private readonly HttpClient _httpClient;

    public ValidationClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ValidationReport?> ValidateCharacterAsync(Guid characterId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/validation/character/{characterId}", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("Character", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("Character", $"Validation error: {ex.Message}");
        }
    }

    public async Task<ValidationReport?> ValidateCalculatedCharacterAsync(ICalculatedCharacter character)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/validation/calculated-character", character);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("CalculatedCharacter", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("CalculatedCharacter", $"Validation error: {ex.Message}");
        }
    }

    public async Task<ValidationReport?> ValidateCampaignAsync(string campaignId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/validation/campaign/{campaignId}", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("Campaign", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("Campaign", $"Validation error: {ex.Message}");
        }
    }

    public async Task<ValidationReport?> ValidateArchetypeProgressionAsync(string characterId, string archetypeId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/validation/archetype/{characterId}/{archetypeId}", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("ArchetypeProgression", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("ArchetypeProgression", $"Validation error: {ex.Message}");
        }
    }

    public async Task<ValidationReport?> ValidatePrerequisitesAsync(IEnumerable<PfPrerequisite> prerequisites, ICalculatedCharacter character)
    {
        try
        {
            var request = new PrerequisiteValidationRequest
            {
                Prerequisites = prerequisites,
                Character = character
            };

            var response = await _httpClient.PostAsJsonAsync("api/validation/prerequisites", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("Prerequisites", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("Prerequisites", $"Validation error: {ex.Message}");
        }
    }

    public async Task<ValidationReport?> ValidateSpellcastingAsync(ICalculatedCharacter character)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/validation/spellcasting", character);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("Spellcasting", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("Spellcasting", $"Validation error: {ex.Message}");
        }
    }

    public async Task<ValidationReport?> ValidateFeatSelectionAsync(string featId, ICalculatedCharacter character)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/validation/feat/{featId}", character);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("FeatSelection", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("FeatSelection", $"Validation error: {ex.Message}");
        }
    }

    public async Task<ValidationReport?> ValidateAbilityScoresAsync(Dictionary<string, int> abilityScores, int characterLevel)
    {
        try
        {
            var request = new AbilityScoreValidationRequest
            {
                AbilityScores = abilityScores,
                CharacterLevel = characterLevel
            };

            var response = await _httpClient.PostAsJsonAsync("api/validation/ability-scores", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("AbilityScores", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("AbilityScores", $"Validation error: {ex.Message}");
        }
    }

    public async Task<ValidationReport?> ValidateEquipmentLoadAsync(ICalculatedCharacter character)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/validation/equipment", character);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationReport>();
            }
            
            return CreateErrorReport("Equipment", $"Validation failed: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return CreateErrorReport("Equipment", $"Validation error: {ex.Message}");
        }
    }

    public async Task<bool> IsValidationServiceHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/validation/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private ValidationReport CreateErrorReport(string entityType, string errorMessage)
    {
        var report = new ValidationReport
        {
            ValidatedEntityType = entityType,
            IsValid = false
        };
        
        report.AddIssue(ValidationSeverity.Error, "System", errorMessage);
        return report;
    }
}

public class PrerequisiteValidationRequest
{
    public IEnumerable<PfPrerequisite> Prerequisites { get; set; } = new List<PfPrerequisite>();
    public ICalculatedCharacter Character { get; set; } = null!;
}

public class AbilityScoreValidationRequest
{
    public Dictionary<string, int> AbilityScores { get; set; } = new Dictionary<string, int>();
    public int CharacterLevel { get; set; } = 1;
}