using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Domain.Validation;

public interface IValidationService
{
    Task<Result<ValidationReport>> ValidateCharacterAsync(PfCharacter character);
    Task<Result<ValidationReport>> ValidateCalculatedCharacterAsync(ICalculatedCharacter character);
    Task<Result<ValidationReport>> ValidateCampaignAsync(string campaignId);
    Task<Result<ValidationReport>> ValidateArchetypeProgressionAsync(string characterId, string archetypeId);
    Task<Result<ValidationReport>> ValidatePrerequisitesAsync(IEnumerable<PfPrerequisite> prerequisites, ICalculatedCharacter character);
    Task<Result<ValidationReport>> ValidateSpellcastingAsync(ICalculatedCharacter character);
    Task<Result<ValidationReport>> ValidateFeatSelectionAsync(ICalculatedCharacter character, string featId);
    Task<Result<ValidationReport>> ValidateAbilityScoreArrayAsync(Dictionary<string, int> abilityScores, int characterLevel);
    Task<Result<ValidationReport>> ValidateEquipmentLoadAsync(ICalculatedCharacter character);
}

public class ValidationReport
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public List<ValidationSuggestion> Suggestions { get; set; } = new();
    public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;
    public string ValidatedEntityId { get; set; } = string.Empty;
    public string ValidatedEntityType { get; set; } = string.Empty;
    
    public void AddIssue(ValidationSeverity severity, string category, string message, string? fixSuggestion = null)
    {
        Issues.Add(new ValidationIssue
        {
            Severity = severity,
            Category = category,
            Message = message,
            FixAction = fixSuggestion
        });
    }
    
    public void AddWarning(string category, string message, string? recommendation = null)
    {
        Warnings.Add(new ValidationWarning
        {
            Category = category,
            Message = message,
            Recommendation = recommendation
        });
    }
    
    public void AddSuggestion(string category, string suggestion, string? benefit = null)
    {
        Suggestions.Add(new ValidationSuggestion
        {
            Category = category,
            Suggestion = suggestion,
            Benefit = benefit
        });
    }

    public bool HasCriticalIssues => Issues.Any(i => i.Severity == ValidationSeverity.Error);
    public bool HasWarnings => Warnings.Any() || Issues.Any(i => i.Severity == ValidationSeverity.Warning);
    public int TotalIssueCount => Issues.Count + Warnings.Count;
}

public class ValidationWarning
{
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class ValidationSuggestion
{
    public string Category { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string? Benefit { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}