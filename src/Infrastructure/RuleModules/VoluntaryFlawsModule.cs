using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Infrastructure.RuleModules;

public class VoluntaryFlawsModule : IRuleModule
{
    public string Name => "Voluntary Flaws";
    public int Priority => 10; // Apply early since it affects ability scores
    public VariantRuleType RuleType => VariantRuleType.VoluntaryFlaws;

    public void OnScores(Character character, CalculatedCharacter calculated)
    {
        // Apply voluntary flaws to ability scores
        // In a real implementation, character would have a VoluntaryFlaws property
        // For now, we'll simulate this logic
        
        // Voluntary flaws are typically applied during character creation
        // Each pair of voluntary flaws grants one additional ability boost
        ApplyVoluntaryFlaws(character, calculated);
    }

    public void OnProficiency(Character character, CalculatedCharacter calculated)
    {
        // Voluntary flaws don't affect proficiency directly
    }

    public void OnFeats(Character character, CalculatedCharacter calculated)
    {
        // Voluntary flaws don't grant additional feats
    }

    public void OnSlots(Character character, CalculatedCharacter calculated)
    {
        // Voluntary flaws don't affect feat slots
    }

    public void OnEncumbrance(Character character, CalculatedCharacter calculated)
    {
        // Strength flaws might affect carrying capacity
        // This is already handled by the base strength modifier calculation
    }

    public void OnValidation(Character character, CalculatedCharacter calculated)
    {
        ValidateVoluntaryFlaws(character, calculated);
    }

    private void ApplyVoluntaryFlaws(Character character, CalculatedCharacter calculated)
    {
        // In a real implementation, you'd read voluntary flaws from character data
        // For demonstration, we'll check if character has any stored voluntary flaw data
        
        // Simulate reading from character's build data
        var voluntaryFlaws = GetVoluntaryFlaws(character);
        
        if (voluntaryFlaws.Any())
        {
            foreach (var flaw in voluntaryFlaws)
            {
                // Apply -2 to the flawed ability
                if (calculated.AbilityScores.ContainsKey(flaw.Ability))
                {
                    calculated.AbilityScores[flaw.Ability] -= 2;
                }
            }
            
            // Recalculate modifiers after flaws
            foreach (var ability in calculated.AbilityScores.Keys.ToList())
            {
                calculated.AbilityModifiers[ability] = (calculated.AbilityScores[ability] - 10) / 2;
            }
        }
    }

    private void ValidateVoluntaryFlaws(Character character, CalculatedCharacter calculated)
    {
        var voluntaryFlaws = GetVoluntaryFlaws(character);
        
        if (voluntaryFlaws.Any())
        {
            // Validate that voluntary flaws follow the rules:
            // 1. You can't apply voluntary flaws to abilities that already have ancestry flaws
            // 2. Each pair of voluntary flaws grants one additional boost
            // 3. No ability can go below 8 after voluntary flaws
            
            var flawCounts = voluntaryFlaws.GroupBy(f => f.Ability).ToDictionary(g => g.Key, g => g.Count());
            
            foreach (var flawCount in flawCounts)
            {
                var ability = flawCount.Key;
                var count = flawCount.Value;
                
                // Check minimum ability score
                if (calculated.AbilityScores.GetValueOrDefault(ability, 10) < 8)
                {
                    calculated.ValidationIssues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Category = "Voluntary Flaws",
                        Message = $"{ability} cannot be reduced below 8",
                        FixAction = $"RemoveVoluntaryFlaw:{ability}"
                    });
                }
                
                // Validate pairing (every 2 flaws should grant 1 boost)
                if (count % 2 != 0)
                {
                    calculated.ValidationIssues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Category = "Voluntary Flaws",
                        Message = $"Unpaired voluntary flaw in {ability}. Each pair of voluntary flaws grants one additional ability boost.",
                        FixAction = $"AddVoluntaryFlaw:{ability}"
                    });
                }
            }
            
            // Add info about additional boosts gained
            var totalFlaws = voluntaryFlaws.Count;
            var additionalBoosts = totalFlaws / 2;
            
            calculated.ValidationIssues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Info,
                Category = "Voluntary Flaws",
                Message = $"Voluntary flaws grant {additionalBoosts} additional ability boost(s)",
                Data = new Dictionary<string, object>
                {
                    ["TotalFlaws"] = totalFlaws,
                    ["AdditionalBoosts"] = additionalBoosts,
                    ["Flaws"] = voluntaryFlaws.Select(f => f.Ability).ToList()
                }
            });
        }
    }

    private List<VoluntaryFlaw> GetVoluntaryFlaws(Character character)
    {
        // In a real implementation, this would read from character's stored data
        // For now, return empty list as demonstration
        return new List<VoluntaryFlaw>();
    }
}

public class VoluntaryFlaw
{
    public string Ability { get; set; } = string.Empty;
    public string Source { get; set; } = "Voluntary";
}