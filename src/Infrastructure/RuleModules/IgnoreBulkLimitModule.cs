using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Infrastructure.RuleModules;

public class IgnoreBulkLimitModule : IRuleModule
{
    public string Name => "Ignore Bulk Limit";
    public int Priority => 50;
    public VariantRuleType RuleType => VariantRuleType.IgnoreBulkLimit;

    public void OnScores(Character character, CalculatedCharacter calculated)
    {
        // Ignore Bulk Limit doesn't affect ability scores
    }

    public void OnProficiency(Character character, CalculatedCharacter calculated)
    {
        // Ignore Bulk Limit doesn't affect proficiency
    }

    public void OnFeats(Character character, CalculatedCharacter calculated)
    {
        // Ignore Bulk Limit doesn't affect feats
    }

    public void OnSlots(Character character, CalculatedCharacter calculated)
    {
        // Ignore Bulk Limit doesn't affect feat slots
    }

    public void OnEncumbrance(Character character, CalculatedCharacter calculated)
    {
        // Override encumbrance calculation - always set to not encumbered
        // But keep the bulk values for reference
        calculated.IsEncumbered = false;
        
        // Still calculate current bulk for display purposes
        // In a real implementation, you'd sum up all carried items
        // For now, we'll keep whatever was already calculated
    }

    public void OnValidation(Character character, CalculatedCharacter calculated)
    {
        calculated.ValidationIssues.Add(new ValidationIssue
        {
            Severity = ValidationSeverity.Info,
            Category = "Ignore Bulk Limit",
            Message = "Bulk limits and encumbrance penalties are ignored. Item bulk is still tracked for reference.",
            Data = new Dictionary<string, object>
            {
                ["CurrentBulk"] = calculated.CurrentBulk,
                ["TheoreticalLimit"] = calculated.BulkLimit,
                ["WouldBeEncumbered"] = calculated.CurrentBulk > calculated.BulkLimit
            }
        });
    }
}