using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Validation;
using PathfinderCampaignManager.Domain.Errors;

namespace PathfinderCampaignManager.Infrastructure.Validation;

public class ValidationService : IValidationService
{
    private readonly IPathfinderDataRepository _dataRepository;
    private readonly IArchetypeService _archetypeService;

    public ValidationService(IPathfinderDataRepository dataRepository, IArchetypeService archetypeService)
    {
        _dataRepository = dataRepository;
        _archetypeService = archetypeService;
    }

    public async Task<Result<ValidationReport>> ValidateCharacterAsync(PfCharacter character)
    {
        var report = new ValidationReport
        {
            ValidatedEntityId = character.Id.ToString(),
            ValidatedEntityType = nameof(PfCharacter)
        };

        try
        {
            // Basic character validation
            await ValidateBasicCharacterInfo(character, report);
            
            // Ability scores validation
            var abilityScores = GetAbilityScoresDictionary(character.AbilityScores);
            await ValidateAbilityScores(abilityScores, character.Level, report);
            
            // Skills validation
            await ValidateSkills(character.Skills, character.AbilityScores, report);
            
            // Equipment validation
            await ValidateEquipment(character.Equipment, report);

            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    public async Task<Result<ValidationReport>> ValidateCalculatedCharacterAsync(ICalculatedCharacter character)
    {
        var report = new ValidationReport
        {
            ValidatedEntityId = character.Id.ToString(),
            ValidatedEntityType = nameof(ICalculatedCharacter)
        };

        try
        {
            // Validate ability scores
            await ValidateAbilityScores(character.AbilityScores, character.Level, report);
            
            // Validate proficiencies
            await ValidateProficiencies(character.Proficiencies, character.Level, report);
            
            // Validate feat slots and selections
            await ValidateFeatSlots(character.FeatSlots, character.Level, report);
            
            // Validate available feats against character build
            await ValidateAvailableFeats(character.AvailableFeats, character, report);

            // Validate combat stats
            await ValidateCombatStats(character, report);

            // Check for validation issues from the character itself
            foreach (var issue in character.ValidationIssues)
            {
                report.Issues.Add(issue);
            }

            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    public async Task<Result<ValidationReport>> ValidateCampaignAsync(string campaignId)
    {
        var report = new ValidationReport
        {
            ValidatedEntityId = campaignId,
            ValidatedEntityType = "Campaign"
        };

        try
        {
            // This would validate campaign-level data integrity
            // For now, we'll add placeholder validation
            report.AddSuggestion("Campaign", "Campaign validation not fully implemented", 
                "This feature will validate campaign data integrity");

            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Campaign validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    public async Task<Result<ValidationReport>> ValidateArchetypeProgressionAsync(string characterId, string archetypeId)
    {
        var report = new ValidationReport
        {
            ValidatedEntityId = $"{characterId}:{archetypeId}",
            ValidatedEntityType = "ArchetypeProgression"
        };

        try
        {
            // Use the archetype service to validate progression
            var validationResult = await _archetypeService.ValidateAllArchetypesAsync(new CalculatedCharacter { Id = Guid.Parse(characterId) });
            
            if (validationResult.IsSuccess)
            {
                foreach (var issue in validationResult.Value)
                {
                    report.Issues.Add(issue);
                }
            }
            else
            {
                report.AddIssue(ValidationSeverity.Error, "Archetype", validationResult.Error.Message);
            }

            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Archetype validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    public async Task<Result<ValidationReport>> ValidatePrerequisitesAsync(IEnumerable<PfPrerequisite> prerequisites, ICalculatedCharacter character)
    {
        var report = new ValidationReport
        {
            ValidatedEntityId = character.Id.ToString(),
            ValidatedEntityType = "Prerequisites"
        };

        try
        {
            foreach (var prerequisite in prerequisites)
            {
                var isValid = ValidatePrerequisite(prerequisite, character);
                if (!isValid)
                {
                    report.AddIssue(ValidationSeverity.Error, "Prerequisite", 
                        $"Prerequisite not met: {FormatPrerequisite(prerequisite)}",
                        $"Ensure character meets the {prerequisite.Type} requirement");
                }
            }

            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Prerequisite validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    public async Task<Result<ValidationReport>> ValidateSpellcastingAsync(ICalculatedCharacter character)
    {
        var report = new ValidationReport
        {
            ValidatedEntityId = character.Id.ToString(),
            ValidatedEntityType = "Spellcasting"
        };

        try
        {
            // Validate spellcasting would check:
            // - Spell slot usage
            // - Known spells vs. allowed
            // - Spell level access
            // - Multiclass spellcasting progression
            
            report.AddSuggestion("Spellcasting", "Spellcasting validation not fully implemented", 
                "This feature will validate spell slots, known spells, and multiclass progression");

            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Spellcasting validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    public async Task<Result<ValidationReport>> ValidateFeatSelectionAsync(ICalculatedCharacter character, string featId)
    {
        var report = new ValidationReport
        {
            ValidatedEntityId = $"{character.Id}:{featId}",
            ValidatedEntityType = "FeatSelection"
        };

        try
        {
            var featResult = await _dataRepository.GetFeatAsync(featId);
            if (featResult.IsFailure)
            {
                report.AddIssue(ValidationSeverity.Error, "Feat", $"Feat {featId} not found");
                return Result<ValidationReport>.Success(report);
            }

            var feat = featResult.Value;

            // Check prerequisites
            var prereqValidation = await ValidatePrerequisitesAsync(feat.Prerequisites, character);
            if (prereqValidation.IsSuccess && prereqValidation.Value.HasCriticalIssues)
            {
                report.AddIssue(ValidationSeverity.Error, "Feat", 
                    $"Cannot select {feat.Name}: prerequisites not met");
            }

            // Check if feat is already selected
            if (character.AvailableFeats.Contains(featId))
            {
                report.AddWarning("Feat", $"{feat.Name} is already selected", 
                    "Consider selecting a different feat");
            }

            // Check level requirements
            if (feat.Level > character.Level)
            {
                report.AddIssue(ValidationSeverity.Error, "Feat", 
                    $"Cannot select {feat.Name}: requires level {feat.Level}, character is level {character.Level}");
            }

            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Feat validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    public async Task<Result<ValidationReport>> ValidateAbilityScoreArrayAsync(Dictionary<string, int> abilityScores, int characterLevel)
    {
        var report = new ValidationReport
        {
            ValidatedEntityType = "AbilityScores"
        };

        try
        {
            await ValidateAbilityScores(abilityScores, characterLevel, report);
            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Ability score validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    public async Task<Result<ValidationReport>> ValidateEquipmentLoadAsync(ICalculatedCharacter character)
    {
        var report = new ValidationReport
        {
            ValidatedEntityId = character.Id.ToString(),
            ValidatedEntityType = "Equipment"
        };

        try
        {
            // Check encumbrance
            if (character.IsEncumbered)
            {
                report.AddWarning("Equipment", "Character is encumbered", 
                    $"Current bulk: {character.CurrentBulk}, Limit: {character.BulkLimit}");
                
                if (character.CurrentBulk > character.BulkLimit * 2)
                {
                    report.AddIssue(ValidationSeverity.Error, "Equipment", 
                        "Character is over maximum bulk limit and cannot move");
                }
            }

            // Add suggestions for optimization
            if (character.CurrentBulk > character.BulkLimit * 0.8)
            {
                report.AddSuggestion("Equipment", "Consider reducing carried equipment", 
                    "Free up bulk to avoid encumbrance penalties");
            }

            report.IsValid = !report.HasCriticalIssues;
            return Result<ValidationReport>.Success(report);
        }
        catch (Exception ex)
        {
            report.AddIssue(ValidationSeverity.Error, "System", $"Equipment validation failed: {ex.Message}");
            return Result<ValidationReport>.Success(report);
        }
    }

    // Private helper methods
    private async Task ValidateBasicCharacterInfo(PfCharacter character, ValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(character.Name))
        {
            report.AddIssue(ValidationSeverity.Error, "Character", "Character must have a name");
        }

        if (character.Level < 1 || character.Level > 20)
        {
            report.AddIssue(ValidationSeverity.Error, "Character", "Character level must be between 1 and 20");
        }

        if (string.IsNullOrWhiteSpace(character.Ancestry))
        {
            report.AddIssue(ValidationSeverity.Error, "Character", "Character must have an ancestry");
        }

        if (string.IsNullOrWhiteSpace(character.Background))
        {
            report.AddIssue(ValidationSeverity.Error, "Character", "Character must have a background");
        }

        if (string.IsNullOrWhiteSpace(character.ClassName))
        {
            report.AddIssue(ValidationSeverity.Error, "Character", "Character must have a class");
        }
    }

    private async Task ValidateAbilityScores(Dictionary<string, int> abilityScores, int level, ValidationReport report)
    {
        var requiredAbilities = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
        
        foreach (var ability in requiredAbilities)
        {
            if (!abilityScores.ContainsKey(ability))
            {
                report.AddIssue(ValidationSeverity.Error, "AbilityScores", $"Missing {ability} score");
                continue;
            }

            var score = abilityScores[ability];
            
            // Check for reasonable bounds
            if (score < 1)
            {
                report.AddIssue(ValidationSeverity.Error, "AbilityScores", $"{ability} score cannot be less than 1");
            }
            else if (score > 30)
            {
                report.AddIssue(ValidationSeverity.Error, "AbilityScores", $"{ability} score cannot exceed 30");
            }
            
            // Warnings for unusual scores
            if (score < 8 && level == 1)
            {
                report.AddWarning("AbilityScores", $"{ability} score of {score} is unusually low for a 1st level character", 
                    "Consider if this fits your character concept");
            }
            
            if (score > 18 && level == 1)
            {
                report.AddWarning("AbilityScores", $"{ability} score of {score} is unusually high for a 1st level character", 
                    "Verify this is correct for your ancestry and background bonuses");
            }
        }

        // Check for total point buy validity (simplified)
        var totalModifiers = abilityScores.Values.Sum(score => (score - 10) / 2);
        var expectedRange = level * 0.5; // Rough estimate
        
        if (Math.Abs(totalModifiers) > expectedRange * 3)
        {
            report.AddWarning("AbilityScores", "Ability score distribution seems unusual", 
                "Verify that ability score increases and bonuses are calculated correctly");
        }
    }

    private async Task ValidateSkills(List<PfCharacterSkill> skills, PfAbilityScores abilityScores, ValidationReport report)
    {
        foreach (var skill in skills)
        {
            if (string.IsNullOrWhiteSpace(skill.SkillName))
            {
                report.AddIssue(ValidationSeverity.Error, "Skills", "Skill must have a name");
                continue;
            }

            // Validate skill proficiencies are reasonable
            if (skill.Proficiency.Rank > ProficiencyRank.Legendary)
            {
                report.AddIssue(ValidationSeverity.Error, "Skills", 
                    $"{skill.SkillName} proficiency rank is invalid");
            }
        }
    }

    private async Task ValidateEquipment(List<string> equipment, ValidationReport report)
    {
        if (!equipment.Any())
        {
            report.AddWarning("Equipment", "Character has no equipment", 
                "Consider adding basic adventuring gear");
        }

        // Check for duplicates
        var duplicates = equipment.GroupBy(e => e).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var duplicate in duplicates)
        {
            report.AddWarning("Equipment", $"Duplicate equipment: {duplicate}", 
                "Remove duplicate entries unless multiple items are intended");
        }
    }

    private async Task ValidateProficiencies(Dictionary<string, ProficiencyLevel> proficiencies, int level, ValidationReport report)
    {
        foreach (var prof in proficiencies)
        {
            if (prof.Value > ProficiencyLevel.Legendary)
            {
                report.AddIssue(ValidationSeverity.Error, "Proficiencies", 
                    $"{prof.Key} proficiency level is invalid");
            }

            // Check if proficiency level is achievable at character level
            var maxProficiency = GetMaxProficiencyForLevel(level);
            if (prof.Value > maxProficiency)
            {
                report.AddWarning("Proficiencies", 
                    $"{prof.Key} proficiency ({prof.Value}) may be too high for level {level}", 
                    "Verify proficiency increases are correctly applied");
            }
        }
    }

    private async Task ValidateFeatSlots(Dictionary<string, List<FeatSlot>> featSlots, int level, ValidationReport report)
    {
        foreach (var category in featSlots)
        {
            foreach (var slot in category.Value)
            {
                if (slot.Level > level)
                {
                    report.AddIssue(ValidationSeverity.Error, "FeatSlots", 
                        $"Feat slot for level {slot.Level} cannot be used by level {level} character");
                }

                if (slot.IsRequired && string.IsNullOrEmpty(slot.SelectedFeatId))
                {
                    report.AddIssue(ValidationSeverity.Warning, "FeatSlots", 
                        $"Required {slot.Type} feat slot at level {slot.Level} is not filled");
                }
            }
        }
    }

    private async Task ValidateAvailableFeats(List<string> availableFeats, ICalculatedCharacter character, ValidationReport report)
    {
        // This would validate that all available feats are appropriate for the character
        // For now, just check for obviously invalid scenarios
        
        if (!availableFeats.Any())
        {
            report.AddWarning("Feats", "Character has no feats selected", 
                "Most characters should have at least some feats by level 2");
        }

        // Check for reasonable number of feats
        var expectedFeatCount = Math.Max(1, character.Level / 2); // Rough estimate
        if (availableFeats.Count > expectedFeatCount * 3)
        {
            report.AddWarning("Feats", "Character has an unusually high number of feats", 
                "Verify feat selections are correct");
        }
    }

    private async Task ValidateCombatStats(ICalculatedCharacter character, ValidationReport report)
    {
        if (character.HitPoints <= 0)
        {
            report.AddIssue(ValidationSeverity.Error, "Combat", "Character must have positive hit points");
        }

        if (character.ArmorClass < 10)
        {
            report.AddWarning("Combat", "Armor Class is very low", 
                "Consider improving armor or dexterity");
        }

        // Check for reasonable AC range
        var expectedAC = 10 + character.Level + 2; // Very rough estimate
        if (character.ArmorClass < expectedAC - 5)
        {
            report.AddSuggestion("Combat", "Consider improving AC through armor, shields, or abilities", 
                "Higher AC improves survivability in combat");
        }
    }

    private bool ValidatePrerequisite(PfPrerequisite prerequisite, ICalculatedCharacter character)
    {
        return prerequisite.Type switch
        {
            "AbilityScore" => ValidateAbilityScorePrerequisite(prerequisite, character),
            "Skill" => ValidateSkillPrerequisite(prerequisite, character),
            "Level" => character.Level >= int.Parse(prerequisite.Value),
            "Feat" => character.AvailableFeats.Contains(prerequisite.Target),
            _ => true // Unknown prerequisite types pass by default
        };
    }

    private bool ValidateAbilityScorePrerequisite(PfPrerequisite prerequisite, ICalculatedCharacter character)
    {
        if (!character.AbilityScores.TryGetValue(prerequisite.Target, out var score))
            return false;

        var requiredScore = int.Parse(prerequisite.Value);
        return prerequisite.Operator switch
        {
            ">=" => score >= requiredScore,
            ">" => score > requiredScore,
            "=" => score == requiredScore,
            "<=" => score <= requiredScore,
            "<" => score < requiredScore,
            _ => false
        };
    }

    private bool ValidateSkillPrerequisite(PfPrerequisite prerequisite, ICalculatedCharacter character)
    {
        if (!character.Proficiencies.TryGetValue(prerequisite.Target, out var proficiency))
            return false;

        var requiredRank = prerequisite.Value switch
        {
            "Trained" => ProficiencyLevel.Trained,
            "Expert" => ProficiencyLevel.Expert,
            "Master" => ProficiencyLevel.Master,
            "Legendary" => ProficiencyLevel.Legendary,
            _ => ProficiencyLevel.Untrained
        };

        return proficiency >= requiredRank;
    }

    private string FormatPrerequisite(PfPrerequisite prerequisite)
    {
        return prerequisite.Type switch
        {
            "AbilityScore" => $"{prerequisite.Target} {prerequisite.Value}+",
            "Skill" => $"{prerequisite.Target} ({prerequisite.Value})",
            "Level" => $"Level {prerequisite.Value}+",
            "Feat" => prerequisite.Target,
            _ => $"{prerequisite.Type}: {prerequisite.Target}"
        };
    }

    private Dictionary<string, int> GetAbilityScoresDictionary(PfAbilityScores abilityScores)
    {
        return new Dictionary<string, int>
        {
            ["Strength"] = abilityScores.Strength,
            ["Dexterity"] = abilityScores.Dexterity,
            ["Constitution"] = abilityScores.Constitution,
            ["Intelligence"] = abilityScores.Intelligence,
            ["Wisdom"] = abilityScores.Wisdom,
            ["Charisma"] = abilityScores.Charisma
        };
    }

    private ProficiencyLevel GetMaxProficiencyForLevel(int level)
    {
        return level switch
        {
            >= 15 => ProficiencyLevel.Legendary,
            >= 7 => ProficiencyLevel.Master,
            >= 3 => ProficiencyLevel.Expert,
            >= 1 => ProficiencyLevel.Trained,
            _ => ProficiencyLevel.Untrained
        };
    }
}