using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Validation;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Presentation.Client.Pages;

public partial class ValidationDemo : ComponentBase
{
    private ValidationReport ValidReport = new();
    private ValidationReport WarningReport = new();
    private ValidationReport InvalidReport = new();
    private ValidationReport SuggestionReport = new();

    protected override void OnInitialized()
    {
        CreateSampleReports();
    }

    private void CreateSampleReports()
    {
        // Valid character report
        ValidReport = new ValidationReport
        {
            ValidatedEntityId = "sample-character-1",
            ValidatedEntityType = "Character",
            IsValid = true
        };
        ValidReport.AddSuggestion("Optimization", "Consider selecting a Heritage feat", 
            "Heritage feats can provide useful passive abilities");

        // Warning report
        WarningReport = new ValidationReport
        {
            ValidatedEntityId = "sample-character-2", 
            ValidatedEntityType = "Character",
            IsValid = true
        };
        WarningReport.AddWarning("AbilityScores", "Constitution score is unusually low (8)", 
            "Consider increasing Constitution for better survivability");
        WarningReport.AddWarning("Equipment", "Character has no armor equipped", 
            "Equip armor to improve AC");
        WarningReport.AddSuggestion("Skills", "Consider training in Perception", 
            "Perception is crucial for initiative and spotting threats");

        // Invalid character report
        InvalidReport = new ValidationReport
        {
            ValidatedEntityId = "sample-character-3",
            ValidatedEntityType = "Character",
            IsValid = false
        };
        InvalidReport.AddIssue(ValidationSeverity.Error, "Character", "Character must have a name");
        InvalidReport.AddIssue(ValidationSeverity.Error, "AbilityScores", "Strength score cannot exceed 30");
        InvalidReport.AddIssue(ValidationSeverity.Warning, "Prerequisites", 
            "Power Attack feat requires Strength 13+, character has Strength 12",
            "Increase Strength to 13 or remove the feat");
        InvalidReport.AddWarning("Feats", "Character has no ancestry feats selected", 
            "Select ancestry feats to gain unique abilities");

        // Suggestion-heavy report
        SuggestionReport = new ValidationReport
        {
            ValidatedEntityId = "sample-character-4",
            ValidatedEntityType = "Character", 
            IsValid = true
        };
        SuggestionReport.AddSuggestion("Combat", "Consider improving AC through armor or abilities", 
            "Higher AC improves survivability in combat");
        SuggestionReport.AddSuggestion("Skills", "Consider specializing in fewer skills for higher bonuses", 
            "Expert and Master proficiency provide significant bonuses");
        SuggestionReport.AddSuggestion("Spellcasting", "Consider learning utility spells", 
            "Utility spells provide versatile problem-solving options");
    }

    private async Task OnValidationCompleted(ValidationReport report)
    {
        // This would handle validation completion in a real scenario
        Console.WriteLine($"Validation completed: {report.IsValid}");
    }
}