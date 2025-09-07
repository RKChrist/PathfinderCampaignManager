using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Services;

/// <summary>
/// Service for calculating and applying modifiers to character attributes
/// </summary>
public class ModifierEngine : IModifierEngine
{
    /// <summary>
    /// Calculate the final values for all character attributes with modifiers applied
    /// </summary>
    public CalculatedCharacterStats CalculateCharacterStats(Character character, List<CustomDefinitionModifier> modifiers)
    {
        var stats = new CalculatedCharacterStats
        {
            CharacterId = character.Id,
            BaseStats = GetBaseStats(character)
        };

        // Group modifiers by target for stacking calculation
        var modifierGroups = modifiers
            .Where(m => m.IsActive)
            .GroupBy(m => m.Target)
            .ToList();

        foreach (var group in modifierGroups)
        {
            var target = group.Key;
            var targetModifiers = group.OrderBy(m => m.Priority).ToList();
            
            var finalModifier = CalculateStackedModifier(targetModifiers);
            stats.Modifiers[target] = finalModifier;
            
            // Track modifier sources
            stats.ModifierSources[target] = targetModifiers
                .Select(m => new ModifierSource
                {
                    SourceId = m.CustomDefinitionId,
                    SourceName = m.CustomDefinition?.Name ?? "Unknown",
                    Value = m.Value,
                    Type = m.ModifierType,
                    Condition = m.Condition
                })
                .ToList();
        }

        // Calculate final values
        CalculateFinalValues(stats);
        
        // Validate stacking rules and add warnings
        ValidateModifierStacking(stats);

        return stats;
    }

