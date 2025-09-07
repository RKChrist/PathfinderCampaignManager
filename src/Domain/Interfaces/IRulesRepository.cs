using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Rules;

namespace PathfinderCampaignManager.Domain.Interfaces;

public interface IRulesRepository
{
    // Core rule operations
    Task<Result<RuleRecord>> GetRuleByIdAsync(RuleId id);
    Task<Result<IEnumerable<RuleRecord>>> GetRulesAsync(RuleCategory? category = null, string? searchTerm = null, IEnumerable<string>? tags = null);
    Task<Result<IEnumerable<RuleRecord>>> GetRulesByCategoryAsync(RuleCategory category);
    
    // Table and formula operations
    Task<Result<RuleTable>> GetRuleTableAsync(RuleId ruleId);
    Task<Result<IEnumerable<RuleTable>>> GetRuleTablesAsync();
    Task<Result<RuleFormula>> GetRuleFormulaAsync(RuleId ruleId);
    Task<Result<IEnumerable<RuleFormula>>> GetRuleFormulasAsync();
    
    // Search and filtering
    Task<Result<IEnumerable<RuleRecord>>> SearchRulesAsync(string searchTerm);
    Task<Result<IEnumerable<RuleRecord>>> GetRulesByTagAsync(string tag);
    Task<Result<IEnumerable<string>>> GetAllTagsAsync();
    
    // Category and metadata operations
    Task<Result<Dictionary<RuleCategory, int>>> GetRuleCategoryCountsAsync();
    Task<Result<IEnumerable<string>>> GetTraitsAsync();
    Task<Result<bool>> ValidateRuleIntegrityAsync();
}

// Validation service for rules
public interface IRulesValidationService
{
    Task<Result<bool>> ValidateRuleRecord(RuleRecord rule);
    Task<Result<bool>> ValidateNoLevelReferences(RuleRecord rule);
    Task<Result<bool>> ValidateNoCampaignReferences(RuleRecord rule);
    Task<Result<bool>> ValidateSrdAlignment(RuleRecord rule);
    Task<Result<IEnumerable<string>>> GetValidationIssues(RuleRecord rule);
    Task<Result<bool>> CheckDataIntegrity();
}