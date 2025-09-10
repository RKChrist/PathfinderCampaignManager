using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Infrastructure.Services;

public class ArchetypeService : IArchetypeService
{
    private readonly IArchetypeRepository _archetypeRepository;

    public ArchetypeService(IArchetypeRepository archetypeRepository)
    {
        _archetypeRepository = archetypeRepository;
    }

    public async Task<Result<bool>> CanTakeArchetypeFeatAsync(string archetypeId, string featId, CalculatedCharacter character)
    {
        // Check if character has the dedication feat
        var hasDedication = await HasDedicationFeatAsync(archetypeId, character);
        if (hasDedication.IsFailure)
        {
            return hasDedication;
        }

        if (!hasDedication.Value)
        {
            var dedicationResult = await _archetypeRepository.GetDedicationFeatAsync(archetypeId);
            if (dedicationResult.IsSuccess && dedicationResult.Value.Id == featId)
            {
                // This IS the dedication feat, check dedication prerequisites
                return await ValidateDedicationPrerequisitesAsync(archetypeId, character);
            }
            
            // Not the dedication feat and doesn't have dedication - can't take
            return Result.Success(false);
        }

        // Has dedication, check if they can take a new archetype feat
        return await CanTakeNewArchetypeAsync(character);
    }

    public async Task<Result<bool>> ValidateDedicationPrerequisitesAsync(string archetypeId, CalculatedCharacter character)
    {
        return await _archetypeRepository.ValidatePrerequisitesAsync(archetypeId, character);
    }

    public async Task<Result<bool>> HasRequiredArchetypeFeatsAsync(string currentArchetypeId, CalculatedCharacter character)
    {
        var archetypeResult = await _archetypeRepository.GetArchetypeByIdAsync(currentArchetypeId);
        if (archetypeResult.IsFailure)
        {
            return Result.Failure<bool>(archetypeResult.Error);
        }

        var archetype = archetypeResult.Value;
        if (!archetype.RequiresTwoFeatsBeforeNewArchetype)
        {
            return Result.Success(true);
        }

        // Count archetype feats for this archetype
        var featCount = await GetArchetypeFeatCountAsync(currentArchetypeId, character);
        if (featCount.IsFailure)
        {
            return Result.Failure<bool>(featCount.Error);
        }

        // Need at least 2 feats (including dedication) before taking another archetype
        return Result.Success(featCount.Value >= 2);
    }

    public async Task<Result<PfMulticlassSpellcasting>> CalculateMulticlassSpellcastingAsync(string archetypeId, int characterLevel)
    {
        var archetypeResult = await _archetypeRepository.GetArchetypeByIdAsync(archetypeId);
        if (archetypeResult.IsFailure)
        {
            return Result.Failure<PfMulticlassSpellcasting>(archetypeResult.Error);
        }

        var archetype = archetypeResult.Value;
        if (archetype.SpellcastingProgression == null)
        {
            return Result.Failure<PfMulticlassSpellcasting>(GeneralErrors.NotFound);
        }

        var progression = new PfMulticlassSpellcasting
        {
            Tradition = archetype.SpellcastingProgression.Tradition,
            SpellcastingAbility = archetype.SpellcastingProgression.SpellcastingAbility,
            PreparedCasting = archetype.SpellcastingProgression.PreparedCasting,
            MaxSpellLevel = Math.Min(archetype.SpellcastingProgression.MaxSpellLevel, characterLevel / 2),
            SpellSlotsFromMulticlass = CalculateSpellSlots(characterLevel, archetype.SpellcastingProgression),
            SpellsKnownFromMulticlass = CalculateSpellsKnown(characterLevel, archetype.SpellcastingProgression)
        };

        return Result.Success(progression);
    }

    public async Task<Result<Dictionary<int, int>>> GetMulticlassSpellSlotsAsync(string archetypeId, int characterLevel)
    {
        var spellcastingResult = await CalculateMulticlassSpellcastingAsync(archetypeId, characterLevel);
        if (spellcastingResult.IsFailure)
        {
            return Result.Failure<Dictionary<int, int>>(spellcastingResult.Error);
        }

        return Result.Success(spellcastingResult.Value.SpellSlotsFromMulticlass);
    }

    public async Task<Result<bool>> CanTakeNewArchetypeAsync(CalculatedCharacter character)
    {
        // Get all archetype feats the character has
        var archetypeFeats = GetCharacterArchetypeFeats(character);
        
        // Group by archetype
        var archetypeGroups = archetypeFeats.GroupBy(f => GetArchetypeFromFeatId(f.Id));
        
        foreach (var group in archetypeGroups)
        {
            var archetypeId = group.Key;
            var featCount = group.Count();
            
            // Check if this archetype requires two feats before taking another
            var archetypeResult = await _archetypeRepository.GetArchetypeByIdAsync(archetypeId);
            if (archetypeResult.IsSuccess && archetypeResult.Value.RequiresTwoFeatsBeforeNewArchetype)
            {
                if (featCount < 2)
                {
                    return Result.Success(false);
                }
            }
        }

        return Result.Success(true);
    }

