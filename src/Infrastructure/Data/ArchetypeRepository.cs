using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Errors;
using static PathfinderCampaignManager.Domain.Errors.GeneralErrors;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Infrastructure.Data;

public class ArchetypeRepository : IArchetypeRepository
{
    private readonly Dictionary<string, PfArchetype> _archetypes = new();
    private readonly Dictionary<string, PfFeat> _archetypeFeats = new();
    private bool _isInitialized = false;

    public async Task<Result<PfArchetype>> GetArchetypeByIdAsync(string id)
    {
        await InitializeAsync();
        
        if (_archetypes.TryGetValue(id, out var archetype))
        {
            return Result.Success(archetype);
        }
        
        return Result.Failure<PfArchetype>(GeneralErrors.NotFound);
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetArchetypesAsync()
    {
        await InitializeAsync();
        return Result<IEnumerable<PfArchetype>>.Success(_archetypes.Values.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetArchetypesByTypeAsync(ArchetypeType type)
    {
        await InitializeAsync();
        var archetypes = _archetypes.Values.Where(a => a.Type == type);
        return Result<IEnumerable<PfArchetype>>.Success(archetypes.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetMulticlassArchetypesAsync()
    {
        return await GetArchetypesByTypeAsync(ArchetypeType.Multiclass);
    }

    public async Task<Result<PfArchetype>> GetMulticlassArchetypeForClassAsync(string classId)
    {
        await InitializeAsync();
        var archetype = _archetypes.Values.FirstOrDefault(a => 
            a.Type == ArchetypeType.Multiclass && 
            a.AssociatedClassId == classId);
            
        if (archetype != null)
        {
            return Result.Success(archetype);
        }
        
        return Result.Failure<PfArchetype>(GeneralErrors.NotFound);
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetClassArchetypesAsync(string classId)
    {
        await InitializeAsync();
        var archetypes = _archetypes.Values.Where(a => 
            a.Type == ArchetypeType.Class && 
            (a.AssociatedClassId == classId || string.IsNullOrEmpty(a.AssociatedClassId)));
        return Result<IEnumerable<PfArchetype>>.Success(archetypes.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetGeneralArchetypesAsync()
    {
        return await GetArchetypesByTypeAsync(ArchetypeType.General);
    }

    public async Task<Result<PfFeat>> GetDedicationFeatAsync(string archetypeId)
    {
        await InitializeAsync();
        
        if (_archetypes.TryGetValue(archetypeId, out var archetype) && 
            _archetypeFeats.TryGetValue(archetype.DedicationFeatId, out var dedicationFeat))
        {
            return Result.Success(dedicationFeat);
        }
        
        return Result.Failure<PfFeat>(GeneralErrors.NotFound);
    }

    public async Task<Result<IEnumerable<PfFeat>>> GetArchetypeFeatsAsync(string archetypeId)
    {
        await InitializeAsync();
        
        if (_archetypes.TryGetValue(archetypeId, out var archetype))
        {
            var feats = archetype.ArchetypeFeatIds
                .Where(_archetypeFeats.ContainsKey)
                .Select(id => _archetypeFeats[id]);
            return Result<IEnumerable<PfFeat>>.Success(feats.AsEnumerable());
        }
        
        return Result<IEnumerable<PfFeat>>.Success(Enumerable.Empty<PfFeat>());
    }

    public async Task<Result<bool>> ValidatePrerequisitesAsync(string archetypeId, CalculatedCharacter character)
    {
        await InitializeAsync();
        
        if (!_archetypes.TryGetValue(archetypeId, out var archetype))
        {
            return Result.Failure<bool>(GeneralErrors.NotFound);
        }

        // Check each prerequisite
        foreach (var prerequisite in archetype.Prerequisites)
        {
            if (!ValidatePrerequisite(prerequisite, character))
            {
                return Result.Success(false);
            }
        }

        return Result.Success(true);
    }

    public async Task<Result<IEnumerable<PfFeat>>> GetAvailableArchetypeFeatsAsync(string archetypeId, CalculatedCharacter character)
    {
        await InitializeAsync();
        
        var allFeatsResult = await GetArchetypeFeatsAsync(archetypeId);
        if (allFeatsResult.IsFailure)
        {
            return Result.Failure<IEnumerable<PfFeat>>(allFeatsResult.Error);
        }

        var availableFeats = allFeatsResult.Value.Where(feat => 
            CanTakeFeat(feat, character)).ToList();

        return Result.Success(availableFeats.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfArchetype>>> SearchArchetypesAsync(string searchTerm)
    {
        await InitializeAsync();
        
        var results = _archetypes.Values.Where(a => 
            a.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            a.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            a.Traits.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));

        return Result<IEnumerable<PfArchetype>>.Success(results.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetArchetypesBySourceAsync(string source)
    {
        await InitializeAsync();
        var archetypes = _archetypes.Values.Where(a => a.Source.Equals(source, StringComparison.OrdinalIgnoreCase));
        return Result<IEnumerable<PfArchetype>>.Success(archetypes.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetArchetypesByTraitAsync(string trait)
    {
        await InitializeAsync();
        var archetypes = _archetypes.Values.Where(a => a.Traits.Contains(trait, StringComparer.OrdinalIgnoreCase));
        return Result<IEnumerable<PfArchetype>>.Success(archetypes.AsEnumerable());
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized) return;
        
        // Initialize archetype data
        InitializeArchetypeFeats();
        InitializeMulticlassArchetypes();
        InitializeClassArchetypes();
        InitializeGeneralArchetypes();
        
        _isInitialized = true;
    }

    private void InitializeMulticlassArchetypes()
    {
        // Barbarian Multiclass Archetype
        var barbarianArchetype = new PfArchetype
        {
            Id = "barbarian-multiclass",
            Name = "Barbarian",
            Description = "You have learned to harness and focus your rage, gaining access to powerful instinct abilities.",
            Type = ArchetypeType.Multiclass,
            AssociatedClassId = "barbarian",
            DedicationFeatId = "barbarian-dedication",
            ArchetypeFeatIds = new List<string> 
            { 
                "barbarian-dedication", 
                "barbarian-resiliency", 
                "basic-fury", 
                "advanced-fury", 
                "instinct-ability", 
                "juggernaut-fortitude" 
            },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "14" }
            },
            Traits = new List<string> { "Multiclass" },
            Source = "Core Rulebook"
        };
        _archetypes[barbarianArchetype.Id] = barbarianArchetype;

        // Fighter Multiclass Archetype
        var fighterArchetype = new PfArchetype
        {
            Id = "fighter-multiclass",
            Name = "Fighter",
            Description = "You have trained in the discipline of combat, gaining access to advanced weapon techniques.",
            Type = ArchetypeType.Multiclass,
            AssociatedClassId = "fighter",
            DedicationFeatId = "fighter-dedication",
            ArchetypeFeatIds = new List<string>
            {
                "fighter-dedication",
                "fighter-resiliency",
                "basic-maneuver",
                "advanced-maneuver",
                "diverse-weapon-expert"
            },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "14" },
                new() { Type = "AbilityScore", Target = "Dexterity", Operator = ">=", Value = "14" }
            },
            Traits = new List<string> { "Multiclass" },
            Source = "Core Rulebook"
        };
        _archetypes[fighterArchetype.Id] = fighterArchetype;

        // Wizard Multiclass Archetype
        var wizardArchetype = new PfArchetype
        {
            Id = "wizard-multiclass",
            Name = "Wizard",
            Description = "You have dabbled in the arcane arts, learning to cast spells through study and preparation.",
            Type = ArchetypeType.Multiclass,
            AssociatedClassId = "wizard",
            DedicationFeatId = "wizard-dedication",
            SpellcastingProgression = new PfMulticlassSpellcasting
            {
                Tradition = "Arcane",
                SpellcastingAbility = "Intelligence",
                PreparedCasting = true,
                MaxSpellLevel = 8,
                SpellSlotsFromMulticlass = new Dictionary<int, int>
                {
                    [1] = 1, [2] = 1, [3] = 1, [4] = 1, [5] = 1, [6] = 1, [7] = 1, [8] = 1
                }
            },
            ArchetypeFeatIds = new List<string>
            {
                "wizard-dedication",
                "basic-arcana",
                "basic-wizard-spellcasting",
                "advanced-arcana",
                "expert-wizard-spellcasting",
                "master-wizard-spellcasting"
            },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Intelligence", Operator = ">=", Value = "14" }
            },
            Traits = new List<string> { "Multiclass" },
            Source = "Core Rulebook"
        };
        _archetypes[wizardArchetype.Id] = wizardArchetype;
    }

    private void InitializeClassArchetypes()
    {
        // Example: Eldritch Archer (Fighter class archetype)
        var eldritchArcher = new PfArchetype
        {
            Id = "eldritch-archer",
            Name = "Eldritch Archer",
            Description = "You blend magic and marksmanship, infusing arrows with arcane power.",
            Type = ArchetypeType.Class,
            AssociatedClassId = "fighter",
            DedicationFeatId = "eldritch-archer-dedication",
            ArchetypeFeatIds = new List<string>
            {
                "eldritch-archer-dedication",
                "eldritch-shot",
                "energy-shot",
                "conjured-ammunition",
                "enchanted-quiver"
            },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Dexterity", Operator = ">=", Value = "14" },
                new() { Type = "AbilityScore", Target = "Intelligence", Operator = ">=", Value = "14" }
            },
            Traits = new List<string> { "Class" },
            Source = "Advanced Player's Guide"
        };
        _archetypes[eldritchArcher.Id] = eldritchArcher;
    }

    private void InitializeGeneralArchetypes()
    {
        // Example: Dual-Weapon Warrior
        var dualWeaponWarrior = new PfArchetype
        {
            Id = "dual-weapon-warrior",
            Name = "Dual-Weapon Warrior",
            Description = "You're able to effortlessly fight with two weapons simultaneously.",
            Type = ArchetypeType.General,
            DedicationFeatId = "dual-weapon-warrior-dedication",
            ArchetypeFeatIds = new List<string>
            {
                "dual-weapon-warrior-dedication",
                "dual-weapon-reload",
                "defensive-flourish",
                "dual-weapon-blitz",
                "twin-riposte"
            },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Dexterity", Operator = ">=", Value = "14" }
            },
            Traits = new List<string> { "General" },
            Source = "Advanced Player's Guide"
        };
        _archetypes[dualWeaponWarrior.Id] = dualWeaponWarrior;

        // Example: Medic
        var medic = new PfArchetype
        {
            Id = "medic",
            Name = "Medic",
            Description = "You become an expert in field medicine, able to treat wounds and diseases.",
            Type = ArchetypeType.General,
            DedicationFeatId = "medic-dedication",
            ArchetypeFeatIds = new List<string>
            {
                "medic-dedication",
                "battle-medicine",
                "continual-recovery",
                "ward-medic",
                "legendary-medic"
            },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "Skill", Target = "Medicine", Value = "Trained" }
            },
            Traits = new List<string> { "General" },
            Source = "Core Rulebook"
        };
        _archetypes[medic.Id] = medic;
    }

    private void InitializeArchetypeFeats()
    {
        // Barbarian Multiclass Feats
        _archetypeFeats["barbarian-dedication"] = new PfFeat
        {
            Id = "barbarian-dedication",
            Name = "Barbarian Dedication",
            Description = "You become trained in Athletics and gain the Rage action.",
            Level = 2,
            Traits = new List<string> { "Archetype", "Dedication", "Multiclass" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "14" }
            }
        };

        _archetypeFeats["barbarian-resiliency"] = new PfFeat
        {
            Id = "barbarian-resiliency",
            Name = "Barbarian Resiliency",
            Description = "You gain the barbarian's greater juggernaut class feature.",
            Level = 4,
            Traits = new List<string> { "Archetype" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "Feat", Target = "Barbarian Dedication" }
            }
        };

        _archetypeFeats["basic-fury"] = new PfFeat
        {
            Id = "basic-fury",
            Name = "Basic Fury",
            Description = "You gain a 1st or 2nd level barbarian feat.",
            Level = 4,
            Traits = new List<string> { "Archetype" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "Feat", Target = "Barbarian Dedication" }
            }
        };

        // Fighter Multiclass Feats
        _archetypeFeats["fighter-dedication"] = new PfFeat
        {
            Id = "fighter-dedication",
            Name = "Fighter Dedication",
            Description = "You become trained in all martial weapons.",
            Level = 2,
            Traits = new List<string> { "Archetype", "Dedication", "Multiclass" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "14" },
                new() { Type = "AbilityScore", Target = "Dexterity", Operator = ">=", Value = "14" }
            }
        };