    /// <summary>
    /// Calculate the stacked modifier value for a group of modifiers on the same target
    /// </summary>
    private ModifierResult CalculateStackedModifier(List<CustomDefinitionModifier> modifiers)
    {
        var result = new ModifierResult
        {
            TotalValue = 0,
            StackingWarnings = new List<string>()
        };

        // Group by modifier type for stacking rules
        var typeGroups = modifiers.GroupBy(m => m.ModifierType);
        
        foreach (var typeGroup in typeGroups)
        {
            var modifierType = typeGroup.Key;
            var typeModifiers = typeGroup.ToList();

            if (modifierType == ModifierType.Untyped)
            {
                // Untyped modifiers always stack
                result.TotalValue += typeModifiers.Sum(m => m.Value);
            }
            else
            {
                // Typed modifiers: only the highest bonus and lowest penalty stack
                var bonuses = typeModifiers.Where(m => m.Value > 0).ToList();
                var penalties = typeModifiers.Where(m => m.Value < 0).ToList();

                if (bonuses.Count > 1)
                {
                    var bestBonus = bonuses.Max(m => m.Value);
                    result.TotalValue += bestBonus;
                    result.StackingWarnings.Add($"Multiple {modifierType} bonuses don't stack. Only the highest (+{bestBonus}) applies.");
                }
                else if (bonuses.Count == 1)
                {
                    result.TotalValue += bonuses[0].Value;
                }

                if (penalties.Count > 1)
                {
                    var worstPenalty = penalties.Min(m => m.Value);
                    result.TotalValue += worstPenalty;
                    result.StackingWarnings.Add($"Multiple {modifierType} penalties don't stack. Only the worst ({worstPenalty}) applies.");
                }
                else if (penalties.Count == 1)
                {
                    result.TotalValue += penalties[0].Value;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get base character stats before modifiers
    /// </summary>
    private Dictionary<ModifierTarget, int> GetBaseStats(Character character)
    {
        var baseStats = new Dictionary<ModifierTarget, int>();

        // Base ability scores (assuming they're stored on character)
        // This would need to be implemented based on your character model
        baseStats[ModifierTarget.Strength] = 10; // character.BaseStrength
        baseStats[ModifierTarget.Dexterity] = 10; // character.BaseDexterity
        baseStats[ModifierTarget.Constitution] = 10; // character.BaseConstitution
        baseStats[ModifierTarget.Intelligence] = 10; // character.BaseIntelligence
        baseStats[ModifierTarget.Wisdom] = 10; // character.BaseWisdom
        baseStats[ModifierTarget.Charisma] = 10; // character.BaseCharisma

        // Derived stats
        baseStats[ModifierTarget.ArmorClass] = 10; // Base AC
        baseStats[ModifierTarget.HitPoints] = CalculateBaseHitPoints(character);
        baseStats[ModifierTarget.Initiative] = GetAbilityModifier(baseStats[ModifierTarget.Dexterity]);
        baseStats[ModifierTarget.Speed] = 25; // Base speed, varies by ancestry

        // Saving throws (base + ability modifier)
        baseStats[ModifierTarget.FortitudeSave] = GetAbilityModifier(baseStats[ModifierTarget.Constitution]);
        baseStats[ModifierTarget.ReflexSave] = GetAbilityModifier(baseStats[ModifierTarget.Dexterity]);
        baseStats[ModifierTarget.WillSave] = GetAbilityModifier(baseStats[ModifierTarget.Wisdom]);

        // Skills (base = ability modifier, plus proficiency if trained)
        baseStats[ModifierTarget.Acrobatics] = GetAbilityModifier(baseStats[ModifierTarget.Dexterity]);
        baseStats[ModifierTarget.Athletics] = GetAbilityModifier(baseStats[ModifierTarget.Strength]);
        baseStats[ModifierTarget.Stealth] = GetAbilityModifier(baseStats[ModifierTarget.Dexterity]);
        baseStats[ModifierTarget.Perception] = GetAbilityModifier(baseStats[ModifierTarget.Wisdom]);
        // ... add all other skills

        return baseStats;
    }

    /// <summary>
    /// Calculate final stat values by applying modifiers to base stats
    /// </summary>
    private void CalculateFinalValues(CalculatedCharacterStats stats)
    {
        foreach (var baseStat in stats.BaseStats)
        {
            var target = baseStat.Key;
            var baseValue = baseStat.Value;
            var modifier = stats.Modifiers.GetValueOrDefault(target)?.TotalValue ?? 0;
            
            stats.FinalStats[target] = baseValue + modifier;
        }

        // Handle special cases where one stat affects another
        RecalculateDependentStats(stats);
    }

    /// <summary>
    /// Recalculate stats that depend on other stats (e.g., skills depend on abilities)
    /// </summary>
    private void RecalculateDependentStats(CalculatedCharacterStats stats)
    {
        // Update ability-dependent stats when ability scores change
        var abilityTargets = new[]
        {
            ModifierTarget.Strength, ModifierTarget.Dexterity, ModifierTarget.Constitution,
            ModifierTarget.Intelligence, ModifierTarget.Wisdom, ModifierTarget.Charisma
        };

        foreach (var ability in abilityTargets)
        {
            if (!stats.FinalStats.ContainsKey(ability)) continue;
            
            var newModifier = GetAbilityModifier(stats.FinalStats[ability]);
            var oldModifier = GetAbilityModifier(stats.BaseStats[ability]);
            var modifierChange = newModifier - oldModifier;

            // Update dependent stats
            UpdateDependentStats(stats, ability, modifierChange);
        }
    }

    /// <summary>
    /// Update stats that depend on a specific ability score
    /// </summary>
    private void UpdateDependentStats(CalculatedCharacterStats stats, ModifierTarget ability, int modifierChange)
    {
        if (modifierChange == 0) return;

        var dependencies = GetStatDependencies(ability);
        
        foreach (var dependent in dependencies)
        {
            if (stats.FinalStats.ContainsKey(dependent))
            {
                stats.FinalStats[dependent] += modifierChange;
            }
        }
    }

    /// <summary>
    /// Get stats that depend on a specific ability score
    /// </summary>
    private List<ModifierTarget> GetStatDependencies(ModifierTarget ability)
    {
        return ability switch
        {
            ModifierTarget.Dexterity => new List<ModifierTarget>
            {
                ModifierTarget.Initiative,
                ModifierTarget.ReflexSave,
                ModifierTarget.Acrobatics,
                ModifierTarget.Stealth,
                ModifierTarget.Thievery
            },
            ModifierTarget.Constitution => new List<ModifierTarget>
            {
                ModifierTarget.FortitudeSave
                // HP would be recalculated separately due to its complexity
            },
            ModifierTarget.Strength => new List<ModifierTarget>
            {
                ModifierTarget.Athletics
            },
            ModifierTarget.Intelligence => new List<ModifierTarget>
            {
                ModifierTarget.Arcana,
                ModifierTarget.Crafting,
                ModifierTarget.Occultism,
                ModifierTarget.Society
            },
            ModifierTarget.Wisdom => new List<ModifierTarget>
            {
                ModifierTarget.WillSave,
                ModifierTarget.Medicine,
                ModifierTarget.Nature,
                ModifierTarget.Perception,
                ModifierTarget.Religion,
                ModifierTarget.Survival
            },
            ModifierTarget.Charisma => new List<ModifierTarget>
            {
                ModifierTarget.Deception,
                ModifierTarget.Diplomacy,
                ModifierTarget.Intimidation,
                ModifierTarget.Performance
            },
            _ => new List<ModifierTarget>()
        };
    }

    /// <summary>
    /// Validate modifier stacking rules and add warnings
    /// </summary>
    private void ValidateModifierStacking(CalculatedCharacterStats stats)
    {
        foreach (var kvp in stats.ModifierSources)
        {
            var target = kvp.Key;
            var sources = kvp.Value;

            // Check for conflicting modifiers
            var conflicts = FindStackingConflicts(sources);
            if (conflicts.Any())
            {
                if (!stats.ValidationWarnings.ContainsKey(target))
                {
                    stats.ValidationWarnings[target] = new List<string>();
                }
                stats.ValidationWarnings[target].AddRange(conflicts);
            }
        }
    }

    /// <summary>
    /// Find stacking conflicts in a list of modifier sources
    /// </summary>
    private List<string> FindStackingConflicts(List<ModifierSource> sources)
    {
        var conflicts = new List<string>();
        
        var typeGroups = sources.GroupBy(s => s.Type);
        
        foreach (var group in typeGroups)
        {
            if (group.Key == ModifierType.Untyped) continue;
            
            var bonuses = group.Where(s => s.Value > 0).ToList();
            var penalties = group.Where(s => s.Value < 0).ToList();
            
            if (bonuses.Count > 1)
            {
                var sourceNames = string.Join(", ", bonuses.Select(b => b.SourceName));
                conflicts.Add($"Multiple {group.Key} bonuses from: {sourceNames}. Only the highest applies.");
            }
            
            if (penalties.Count > 1)
            {
                var sourceNames = string.Join(", ", penalties.Select(p => p.SourceName));
                conflicts.Add($"Multiple {group.Key} penalties from: {sourceNames}. Only the worst applies.");
            }
        }
        
        return conflicts;
    }

    /// <summary>
    /// Calculate base hit points for a character
    /// </summary>
    private int CalculateBaseHitPoints(Character character)
    {
        // This would be based on class, level, constitution, etc.
        // Placeholder implementation
        return 8; // + constitution modifier + class HP per level
    }

    /// <summary>
    /// Get ability modifier from ability score
    /// </summary>
    private int GetAbilityModifier(int abilityScore)
    {
        return (abilityScore - 10) / 2;
    }
}

/// <summary>
/// Calculated character statistics with modifiers applied
/// </summary>
public class CalculatedCharacterStats
{
    public Guid CharacterId { get; set; }
    public Dictionary<ModifierTarget, int> BaseStats { get; set; } = new();
    public Dictionary<ModifierTarget, int> FinalStats { get; set; } = new();
    public Dictionary<ModifierTarget, ModifierResult> Modifiers { get; set; } = new();
    public Dictionary<ModifierTarget, List<ModifierSource>> ModifierSources { get; set; } = new();
    public Dictionary<ModifierTarget, List<string>> ValidationWarnings { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Get the total modifier for a specific target
    /// </summary>
    public int GetModifier(ModifierTarget target)
    {
        return Modifiers.GetValueOrDefault(target)?.TotalValue ?? 0;
    }

    /// <summary>
    /// Get the final value for a specific target
    /// </summary>
    public int GetFinalValue(ModifierTarget target)
    {
        return FinalStats.GetValueOrDefault(target, BaseStats.GetValueOrDefault(target, 0));
    }

    /// <summary>
    /// Get all sources contributing to a modifier
    /// </summary>
    public List<ModifierSource> GetModifierSources(ModifierTarget target)
    {
        return ModifierSources.GetValueOrDefault(target, new List<ModifierSource>());
    }

    /// <summary>
    /// Check if there are any validation warnings for a target
    /// </summary>
    public bool HasWarnings(ModifierTarget target)
    {
        return ValidationWarnings.ContainsKey(target) && ValidationWarnings[target].Any();
    }

    /// <summary>
    /// Get validation warnings for a target
    /// </summary>
    public List<string> GetWarnings(ModifierTarget target)
    {
        return ValidationWarnings.GetValueOrDefault(target, new List<string>());
    }
}

/// <summary>
/// Result of modifier calculation including stacking warnings
/// </summary>
public class ModifierResult
{
    public int TotalValue { get; set; }
    public List<string> StackingWarnings { get; set; } = new();
}

/// <summary>
/// Information about a modifier source
/// </summary>
public class ModifierSource
{
    public Guid SourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public int Value { get; set; }
    public ModifierType Type { get; set; }
    public string? Condition { get; set; }
}

/// <summary>
/// Extensions for working with calculated stats
/// </summary>
public static class CalculatedCharacterStatsExtensions
{
    /// <summary>
    /// Get a formatted breakdown of all modifiers for a target
    /// </summary>
    public static string GetModifierBreakdown(this CalculatedCharacterStats stats, ModifierTarget target)
    {
        var sources = stats.GetModifierSources(target);
        if (!sources.Any())
        {
            return "No modifiers";
        }

        var breakdown = sources.Select(s => 
        {
            var sign = s.Value >= 0 ? "+" : "";
            var condition = string.IsNullOrEmpty(s.Condition) ? "" : $" ({s.Condition})";
            var typeInfo = s.Type == ModifierType.Untyped ? "" : $" [{s.Type}]";
            return $"{s.SourceName}: {sign}{s.Value}{typeInfo}{condition}";
        });

        return string.Join("\n", breakdown);
    }

    /// <summary>
    /// Export stats to dictionary for API serialization
    /// </summary>
    public static Dictionary<string, object> ToApiResponse(this CalculatedCharacterStats stats)
    {
        return new Dictionary<string, object>
        {
            ["characterId"] = stats.CharacterId,
            ["calculatedAt"] = stats.CalculatedAt,
            ["baseStats"] = stats.BaseStats.ToDictionary(
                kvp => kvp.Key.ToString(), 
                kvp => kvp.Value),
            ["finalStats"] = stats.FinalStats.ToDictionary(
                kvp => kvp.Key.ToString(), 
                kvp => kvp.Value),
            ["modifiers"] = stats.Modifiers.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => new { 
                    value = kvp.Value.TotalValue, 
                    warnings = kvp.Value.StackingWarnings 
                }),
            ["warnings"] = stats.ValidationWarnings.Where(w => w.Value.Any()).ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value)
        };
    }
}