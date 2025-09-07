using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Infrastructure.RuleModules;

public class FreeArchetypeModule : IRuleModule
{
    public string Name => "Free Archetype";
    public int Priority => 200;
    public VariantRuleType RuleType => VariantRuleType.FreeArchetype;

    public void OnScores(Character character, CalculatedCharacter calculated)
    {
        // Free Archetype doesn't affect ability scores
    }

    public void OnProficiency(Character character, CalculatedCharacter calculated)
    {
        // Free Archetype doesn't affect proficiency directly
    }

    public void OnFeats(Character character, CalculatedCharacter calculated)
    {
        // Free Archetype doesn't change available feat lists directly
    }

    public void OnSlots(Character character, CalculatedCharacter calculated)
    {
        // Add dedicated archetype feat slots at even levels
        if (!calculated.FeatSlots.ContainsKey("Archetype"))
        {
            calculated.FeatSlots["Archetype"] = new List<FeatSlot>();
        }

        var archetypeSlots = calculated.FeatSlots["Archetype"];
        
        // Add archetype slots for even levels
        for (int level = 2; level <= calculated.Level; level += 2)
        {
            if (!archetypeSlots.Any(slot => slot.Level == level))
            {
                archetypeSlots.Add(new FeatSlot
                {
                    Type = "Archetype",
                    Level = level,
                    Category = "Free Archetype",
                    IsRequired = false
                });
            }
        }
    }

    public void OnEncumbrance(Character character, CalculatedCharacter calculated)
    {
        // Free Archetype doesn't affect encumbrance
    }

    public void OnValidation(Character character, CalculatedCharacter calculated)
    {
        // Validate archetype feat choices
        if (calculated.FeatSlots.TryGetValue("Archetype", out var archetypeSlots))
        {
            var selectedArchetypes = new HashSet<string>();
            
            foreach (var slot in archetypeSlots.Where(s => s.SelectedFeatId != null))
            {
                // In a real implementation, you'd look up the feat and check if it's a dedication feat
                // For now, we'll assume dedication feats end with "Dedication"
                if (slot.SelectedFeatId.EndsWith("Dedication"))
                {
                    var archetypeName = ExtractArchetypeName(slot.SelectedFeatId);
                    selectedArchetypes.Add(archetypeName);
                }
            }

            // Validate archetype prerequisites and restrictions
            foreach (var archetype in selectedArchetypes)
            {
                ValidateArchetypeProgression(calculated, archetype);
            }
        }

        calculated.ValidationIssues.Add(new ValidationIssue
        {
            Severity = ValidationSeverity.Info,
            Category = "Free Archetype",
            Message = "You gain archetype feats at even levels that don't count against your normal class feat progression"
        });
    }

    private string ExtractArchetypeName(string featId)
    {
        // Extract archetype name from dedication feat ID
        // This is a simplified implementation
        return featId.Replace("Dedication", "").Trim();
    }

    private void ValidateArchetypeProgression(CalculatedCharacter calculated, string archetypeName)
    {
        // Validate that the character has the dedication feat before taking other archetype feats
        var archetypeSlots = calculated.FeatSlots["Archetype"];
        var hasDedication = archetypeSlots.Any(slot => 
            slot.SelectedFeatId != null && 
            slot.SelectedFeatId.Contains($"{archetypeName}Dedication"));

        if (!hasDedication)
        {
            var archetypeFeats = archetypeSlots.Where(slot => 
                slot.SelectedFeatId != null && 
                slot.SelectedFeatId.Contains(archetypeName) && 
                !slot.SelectedFeatId.Contains("Dedication")).ToList();

            if (archetypeFeats.Any())
            {
                calculated.ValidationIssues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Free Archetype",
                    Message = $"You must take the {archetypeName} Dedication feat before taking other {archetypeName} archetype feats",
                    FixAction = $"AddFeat:{archetypeName}Dedication"
                });
            }
        }
    }
}