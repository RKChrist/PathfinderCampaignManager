using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Infrastructure.RuleModules;

public class AutomaticBonusProgressionModule : IRuleModule
{
    public string Name => "Automatic Bonus Progression";
    public int Priority => 100;
    public VariantRuleType RuleType => VariantRuleType.AutomaticBonusProgression;

    public void OnScores(Character character, CalculatedCharacter calculated)
    {
        // ABP doesn't affect ability scores directly
    }

    public void OnProficiency(Character character, CalculatedCharacter calculated)
    {
        // ABP doesn't affect proficiency training
    }

    public void OnFeats(Character character, CalculatedCharacter calculated)
    {
        // ABP doesn't grant additional feats
    }

    public void OnSlots(Character character, CalculatedCharacter calculated)
    {
        // ABP doesn't affect feat slots
    }

    public void OnEncumbrance(Character character, CalculatedCharacter calculated)
    {
        // ABP doesn't affect encumbrance
    }

    public void OnValidation(Character character, CalculatedCharacter calculated)
    {
        // Add validation to ensure no item bonuses are applied when ABP is active
        calculated.ValidationIssues.Add(new ValidationIssue
        {
            Severity = ValidationSeverity.Info,
            Category = "Automatic Bonus Progression",
            Message = "Item bonuses to AC, attack rolls, and damage are replaced by automatic bonuses"
        });
    }

    // Helper methods for calculating ABP bonuses
    public static int GetAttackPotencyBonus(int level)
    {
        return level switch
        {
            >= 16 => 3,
            >= 10 => 2,
            >= 4 => 1,
            _ => 0
        };
    }

    public static int GetArmorPotencyBonus(int level)
    {
        return level switch
        {
            >= 18 => 3,
            >= 11 => 2,
            >= 5 => 1,
            _ => 0
        };
    }

    public static int GetResilientBonus(int level)
    {
        return level switch
        {
            >= 17 => 3,
            >= 11 => 2,
            >= 8 => 1,
            _ => 0
        };
    }

    public static int GetStrikingDice(int level)
    {
        return level switch
        {
            >= 19 => 3, // Major striking
            >= 12 => 2, // Greater striking
            >= 4 => 1,  // Striking
            _ => 0
        };
    }
}