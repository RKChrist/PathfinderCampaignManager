using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Rules;
using System.Net.Http.Json;
using System.Text.Json;

namespace PathfinderCampaignManager.Presentation.Client.Services;

public class RulesApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public RulesApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<Result<IEnumerable<RuleCategory>>> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/rules/categories");
            if (response.IsSuccessStatusCode)
            {
                var categories = await response.Content.ReadFromJsonAsync<IEnumerable<RuleCategory>>(_jsonOptions);
                return categories != null ? Result<IEnumerable<RuleCategory>>.Success(categories) : Result.Failure<IEnumerable<RuleCategory>>(new Domain.Errors.DomainError("CATEGORIES.NOT_FOUND", "Categories not found"));
            }
            return Result.Failure<IEnumerable<RuleCategory>>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<RuleCategory>>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<RuleRecord>>> GetCategoryRulesAsync(string categoryName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/rules/categories/{categoryName}");
            if (response.IsSuccessStatusCode)
            {
                var rules = await response.Content.ReadFromJsonAsync<IEnumerable<RuleRecord>>(_jsonOptions);
                return rules != null ? Result.Success(rules) : Result.Failure<IEnumerable<RuleRecord>>(new Domain.Errors.DomainError("CATEGORY.NOT_FOUND", "Category rules not found"));
            }
            return Result.Failure<IEnumerable<RuleRecord>>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<RuleRecord>>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<RuleRecord>> GetRuleAsync(string categoryName, string ruleId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/rules/categories/{categoryName}/rules/{ruleId}");
            if (response.IsSuccessStatusCode)
            {
                var rule = await response.Content.ReadFromJsonAsync<RuleRecord>(_jsonOptions);
                return rule != null ? Result.Success(rule) : Result.Failure<RuleRecord>(new Domain.Errors.DomainError("RULE.NOT_FOUND", "Rule not found"));
            }
            return Result.Failure<RuleRecord>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<RuleRecord>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<RuleRecord>>> SearchRulesAsync(string searchTerm)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/rules/search?q={Uri.EscapeDataString(searchTerm)}");
            if (response.IsSuccessStatusCode)
            {
                var rules = await response.Content.ReadFromJsonAsync<IEnumerable<RuleRecord>>(_jsonOptions);
                return rules != null ? Result.Success(rules) : Result.Failure<IEnumerable<RuleRecord>>(new Domain.Errors.DomainError("RULES.NOT_FOUND", "No rules found"));
            }
            return Result.Failure<IEnumerable<RuleRecord>>(new Domain.Errors.DomainError("API.ERROR", $"API returned {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<RuleRecord>>(new Domain.Errors.DomainError("API.EXCEPTION", ex.Message));
        }
    }
}