    public async Task<Result<IEnumerable<string>>> GetBlockedArchetypesAsync(CalculatedCharacter character)
    {
        var blockedArchetypes = new List<string>();
        
        // Check for conflicting archetypes
        var currentArchetypes = GetCharacterArchetypes(character);
        
        // Add logic for mutually exclusive archetypes
        // For now, return empty list as most archetypes don't conflict
        
        return Result<IEnumerable<string>>.Success(blockedArchetypes.AsEnumerable());
    }

    public async Task<Result<int>> GetArchetypeFeatCountAsync(string archetypeId, CalculatedCharacter character)
    {
        var count = character.SelectedFeats.Count(f => GetArchetypeFromFeatId(f.Id) == archetypeId);
        return Result<int>.Success(count);
    }

    public async Task<Result<IEnumerable<PfFeat>>> GetNextAvailableFeatsAsync(string archetypeId, CalculatedCharacter character)
    {
        return await _archetypeRepository.GetAvailableArchetypeFeatsAsync(archetypeId, character);
    }

    public async Task<Result<bool>> ValidateArchetypeProgressionAsync(string archetypeId, CalculatedCharacter character)
    {
        // Check if character has dedication feat
        var hasDedication = await HasDedicationFeatAsync(archetypeId, character);
        if (hasDedication.IsFailure)
        {
            return hasDedication;
        }

        var archetypeFeats = character.SelectedFeats.Where(f => GetArchetypeFromFeatId(f.Id) == archetypeId).ToList();
        
        if (archetypeFeats.Any() && !hasDedication.Value)
        {
            // Has archetype feats but no dedication - invalid
            return Result.Success(false);
        }

        // Validate feat prerequisites are met
        foreach (var feat in archetypeFeats)
        {
            foreach (var prerequisite in feat.Prerequisites)
            {
                if (!ValidatePrerequisite(prerequisite, character))
                {
                    return Result.Success(false);
                }
            }
        }

        return Result.Success(true);
    }

    public async Task<Result<bool>> HasConflictingArchetypesAsync(string proposedArchetypeId, CalculatedCharacter character)
    {
        // Most archetypes don't conflict, but some might have restrictions
        // For example, some class archetypes might be mutually exclusive
        
        var archetypeResult = await _archetypeRepository.GetArchetypeByIdAsync(proposedArchetypeId);
        if (archetypeResult.IsFailure)
        {
            return Result.Failure<bool>(archetypeResult.Error);
        }

        var proposedArchetype = archetypeResult.Value;
        var currentArchetypes = GetCharacterArchetypes(character);

        // Check for class archetype conflicts
        if (proposedArchetype.Type == ArchetypeType.Class)
        {
            var hasConflictingClassArchetype = false;
            foreach (var currentArchetypeId in currentArchetypes)
            {
                var currentArchetypeResult = await _archetypeRepository.GetArchetypeByIdAsync(currentArchetypeId);
                if (currentArchetypeResult.IsSuccess)
                {
                    var currentArchetype = currentArchetypeResult.Value;
                    if (currentArchetype.Type == ArchetypeType.Class && 
                        currentArchetype.AssociatedClassId == proposedArchetype.AssociatedClassId &&
                        currentArchetype.Id != proposedArchetype.Id)
                    {
                        hasConflictingClassArchetype = true;
                        break;
                    }
                }
            }
            if (hasConflictingClassArchetype)
            {
                return Result.Success(true);
            }
        }

        return Result.Success(false);
    }