        _archetypeFeats["basic-maneuver"] = new PfFeat
        {
            Id = "basic-maneuver",
            Name = "Basic Maneuver",
            Description = "You gain a 1st or 2nd level fighter feat.",
            Level = 4,
            Traits = new List<string> { "Archetype" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "Feat", Target = "Fighter Dedication" }
            }
        };

        // Wizard Multiclass Feats
        _archetypeFeats["wizard-dedication"] = new PfFeat
        {
            Id = "wizard-dedication",
            Name = "Wizard Dedication",
            Description = "You cast spells like a wizard, gaining a spellbook with four common arcane cantrips.",
            Level = 2,
            Traits = new List<string> { "Archetype", "Dedication", "Multiclass" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Intelligence", Operator = ">=", Value = "14" }
            }
        };

        _archetypeFeats["basic-arcana"] = new PfFeat
        {
            Id = "basic-arcana",
            Name = "Basic Arcana",
            Description = "You gain a 1st or 2nd level wizard feat.",
            Level = 4,
            Traits = new List<string> { "Archetype" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "Feat", Target = "Wizard Dedication" }
            }
        };

        _archetypeFeats["basic-wizard-spellcasting"] = new PfFeat
        {
            Id = "basic-wizard-spellcasting",
            Name = "Basic Wizard Spellcasting",
            Description = "You gain the basic spellcasting benefits.",
            Level = 4,
            Traits = new List<string> { "Archetype" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "Feat", Target = "Wizard Dedication" }
            }
        };

        // General Archetype Feats
        _archetypeFeats["dual-weapon-warrior-dedication"] = new PfFeat
        {
            Id = "dual-weapon-warrior-dedication",
            Name = "Dual-Weapon Warrior Dedication",
            Description = "You're able to effortlessly fight with two weapons simultaneously.",
            Level = 2,
            Traits = new List<string> { "Archetype", "Dedication" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Dexterity", Operator = ">=", Value = "14" }
            }
        };

        _archetypeFeats["medic-dedication"] = new PfFeat
        {
            Id = "medic-dedication",
            Name = "Medic Dedication",
            Description = "You become an expert in field medicine.",
            Level = 2,
            Traits = new List<string> { "Archetype", "Dedication" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "Skill", Target = "Medicine", Operator = ">=", Value = "Trained" }
            }
        };

        // Class Archetype Feats
        _archetypeFeats["eldritch-archer-dedication"] = new PfFeat
        {
            Id = "eldritch-archer-dedication",
            Name = "Eldritch Archer Dedication",
            Description = "You blend magic and marksmanship.",
            Level = 6,
            Traits = new List<string> { "Archetype", "Dedication" },
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Dexterity", Operator = ">=", Value = "14" },
                new() { Type = "AbilityScore", Target = "Intelligence", Operator = ">=", Value = "14" }
            }
        };
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
            "Trained" => ProficiencyLevel.Trained,
            "Expert" => ProficiencyLevel.Expert,
            "Master" => ProficiencyLevel.Master,
            "Legendary" => ProficiencyLevel.Legendary,
            _ => ProficiencyLevel.Untrained
        };

        return proficiency >= requiredRank;
    }

    private bool CanTakeFeat(PfFeat feat, CalculatedCharacter character)
    {
        // Check if character meets feat prerequisites
        foreach (var prerequisite in feat.Prerequisites)
        {
            if (!ValidatePrerequisite(prerequisite, character))
                return false;
        }

        // Check if feat is already known
        if (character.SelectedFeats.Any(f => f.Id == feat.Id))
            return false;

        return true;
    }
}