    public async Task<Result<IEnumerable<ValidationIssue>>> ValidateAllArchetypesAsync(CalculatedCharacter character)
    {
        var issues = new List<ValidationIssue>();
        var currentArchetypes = GetCharacterArchetypes(character);

        foreach (var archetypeId in currentArchetypes)
        {
            var validationResult = await ValidateArchetypeProgressionAsync(archetypeId, character);
            if (validationResult.IsFailure)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Archetype",
                    Message = $"Validation failed for archetype {archetypeId}: {validationResult.Error.Message}"
                });
            }
            else if (!validationResult.Value)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Archetype",
                    Message = $"Invalid progression for archetype {archetypeId}"
                });
            }
        }

        return Result<IEnumerable<ValidationIssue>>.Success(issues.AsEnumerable());
    }

    public async Task<Result<ArchetypeBenefits>> CalculateArchetypeBenefitsAsync(string archetypeId, int characterLevel)
    {
        var archetypeResult = await _archetypeRepository.GetArchetypeByIdAsync(archetypeId);
        if (archetypeResult.IsFailure)
        {
            return Result.Failure<ArchetypeBenefits>(DomainError.None);
        }

        var archetype = archetypeResult.Value;
        var benefits = new ArchetypeBenefits();

        // Calculate spellcasting benefits for multiclass archetypes
        if (archetype.Type == ArchetypeType.Multiclass && archetype.SpellcastingProgression != null)
        {
            var spellcastingResult = await CalculateMulticlassSpellcastingAsync(archetypeId, characterLevel);
            if (spellcastingResult.IsSuccess)
            {
                benefits.SpellcastingProgression = spellcastingResult.Value;
                benefits.SpellSlots = spellcastingResult.Value.SpellSlotsFromMulticlass.ToDictionary(
                    kvp => kvp.Key.ToString(), 
                    kvp => kvp.Value);
            }
        }

        return Result<ArchetypeBenefits>.Success(benefits);
    }

    private async Task<Result<bool>> HasDedicationFeatAsync(string archetypeId, CalculatedCharacter character)
    {
        var dedicationResult = await _archetypeRepository.GetDedicationFeatAsync(archetypeId);
        if (dedicationResult.IsFailure)
        {
            return Result.Failure<bool>(dedicationResult.Error);
        }

        var hasDedication = character.SelectedFeats.Any(f => f.Id == dedicationResult.Value.Id);
        return Result.Success(hasDedication);
    }

    private IEnumerable<PfFeat> GetCharacterArchetypeFeats(CalculatedCharacter character)
    {
        // Filter feats that belong to archetypes (simple heuristic: contain "archetype" trait or have archetype-style naming)
        return character.SelectedFeats.Where(f => 
            f.Traits.Contains("Archetype", StringComparer.OrdinalIgnoreCase) ||
            f.Traits.Contains("Dedication", StringComparer.OrdinalIgnoreCase) ||
            f.Traits.Contains("Multiclass", StringComparer.OrdinalIgnoreCase));
    }

    private IEnumerable<string> GetCharacterArchetypes(CalculatedCharacter character)
    {
        return GetCharacterArchetypeFeats(character)
            .Select(f => GetArchetypeFromFeatId(f.Id))
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct();
    }

    private string GetArchetypeFromFeatId(string featId)
    {
        // Simple heuristic to extract archetype ID from feat ID
        // In a real implementation, you'd have a mapping or query the feat for its archetype
        if (featId.Contains("barbarian")) return "barbarian-multiclass";
        if (featId.Contains("fighter")) return "fighter-multiclass";
        if (featId.Contains("wizard")) return "wizard-multiclass";
        if (featId.Contains("eldritch-archer")) return "eldritch-archer";
        if (featId.Contains("dual-weapon-warrior")) return "dual-weapon-warrior";
        if (featId.Contains("medic")) return "medic";
        
        return string.Empty;
    }

    private Dictionary<int, int> CalculateSpellSlots(int characterLevel, PfMulticlassSpellcasting progression)
    {
        var slots = new Dictionary<int, int>();
        var maxSpellLevel = Math.Min(progression.MaxSpellLevel, characterLevel / 2);

        for (int spellLevel = 1; spellLevel <= maxSpellLevel; spellLevel++)
        {
            // Multiclass spellcasting progression: very limited slots
            var slotsForLevel = spellLevel <= 4 ? 1 : 0; // Only spell levels 1-4 for most multiclass
            if (characterLevel >= spellLevel * 2 && slotsForLevel > 0)
            {
                slots[spellLevel] = slotsForLevel;
            }
        }

        return slots;
    }

    private Dictionary<int, int> CalculateSpellsKnown(int characterLevel, PfMulticlassSpellcasting progression)
    {
        var known = new Dictionary<int, int>();
        
        if (progression.PreparedCasting)
        {
            // Prepared casters don't track spells known separately
            return known;
        }

        var maxSpellLevel = Math.Min(progression.MaxSpellLevel, characterLevel / 2);
        for (int spellLevel = 1; spellLevel <= maxSpellLevel; spellLevel++)
        {
            if (characterLevel >= spellLevel * 2)
            {
                known[spellLevel] = 2; // Limited spells known for multiclass
            }
        }

        return known;
    }

    private bool ValidatePrerequisite(PfPrerequisite prerequisite, CalculatedCharacter character)
    {
        return prerequisite.Type switch
        {
            "AbilityScore" => ValidateAbilityScorePrerequisite(prerequisite, character),
            "Skill" => ValidateSkillPrerequisite(prerequisite, character),
            "Level" => character.Level >= int.Parse(prerequisite.Value),
            "Feat" => character.AvailableFeats.Contains(prerequisite.Target),
            _ => true
        };
    }

    private bool ValidateAbilityScorePrerequisite(PfPrerequisite prerequisite, CalculatedCharacter character)
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

    private bool ValidateSkillPrerequisite(PfPrerequisite prerequisite, CalculatedCharacter character)
    {
        if (!character.Proficiencies.TryGetValue(prerequisite.Target, out var proficiency))
            return false;

        var requiredRank = prerequisite.Value switch
        {
            "Trained" => ProficiencyRank.Trained,
            "Expert" => ProficiencyRank.Expert,
            "Master" => ProficiencyRank.Master,
            "Legendary" => ProficiencyRank.Legendary,
            _ => ProficiencyRank.Untrained
        };

        return proficiency >= requiredRank;
    }
}