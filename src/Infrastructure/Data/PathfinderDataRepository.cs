using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.ValueObjects;
using System.Text.Json;
using static PathfinderCampaignManager.Domain.Errors.GeneralErrors;

namespace PathfinderCampaignManager.Infrastructure.Data;

public class PathfinderDataRepository : IPathfinderDataRepository
{
    private readonly Dictionary<string, PfClass> _classes = new();
    private readonly Dictionary<string, PfFeat> _feats = new();
    private readonly Dictionary<string, PfSpell> _spells = new();
    private readonly Dictionary<string, PfAncestry> _ancestries = new();
    private readonly Dictionary<string, PfBackground> _backgrounds = new();
    private readonly Dictionary<string, PfArchetype> _archetypes = new();
    private readonly Dictionary<string, PfTrait> _traits = new();
    private readonly Dictionary<string, PfWeapon> _weapons = new();
    private readonly Dictionary<string, PfArmor> _armor = new();
    private readonly Dictionary<string, PfShield> _shields = new();
    private readonly Dictionary<string, PfEquipment> _equipment = new();
    private readonly Dictionary<string, PfAffliction> _afflictions = new();
    private readonly Dictionary<string, PfCondition> _conditions = new();
    private readonly Dictionary<string, PfMonster> _monsters = new();
    
    private bool _isInitialized = false;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        
        // Load all data from embedded resources or JSON files
        await LoadClassesAsync();
        await LoadFeatsAsync();
        await LoadSpellsAsync();
        await LoadAncestriesAsync();
        await LoadBackgroundsAsync();
        await LoadArchetypesAsync();
        await LoadTraitsAsync();
        await LoadWeaponsAsync();
        await LoadArmorAsync();
        await LoadEquipmentAsync();
        await LoadAfflictionsAsync();
        await LoadMonstersAsync();
        
        _isInitialized = true;
    }

    private async Task LoadClassesAsync()
    {
        // For now, we'll create the Fighter class as a complete implementation
        var fighter = CreateFighterClass();
        _classes[fighter.Id] = fighter;
        
        // Add placeholder for other classes that will be implemented
        var classIds = new[] { "barbarian", "bard", "champion", "cleric", "druid", "monk", "ranger", "rogue", "sorcerer", "wizard", "alchemist" };
        foreach (var id in classIds)
        {
            if (!_classes.ContainsKey(id))
            {
                _classes[id] = CreatePlaceholderClass(id);
            }
        }
        
        await Task.CompletedTask;
    }

    private async Task LoadFeatsAsync()
    {
        // Load core feats - we'll implement key ones
        LoadCoreFeats();
        await Task.CompletedTask;
    }

    private async Task LoadSpellsAsync()
    {
        // Load core spells - we'll implement key ones
        LoadCoreSpells();
        await Task.CompletedTask;
    }

    private async Task LoadAncestriesAsync()
    {
        // Load core ancestries
        LoadCoreAncestries();
        await Task.CompletedTask;
    }

    private async Task LoadBackgroundsAsync()
    {
        // Load core backgrounds
        LoadCoreBackgrounds();
        await Task.CompletedTask;
    }

    private async Task LoadArchetypesAsync()
    {
        // Load core archetypes
        LoadCoreArchetypes();
        await Task.CompletedTask;
    }

    private async Task LoadTraitsAsync()
    {
        // Load trait definitions
        LoadCoreTraits();
        await Task.CompletedTask;
    }

    // Classes
    public async Task<Result<PfClass>> GetClassAsync(string id)
    {
        await InitializeAsync();
        return _classes.TryGetValue(id, out var pfClass) 
            ? Result.Success(pfClass)
            : Result.Failure<PfClass>(new DomainError("CLASS_NOT_FOUND", $"Class with id '{id}' not found"));
    }

    public async Task<Result<IEnumerable<PfClass>>> GetAllClassesAsync()
    {
        await InitializeAsync();
        return Result.Success(_classes.Values.AsEnumerable());
    }

    // Feats
    public async Task<Result<PfFeat>> GetFeatAsync(string id)
    {
        await InitializeAsync();
        return _feats.TryGetValue(id, out var feat) 
            ? Result.Success(feat)
            : Result.Failure<PfFeat>(new DomainError("FEAT_NOT_FOUND", $"Feat with id '{id}' not found"));
    }

    public async Task<Result<IEnumerable<PfFeat>>> GetFeatsAsync(int? level = null, string? type = null, string? sourceId = null)
    {
        await InitializeAsync();
        var feats = _feats.Values.AsEnumerable();
        
        if (level.HasValue)
            feats = feats.Where(f => f.Level == level.Value);
        if (!string.IsNullOrEmpty(type))
            feats = feats.Where(f => f.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(sourceId))
            feats = feats.Where(f => f.Source.Equals(sourceId, StringComparison.OrdinalIgnoreCase));
            
        return Result.Success(feats);
    }

    public async Task<Result<IEnumerable<PfFeat>>> GetFeatsByTraitAsync(string trait)
    {
        await InitializeAsync();
        var feats = _feats.Values.Where(f => f.Traits.Contains(trait, StringComparer.OrdinalIgnoreCase));
        return Result.Success(feats);
    }

    public async Task<Result<IEnumerable<PfFeat>>> GetFeatsByPrerequisiteAsync(string prerequisiteType, string prerequisiteValue)
    {
        await InitializeAsync();
        var feats = _feats.Values.Where(f => 
            f.Prerequisites.Any(p => 
                p.Type.Equals(prerequisiteType, StringComparison.OrdinalIgnoreCase) &&
                p.Value.Equals(prerequisiteValue, StringComparison.OrdinalIgnoreCase)));
        return Result.Success(feats);
    }

    // Spells
    public async Task<Result<PfSpell>> GetSpellAsync(string id)
    {
        await InitializeAsync();
        return _spells.TryGetValue(id, out var spell) 
            ? Result.Success(spell)
            : Result.Failure<PfSpell>(new DomainError("SPELL_NOT_FOUND", $"Spell with id '{id}' not found"));
    }

    public async Task<Result<IEnumerable<PfSpell>>> GetSpellsAsync(int? level = null, string? tradition = null, string? school = null)
    {
        await InitializeAsync();
        var spells = _spells.Values.AsEnumerable();
        
        if (level.HasValue)
            spells = spells.Where(s => s.Level == level.Value);
        if (!string.IsNullOrEmpty(tradition))
            spells = spells.Where(s => s.Traditions.Contains(tradition, StringComparer.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(school))
            spells = spells.Where(s => s.School.Equals(school, StringComparison.OrdinalIgnoreCase));
            
        return Result.Success(spells);
    }

    public async Task<Result<IEnumerable<PfSpell>>> GetSpellsByTraitAsync(string trait)
    {
        await InitializeAsync();
        var spells = _spells.Values.Where(s => s.Traits.Contains(trait, StringComparer.OrdinalIgnoreCase));
        return Result.Success(spells);
    }

    public async Task<Result<IEnumerable<PfSpell>>> GetCantripsAsync()
    {
        await InitializeAsync();
        var cantrips = _spells.Values.Where(s => s.IsCantrip);
        return Result.Success(cantrips);
    }

    public async Task<Result<IEnumerable<PfSpell>>> GetFocusSpellsAsync()
    {
        await InitializeAsync();
        var focusSpells = _spells.Values.Where(s => s.IsFocus);
        return Result.Success(focusSpells);
    }

    // Ancestries
    public async Task<Result<PfAncestry>> GetAncestryAsync(string id)
    {
        await InitializeAsync();
        return _ancestries.TryGetValue(id, out var ancestry) 
            ? Result.Success(ancestry)
            : Result.Failure<PfAncestry>(new DomainError("ANCESTRY_NOT_FOUND", $"Ancestry with id '{id}' not found"));
    }

    public async Task<Result<IEnumerable<PfAncestry>>> GetAllAncestriesAsync()
    {
        await InitializeAsync();
        return Result.Success(_ancestries.Values.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfHeritage>>> GetHeritagesForAncestryAsync(string ancestryId)
    {
        await InitializeAsync();
        if (_ancestries.TryGetValue(ancestryId, out var ancestry))
        {
            return Result.Success(ancestry.Heritages.AsEnumerable());
        }
        return Result.Failure<IEnumerable<PfHeritage>>(new DomainError("ANCESTRY_NOT_FOUND", $"Ancestry with id '{ancestryId}' not found"));
    }

    // Backgrounds
    public async Task<Result<PfBackground>> GetBackgroundAsync(string id)
    {
        await InitializeAsync();
        return _backgrounds.TryGetValue(id, out var background) 
            ? Result.Success(background)
            : Result.Failure<PfBackground>(new DomainError("BACKGROUND_NOT_FOUND", $"Background with id '{id}' not found"));
    }

    public async Task<Result<IEnumerable<PfBackground>>> GetAllBackgroundsAsync()
    {
        await InitializeAsync();
        return Result.Success(_backgrounds.Values.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfBackground>>> GetBackgroundsByCategoryAsync(string category)
    {
        await InitializeAsync();
        var backgrounds = _backgrounds.Values.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        return Result.Success(backgrounds);
    }

    // Archetypes
    public async Task<Result<PfArchetype>> GetArchetypeAsync(string id)
    {
        await InitializeAsync();
        return _archetypes.TryGetValue(id, out var archetype) 
            ? Result.Success(archetype)
            : Result.Failure<PfArchetype>(new DomainError("ARCHETYPE_NOT_FOUND", $"Archetype with id '{id}' not found"));
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetAllArchetypesAsync()
    {
        await InitializeAsync();
        return Result.Success(_archetypes.Values.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfArchetype>>> GetMulticlassArchetypesAsync()
    {
        await InitializeAsync();
        var multiclassArchetypes = _archetypes.Values.Where(a => a.Type == ArchetypeType.Multiclass);
        return Result.Success(multiclassArchetypes);
    }

    // Traits
    public async Task<Result<PfTrait>> GetTraitAsync(string id)
    {
        await InitializeAsync();
        return _traits.TryGetValue(id, out var trait) 
            ? Result.Success(trait)
            : Result.Failure<PfTrait>(new DomainError("TRAIT_NOT_FOUND", $"Trait with id '{id}' not found"));
    }

    public async Task<Result<IEnumerable<PfTrait>>> GetAllTraitsAsync()
    {
        await InitializeAsync();
        return Result.Success(_traits.Values.AsEnumerable());
    }

    public async Task<Result<IEnumerable<PfTrait>>> GetTraitsByCategoryAsync(string category)
    {
        await InitializeAsync();
        var traits = _traits.Values.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        return Result.Success(traits);
    }

    // Validation
    public async Task<Result<bool>> ValidatePrerequisitesAsync(string characterId, string featId)
    {
        await InitializeAsync();
        // TODO: Implement prerequisite validation logic
        return Result.Success(true);
    }

    public async Task<Result<IEnumerable<string>>> GetMissingIds(IEnumerable<string> ids, string entityType)
    {
        await InitializeAsync();
        var missingIds = new List<string>();
        
        var repository = entityType.ToLowerInvariant() switch
        {
            "class" => _classes.Keys.AsEnumerable(),
            "feat" => _feats.Keys.AsEnumerable(),
            "spell" => _spells.Keys.AsEnumerable(),
            "ancestry" => _ancestries.Keys.AsEnumerable(),
            "background" => _backgrounds.Keys.AsEnumerable(),
            "archetype" => _archetypes.Keys.AsEnumerable(),
            "trait" => _traits.Keys.AsEnumerable(),
            _ => Enumerable.Empty<string>()
        };
        
        foreach (var id in ids)
        {
            if (!repository.Contains(id))
            {
                missingIds.Add(id);
            }
        }
        
        return Result.Success(missingIds.AsEnumerable());
    }

    public async Task<Result<Dictionary<string, int>>> GetDataCompletionStats()
    {
        await InitializeAsync();
        var stats = new Dictionary<string, int>
        {
            ["Classes"] = _classes.Count,
            ["Feats"] = _feats.Count,
            ["Spells"] = _spells.Count,
            ["Ancestries"] = _ancestries.Count,
            ["Backgrounds"] = _backgrounds.Count,
            ["Archetypes"] = _archetypes.Count,
            ["Traits"] = _traits.Count
        };
        
        return Result.Success(stats);
    }

    // Data creation methods - implementing complete Fighter class per SRD
    private PfClass CreateFighterClass()
    {
        var fighter = new PfClass
        {
            Id = "fighter",
            Name = "Fighter",
            Description = "Fighting for honor, greed, loyalty, or simply the thrill of battle, you are an undisputed master of weaponry and techniques of war. You combine your actions through clever combinations of opening moves, finishing strikes, and counterattacks whenever your foes are unwise enough to drop their guard. Whether you are a knight, mercenary, sharpshooter, or blade master, you have honed your martial skills into an art form and perform devastating critical attacks on your enemies.",
            KeyAbilities = new List<string> { "Strength", "Dexterity" },
            HitPoints = 10,
            SkillRanks = 3,
            Source = "Core Rulebook",
            Rarity = "Common",
            
            // Fighter progressions using the generic system
            FortitudeProgression = new Progression<SaveProgression>("fighter_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("fighter_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("fighter_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("fighter_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            // Class DC progression
            ClassDcProgression = new Progression<SpellcastingProgression>("fighter_class_dc", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master),
                new(19, Proficiency.Legendary)
            }),
            
            // Weapon progressions
            WeaponProgressions = new List<Progression<WeaponProgression>>
            {
                new("fighter_simple_weapons", new List<ProgressionStep>
                {
                    new(1, Proficiency.Expert),
                    new(5, Proficiency.Master),
                    new(13, Proficiency.Legendary)
                }),
                new("fighter_martial_weapons", new List<ProgressionStep>
                {
                    new(1, Proficiency.Expert),
                    new(5, Proficiency.Master),
                    new(13, Proficiency.Legendary)
                }),
                new("fighter_advanced_weapons", new List<ProgressionStep>
                {
                    new(1, Proficiency.Trained),
                    new(13, Proficiency.Expert)
                }),
                new("fighter_unarmed_attacks", new List<ProgressionStep>
                {
                    new(1, Proficiency.Expert),
                    new(5, Proficiency.Master),
                    new(13, Proficiency.Legendary)
                })
            },
            
            // Armor progressions
            ArmorProgressions = new List<Progression<ArmorProgression>>
            {
                new("fighter_unarmored_defense", new List<ProgressionStep>
                {
                    new(1, Proficiency.Trained),
                    new(13, Proficiency.Expert),
                    new(17, Proficiency.Master)
                }),
                new("fighter_light_armor", new List<ProgressionStep>
                {
                    new(1, Proficiency.Trained),
                    new(13, Proficiency.Expert),
                    new(17, Proficiency.Master)
                }),
                new("fighter_medium_armor", new List<ProgressionStep>
                {
                    new(1, Proficiency.Trained),
                    new(13, Proficiency.Expert),
                    new(17, Proficiency.Master)
                }),
                new("fighter_heavy_armor", new List<ProgressionStep>
                {
                    new(1, Proficiency.Trained),
                    new(13, Proficiency.Expert),
                    new(17, Proficiency.Master)
                })
            },
            
            // Class feat levels
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            // Class features by level (1-20)
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new() {
                        Id = "fighter_attack_of_opportunity",
                        Name = "Attack of Opportunity",
                        Description = "Ever watchful for weaknesses, you can quickly attack foes that leave an opening in their defenses. You gain the Attack of Opportunity reaction.",
                        Level = 1,
                        Type = "Class Feature",
                        ActionCost = "reaction",
                        Trigger = "A creature within your reach uses a manipulate action or a move action, makes a ranged attack, or leaves a square during a move action it's using.",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    },
                    new() {
                        Id = "fighter_shield_block",
                        Name = "Shield Block",
                        Description = "You gain the Shield Block general feat.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    }
                },
                
                [3] = new List<PfClassFeature>
                {
                    new() {
                        Id = "fighter_bravery",
                        Name = "Bravery",
                        Description = "Having faced countless foes and the chaos of battle, you have learned to overcome fear. Your proficiency rank for Will saves increases to expert. When you roll a success at a Will save against a fear effect, you get a critical success instead. In addition, any time you gain the frightened condition, reduce its value by 1.",
                        Level = 3,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    }
                },
                
                [5] = new List<PfClassFeature>
                {
                    new() {
                        Id = "fighter_fighter_weapon_mastery",
                        Name = "Fighter Weapon Mastery",
                        Description = "Hours spent training with your preferred weapons, learning and developing new combat techniques, have made you particularly effective with your weapons of choice. Your proficiency ranks for simple weapons, martial weapons, and unarmed attacks increase to master.",
                        Level = 5,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    }
                },
                
                [9] = new List<PfClassFeature>
                {
                    new() {
                        Id = "fighter_combat_flexibility",
                        Name = "Combat Flexibility",
                        Description = "Through your experience in battle, you can prepare for all kinds of circumstances. When you make your daily preparations, you can choose one fighter feat of 8th level or lower that you don't already have. You can use that feat until your next daily preparations. You must meet all of the feat's other prerequisites.",
                        Level = 9,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    },
                    new() {
                        Id = "fighter_juggernaut",
                        Name = "Juggernaut",
                        Description = "Your body is accustomed to physical hardship and resistant to ailments. Your proficiency rank for Fortitude saves increases to master. When you roll a success on a Fortitude save, you get a critical success instead.",
                        Level = 9,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    }
                },
                
                [13] = new List<PfClassFeature>
                {
                    new() {
                        Id = "fighter_weapon_legend",
                        Name = "Weapon Legend",
                        Description = "You've learned fighting techniques that apply to all armaments, and you've developed unparalleled skill with your weapons. Your proficiency ranks for simple weapons, martial weapons, and unarmed attacks increase to legendary, and your proficiency rank for advanced weapons increases to expert. You gain access to the critical specialization effects of all weapons and unarmed attacks when attacking your hunted prey.",
                        Level = 13,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    },
                    new() {
                        Id = "fighter_armor_expertise",
                        Name = "Armor Expertise",
                        Description = "You have spent so much time wearing armor that you know how to make the most of its protection. Your proficiency rank for light, medium, and heavy armor, as well as for unarmored defense, increase to expert.",
                        Level = 13,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    }
                },
                
                [15] = new List<PfClassFeature>
                {
                    new() {
                        Id = "fighter_greater_weapon_specialization",
                        Name = "Greater Weapon Specialization",
                        Description = "Your damage from weapon specialization increases to 4 with weapons and unarmed attacks in which you're an expert, 6 if you're a master, and 8 if you're legendary.",
                        Level = 15,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    }
                },
                
                [17] = new List<PfClassFeature>
                {
                    new() {
                        Id = "fighter_armor_mastery",
                        Name = "Armor Mastery",
                        Description = "Your skill with armor reaches the peak of mortal capability. Your proficiency rank for light, medium, and heavy armor, as well as for unarmored defense, increase to master.",
                        Level = 17,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    }
                },
                
                [19] = new List<PfClassFeature>
                {
                    new() {
                        Id = "fighter_versatile_legend",
                        Name = "Versatile Legend",
                        Description = "You are nigh-unmatched with any weapon. Your proficiency rank for your fighter class DC increases to legendary.",
                        Level = 19,
                        Type = "Class Feature",
                        Traits = new List<string> { "fighter" },
                        Source = "Core Rulebook"
                    }
                }
            }
        };
        
        return fighter;
    }
    
    private PfClass CreatePlaceholderClass(string id)
    {
        var names = new Dictionary<string, string>
        {
            ["barbarian"] = "Barbarian",
            ["bard"] = "Bard", 
            ["champion"] = "Champion",
            ["cleric"] = "Cleric",
            ["druid"] = "Druid",
            ["monk"] = "Monk",
            ["ranger"] = "Ranger",
            ["rogue"] = "Rogue",
            ["sorcerer"] = "Sorcerer",
            ["wizard"] = "Wizard",
            ["alchemist"] = "Alchemist"
        };
        
        return new PfClass
        {
            Id = id,
            Name = names.GetValueOrDefault(id, "Unknown"),
            Description = "Placeholder - Full implementation coming soon",
            KeyAbilities = new List<string> { "Strength" }, // Temporary
            HitPoints = 8, // Temporary
            SkillRanks = 2, // Temporary
            Source = "Core Rulebook",
            Rarity = "Common"
        };
    }
    
    private void LoadCoreFeats()
    {
        // Implementing key feats with full prerequisite and effect logic
        
        // Toughness - General Feat
        _feats["toughness"] = new PfFeat
        {
            Id = "toughness",
            Name = "Toughness", 
            Description = "You can withstand more punishment than most. Increase your maximum Hit Points by your level. The DC of recovery checks is equal to 9 + your dying condition value.",
            Level = 1,
            Type = "General",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "general" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "defense" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Modifier",
                    Target = "HitPoints", 
                    Value = "level",
                    Condition = "always"
                },
                new() {
                    Type = "Replace",
                    Target = "RecoveryDC",
                    Value = "9 + dying_value",
                    Condition = "when_dying"
                }
            }
        };
        
        // Shield Block - General Feat (also Fighter class feature)
        _feats["shield_block"] = new PfFeat
        {
            Id = "shield_block",
            Name = "Shield Block",
            Description = "You snap your shield in place to ward off a blow. Your shield prevents you from taking an amount of damage up to the shield's Hardness. You and the shield each take any remaining damage, possibly breaking or destroying the shield.",
            Level = 1,
            Type = "General",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "general" },
            ActionCost = "reaction",
            Trigger = "While you have your shield raised, you would take damage from a physical attack.",
            Requirements = "You are wielding a shield.",
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "defense" }
        };
        
        // Power Attack - Fighter Feat
        _feats["power_attack"] = new PfFeat
        {
            Id = "power_attack",
            Name = "Power Attack",
            Description = "You unleash a particularly powerful attack that clobbers your foe but leaves you a bit unsteady. Make a melee Strike. This counts as two attacks when calculating your multiple attack penalty. If this Strike hits, you deal an extra die of weapon damage. If you're at least 10th level, increase this to two extra dice, and if you're at least 18th level, increase it to three extra dice.",
            Level = 1,
            Type = "Class",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "fighter", "flourish" },
            ActionCost = "2",
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "offense" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Allow",
                    Target = "PowerAttack",
                    Value = "true",
                    Parameters = new Dictionary<string, object>
                    {
                        ["extra_dice_1_9"] = 1,
                        ["extra_dice_10_17"] = 2,
                        ["extra_dice_18_20"] = 3,
                        ["map_penalty"] = 2
                    }
                }
            }
        };
        
        // Sudden Charge - Fighter Feat  
        _feats["sudden_charge"] = new PfFeat
        {
            Id = "sudden_charge",
            Name = "Sudden Charge",
            Description = "With a quick sprint, you dash up to your foe and swing. Stride twice. If you end your movement within melee reach of at least one enemy, you can make a melee Strike against that enemy.",
            Level = 1,
            Type = "Class",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "fighter", "flourish", "open" },
            ActionCost = "2",
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "movement", "offense" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Allow",
                    Target = "SuddenCharge",
                    Value = "true",
                    Parameters = new Dictionary<string, object>
                    {
                        ["stride_count"] = 2,
                        ["allow_strike"] = true
                    }
                }
            }
        };
        
        // Terrain Stalker - Skill Feat with choice logic
        _feats["terrain_stalker"] = new PfFeat
        {
            Id = "terrain_stalker", 
            Name = "Terrain Stalker",
            Description = "Select one type of difficult terrain from the following list: rubble, snow, or underbrush. While undetected by all non-allies in that type of terrain, you can Sneak without attempting a Stealth check as long as you move no more than 5 feet and do not move within 10 feet of an enemy. This also allows you to automatically approach creatures to within 15 feet while Avoiding Notice during exploration as long as they aren't actively Seeking or on guard.",
            Level = 1,
            Type = "Skill",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "Proficiency",
                    Target = "Stealth",
                    Operator = ">=",
                    Value = "Trained"
                }
            },
            Traits = new List<string> { "general", "skill" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "skill", "stealth" },
            Choices = new List<PfFeatChoice>
            {
                new() {
                    Id = "terrain_stalker_terrain",
                    Name = "Terrain Type",
                    Type = "Terrain",
                    Options = new List<string> { "rubble", "snow", "underbrush" },
                    MaxSelections = 1,
                    IsRequired = true
                }
            },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Allow",
                    Target = "TerrainStalker",
                    Value = "true",
                    Condition = "in_chosen_terrain",
                    Parameters = new Dictionary<string, object>
                    {
                        ["terrain_type"] = "chosen",
                        ["sneak_without_check"] = true,
                        ["max_movement"] = 5,
                        ["avoid_notice_bonus"] = true
                    }
                }
            }
        };
        
        // Fleet - General Feat
        _feats["fleet"] = new PfFeat
        {
            Id = "fleet",
            Name = "Fleet",
            Description = "You move more quickly on foot. Your Speed increases by 5 feet.",
            Level = 1,
            Type = "General",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "general" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "movement" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Modifier",
                    Target = "Speed",
                    Value = "+5",
                    Condition = "always"
                }
            }
        };
        
        // Incredible Initiative - General Feat
        _feats["incredible_initiative"] = new PfFeat
        {
            Id = "incredible_initiative",
            Name = "Incredible Initiative",
            Description = "You react more quickly than others can. You gain a +2 circumstance bonus to initiative rolls.",
            Level = 1,
            Type = "General",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "general" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "combat" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Modifier",
                    Target = "Initiative",
                    Value = "+2",
                    Condition = "always"
                }
            }
        };
        
        // Ride - General Feat
        _feats["ride"] = new PfFeat
        {
            Id = "ride",
            Name = "Ride",
            Description = "When you're mounted, you gain the benefits of the Mount action. You can use actions that have the move trait even when you're mounted, and your mount moves along with you for as many of these actions as you command it to take, up to its Speed.",
            Level = 1,
            Type = "General",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "general" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "mount" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Allow",
                    Target = "Mount",
                    Value = "true",
                    Condition = "mounted"
                }
            }
        };
        
        // Blind-Fight - General Feat
        _feats["blind_fight"] = new PfFeat
        {
            Id = "blind_fight",
            Name = "Blind-Fight",
            Description = "Your battle instincts make you more aware of concealed and invisible opponents. You don't need to succeed at a flat check to target concealed creatures. You're not flat-footed to creatures that are hidden from you (unless you're flat-footed to them for reasons other than the hidden condition), and you need only a successful DC 5 flat check to target a hidden creature.",
            Level = 8,
            Type = "General",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "Proficiency",
                    Target = "Perception",
                    Operator = ">=",
                    Value = "Master"
                }
            },
            Traits = new List<string> { "general" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "perception" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Modify",
                    Target = "ConcealmentRules",
                    Value = "ignore_flat_check",
                    Condition = "targeting_concealed"
                },
                new() {
                    Type = "Modify",
                    Target = "HiddenTargetDC", 
                    Value = "5",
                    Condition = "targeting_hidden"
                }
            }
        };
        
        // Point-Blank Shot - Fighter Feat
        _feats["point_blank_shot"] = new PfFeat
        {
            Id = "point_blank_shot",
            Name = "Point-Blank Shot",
            Description = "You take aim to pick off nearby enemies quickly. When using a ranged volley weapon while you are in the weapon's volley range, you don't take the penalty to your attack roll. When using a ranged weapon that doesn't have the volley trait, you gain a +2 circumstance bonus to the attack roll when your target is within the weapon's first range increment.",
            Level = 1,
            Type = "Class",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "fighter" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "ranged" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Remove",
                    Target = "VolleyPenalty",
                    Value = "true",
                    Condition = "in_volley_range"
                },
                new() {
                    Type = "Modifier",
                    Target = "RangedAttack",
                    Value = "+2",
                    Condition = "first_range_increment"
                }
            }
        };
        
        // Double Slice - Fighter Feat
        _feats["double_slice"] = new PfFeat
        {
            Id = "double_slice",
            Name = "Double Slice",
            Description = "You lash out at your foe with both weapons. Make two Strikes, one with each of your two melee weapons, each using your current multiple attack penalty. Both Strikes must have the same target. If the second Strike is made with a weapon that doesn't have the agile trait, it takes a –2 penalty. If both attacks hit, combine their damage, and then add any applicable bonuses to your double slice. The target takes this damage only once, with resistances and weaknesses applied only once.",
            Level = 1,
            Type = "Class",
            Prerequisites = new List<PfPrerequisite>(),
            Traits = new List<string> { "fighter" },
            ActionCost = "2",
            Requirements = "You are wielding two melee weapons, each in a different hand.",
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "dual-wield", "offense" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Allow",
                    Target = "DoubleSlice",
                    Value = "true",
                    Parameters = new Dictionary<string, object>
                    {
                        ["combine_damage"] = true,
                        ["non_agile_penalty"] = -2
                    }
                }
            }
        };
        
        // Intimidating Glare - Skill Feat
        _feats["intimidating_glare"] = new PfFeat
        {
            Id = "intimidating_glare",
            Name = "Intimidating Glare", 
            Description = "You can Demoralize with a mere glare. When you do, Demoralize loses the auditory trait and gains the visual trait, and you don't take a penalty if the creature doesn't understand your language.",
            Level = 1,
            Type = "Skill",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "Proficiency",
                    Target = "Intimidation", 
                    Operator = ">=",
                    Value = "Trained"
                }
            },
            Traits = new List<string> { "general", "skill" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "skill", "intimidation" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Modify",
                    Target = "Demoralize",
                    Value = "visual_instead_of_auditory",
                    Parameters = new Dictionary<string, object>
                    {
                        ["remove_language_penalty"] = true
                    }
                }
            }
        };
        
        // Battle Medicine - Skill Feat
        _feats["battle_medicine"] = new PfFeat
        {
            Id = "battle_medicine",
            Name = "Battle Medicine",
            Description = "You can patch up wounds, even in combat. Attempt a Medicine check with the same DC as for Treat Wounds and restore the corresponding amount of HP; this doesn't remove the wounded condition. As with Treat Wounds, you can attempt checks against higher DCs if you have the minimum proficiency rank. The target is then temporarily immune to your Battle Medicine for 1 day.",
            Level = 1,
            Type = "Skill",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "Proficiency",
                    Target = "Medicine",
                    Operator = ">=",
                    Value = "Trained"
                }
            },
            Traits = new List<string> { "general", "skill" },
            ActionCost = "1",
            Requirements = "You are holding healer's tools, or you are wearing them and have a hand free.",
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "skill", "medicine", "healing" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Allow",
                    Target = "BattleMedicine",
                    Value = "true",
                    Parameters = new Dictionary<string, object>
                    {
                        ["uses_treat_wounds_dc"] = true,
                        ["immunity_duration"] = "1_day"
                    }
                }
            }
        };
        
        // Assurance - Skill Feat
        _feats["assurance"] = new PfFeat
        {
            Id = "assurance",
            Name = "Assurance",
            Description = "Even in the worst circumstances, you can perform basic tasks. Choose a skill you're trained in. You can forgo rolling a skill check for that skill to instead receive a result of 10 + your proficiency bonus (do not apply any other bonuses, penalties, or modifiers). Special: You can select this feat multiple times. Each time you select it, it applies to a different skill in which you're trained.",
            Level = 1,
            Type = "Skill",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "Proficiency",
                    Target = "any_skill",
                    Operator = ">=",
                    Value = "Trained"
                }
            },
            Traits = new List<string> { "general", "skill", "fortune" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "skill", "fortune" },
            Choices = new List<PfFeatChoice>
            {
                new() {
                    Id = "assurance_skill",
                    Name = "Skill",
                    Type = "Skill",
                    Options = new List<string> { "acrobatics", "arcana", "athletics", "crafting", "deception", "diplomacy", "intimidation", "lore", "medicine", "nature", "occultism", "perception", "performance", "religion", "society", "stealth", "survival", "thievery" },
                    MaxSelections = 1,
                    IsRequired = true
                }
            },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Allow",
                    Target = "Assurance",
                    Value = "true",
                    Condition = "using_chosen_skill",
                    Parameters = new Dictionary<string, object>
                    {
                        ["result"] = "10_plus_proficiency",
                        ["ignore_other_modifiers"] = true
                    }
                }
            }
        };
        
        // Cat Fall - Skill Feat
        _feats["cat_fall"] = new PfFeat
        {
            Id = "cat_fall",
            Name = "Cat Fall",
            Description = "Your catlike reflexes and balance allow you to always land on your feet and take less damage from falling. Treat falls as 10 feet shorter. If you're an expert in Acrobatics, treat falls as 25 feet shorter. If you're a master in Acrobatics, treat them as 50 feet shorter. If you're legendary in Acrobatics, you always land on your feet and don't take damage, regardless of the distance of the fall.",
            Level = 1,
            Type = "Skill",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "Proficiency",
                    Target = "Acrobatics",
                    Operator = ">=",
                    Value = "Trained"
                }
            },
            Traits = new List<string> { "general", "skill" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "skill", "acrobatics" },
            Effects = new List<PfFeatEffect>
            {
                new() {
                    Type = "Modify",
                    Target = "FallDamage",
                    Value = "reduce_by_proficiency",
                    Parameters = new Dictionary<string, object>
                    {
                        ["trained"] = "10_feet",
                        ["expert"] = "25_feet", 
                        ["master"] = "50_feet",
                        ["legendary"] = "immune"
                    }
                }
            }
        };
    }
    
    private void LoadCoreSpells()
    {
        // Implementing key spells with full data
        
        // Magic Missile - Evocation Cantrip -> 1st Level
        _spells["magic_missile"] = new PfSpell
        {
            Id = "magic_missile", 
            Name = "Magic Missile",
            Description = "You send a dart of force streaking toward a creature that you can see. It automatically hits and deals 1d4+1 force damage. For each additional action you use when Casting the Spell, increase the number of missiles you shoot by one, to a maximum of three missiles for 3 actions. You choose the target for each missile individually. If you shoot more than one missile at the same target, combine the damage before applying bonuses or penalties to damage, resistances, weaknesses, and so forth.",
            Level = 1,
            Traditions = new List<string> { "Arcane", "Occult" },
            School = "Evocation",
            Traits = new List<string> { "evocation", "force" },
            ActionCost = "1-3",
            Range = "120 feet",
            Targets = "1 creature",
            Duration = "instantaneous",
            Components = new List<string> { "somatic", "verbal" },
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "1d4+1",
                    DamageType = "force"
                }
            },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "offense", "force" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "+2",
                    Effect = "The spell creates one more missile.",
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "1d4+1",
                            DamageType = "force"
                        }
                    }
                }
            }
        };
        
        // Heal - Necromancy spell
        _spells["heal"] = new PfSpell
        {
            Id = "heal",
            Name = "Heal", 
            Description = "You channel positive energy to heal the living or damage the undead. If the target is a willing living creature, you restore 1d8 Hit Points. If the target is undead, you deal that amount of positive damage to it, and it gets a basic Fortitude save. When you cast heal, choose one of the following ways to use it.",
            Level = 1,
            Traditions = new List<string> { "Divine", "Primal" },
            School = "Necromancy",
            Traits = new List<string> { "necromancy", "healing", "positive" },
            ActionCost = "1-3",
            Range = "touch or 30 feet",
            Targets = "1 willing living creature or 1 undead creature",
            Duration = "instantaneous", 
            Components = new List<string> { "somatic", "verbal" },
            SavingThrow = "basic Fortitude (undead only)",
            Healing = new List<PfSpellHealing>
            {
                new() {
                    DiceFormula = "1d8",
                    Type = "Healing",
                    Condition = "target_is_living"
                }
            },
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "1d8", 
                    DamageType = "positive",
                    Condition = "target_is_undead"
                }
            },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "healing", "positive" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "+1",
                    Effect = "The amount of healing or damage increases by 1d8.",
                    AdditionalHealing = new List<PfSpellHealing>
                    {
                        new() {
                            DiceFormula = "1d8",
                            Type = "Healing"
                        }
                    },
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "1d8",
                            DamageType = "positive"
                        }
                    }
                }
            }
        };
        
        // Light - Evocation Cantrip
        _spells["light"] = new PfSpell
        {
            Id = "light",
            Name = "Light",
            Description = "The object glows, casting bright light in a 20-foot radius (and dim light for the next 20 feet) like a torch. If you target an object held or worn by a hostile creature, that creature can attempt a Reflex save to negate the effect.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Divine", "Occult", "Primal" },
            School = "Evocation",
            Traits = new List<string> { "cantrip", "evocation", "light" },
            ActionCost = "2", 
            Range = "touch",
            Targets = "1 object of 1 Bulk or less, either unattended or possessed by you or a willing ally",
            Duration = "until the next time you make your daily preparations",
            Components = new List<string> { "somatic", "verbal" },
            SavingThrow = "Reflex negates (hostile targets only)",
            IsCantrip = true,
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "utility", "light" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "4th",
                    Effect = "The light's radius increases to 60 feet of bright light (and 60 feet of dim light beyond that).",
                    AdditionalRange = "60 feet bright, 60 feet dim"
                }
            }
        };
        
        // Burning Hands - 1st Level Evocation
        _spells["burning_hands"] = new PfSpell
        {
            Id = "burning_hands",
            Name = "Burning Hands",
            Description = "Gouts of flame rush from your hands. You deal 2d6 fire damage to all creatures in the area. Each creature must attempt a basic Reflex save.",
            Level = 1,
            Traditions = new List<string> { "Arcane", "Primal" },
            School = "Evocation",
            Traits = new List<string> { "evocation", "fire" },
            ActionCost = "2",
            Area = "15-foot cone",
            Duration = "instantaneous",
            Components = new List<string> { "somatic", "verbal" },
            SavingThrow = "basic Reflex",
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "2d6",
                    DamageType = "fire"
                }
            },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "offense", "fire", "aoe" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "+1",
                    Effect = "The damage increases by 2d6.",
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "2d6",
                            DamageType = "fire"
                        }
                    }
                }
            }
        };
        
        // Shield - 1st Level Abjuration (Cantrip)
        _spells["shield"] = new PfSpell
        {
            Id = "shield",
            Name = "Shield",
            Description = "A shimmering field appears and shoos projectiles away from you. You gain a +1 circumstance bonus to AC until the start of your next turn. You can Dismiss the spell. While the spell is in effect, you can use the Shield Block reaction with your magic shield. The shield has Hardness 5. After the shield blocks damage, the spell ends and you can't cast shield again for 10 minutes. At 3rd level and every 2 levels thereafter, the shield's Hardness increases by 5.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Divine", "Occult" },
            School = "Abjuration",
            Traits = new List<string> { "cantrip", "abjuration" },
            ActionCost = "1",
            Range = "0",
            Duration = "until the start of your next turn",
            Components = new List<string> { "verbal" },
            IsCantrip = true,
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "defense" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "3rd",
                    Effect = "The shield's Hardness increases to 10, and it can block 20 damage before being destroyed.",
                    AdditionalValues = new Dictionary<string, object>
                    {
                        ["hardness"] = 10,
                        ["hp"] = 20
                    }
                },
                new() {
                    Level = "5th", 
                    Effect = "The shield's Hardness increases to 15, and it can block 30 damage.",
                    AdditionalValues = new Dictionary<string, object>
                    {
                        ["hardness"] = 15,
                        ["hp"] = 30
                    }
                }
            }
        };
        
        // Mage Hand - Evocation Cantrip
        _spells["mage_hand"] = new PfSpell
        {
            Id = "mage_hand",
            Name = "Mage Hand",
            Description = "You create a floating, magical hand, either invisible or ghostlike, that grasps the target object and moves it slowly up to 20 feet. Because you're directing the spell, you can move the object in any direction. When you Sustain the Spell, you can move the object an additional 20 feet. If the object is in the air when the spell ends, the object falls.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Occult" },
            School = "Evocation", 
            Traits = new List<string> { "cantrip", "evocation" },
            ActionCost = "2",
            Range = "30 feet",
            Targets = "1 unattended object of light Bulk or less",
            Duration = "sustained",
            Components = new List<string> { "somatic", "verbal" },
            IsCantrip = true,
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "utility" }
        };
        
        // Detect Magic - Divination Cantrip
        _spells["detect_magic"] = new PfSpell
        {
            Id = "detect_magic",
            Name = "Detect Magic",
            Description = "You send out a pulse that registers the presence of magic. You receive no information beyond the presence or absence of magic. You can choose to ignore magic you're fully aware of, such as the magic items and ongoing spells of you and your allies.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Divine", "Occult", "Primal" },
            School = "Divination",
            Traits = new List<string> { "cantrip", "divination", "detection" },
            ActionCost = "2",
            Area = "30-foot emanation",
            Duration = "instantaneous",
            Components = new List<string> { "somatic", "verbal" },
            IsCantrip = true,
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "detection" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "3rd",
                    Effect = "You learn the school of magic for the highest-level effect within range that the spell detects."
                },
                new() {
                    Level = "4th",
                    Effect = "As 3rd level, but you also pinpoint the source of the highest-level magic. Like for an imprecise sense, you don't learn the exact location, but can narrow down the source to within a 5-foot cube (or the nearest if larger than that)."
                }
            }
        };
        
        // Prestidigitation - Evocation Cantrip  
        _spells["prestidigitation"] = new PfSpell
        {
            Id = "prestidigitation",
            Name = "Prestidigitation",
            Description = "The simplest magic does your bidding. You can perform simple magical effects for as long as you Sustain the Spell. Each time you Sustain the Spell, you can choose one of four options.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Divine", "Occult", "Primal" },
            School = "Evocation",
            Traits = new List<string> { "cantrip", "evocation" },
            ActionCost = "2",
            Range = "10 feet",
            Targets = "1 object (cook, lift, or clean)",
            Duration = "sustained",
            Components = new List<string> { "somatic", "verbal" },
            IsCantrip = true,
            Source = "Core Rulebook", 
            Rarity = "Common",
            Tags = new List<string> { "utility" }
        };
        
        // Fireball - 3rd Level Evocation
        _spells["fireball"] = new PfSpell
        {
            Id = "fireball",
            Name = "Fireball",
            Description = "A roaring blast of fire appears at a spot you designate, dealing 6d6 fire damage.",
            Level = 3,
            Traditions = new List<string> { "Arcane", "Primal" },
            School = "Evocation", 
            Traits = new List<string> { "evocation", "fire" },
            ActionCost = "2",
            Range = "500 feet",
            Area = "20-foot burst",
            Duration = "instantaneous",
            Components = new List<string> { "somatic", "verbal" },
            SavingThrow = "basic Reflex",
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "6d6",
                    DamageType = "fire"
                }
            },
            Source = "Core Rulebook",
            Rarity = "Common", 
            Tags = new List<string> { "offense", "fire", "aoe" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "+1",
                    Effect = "The damage increases by 2d6.",
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "2d6",
                            DamageType = "fire"
                        }
                    }
                }
            }
        };
        
        // Bless - 1st Level Enchantment
        _spells["bless"] = new PfSpell
        {
            Id = "bless",
            Name = "Bless",
            Description = "Blessings from beyond help your companions strike true. You and your allies gain a +1 status bonus to attack rolls while within the area.",
            Level = 1,
            Traditions = new List<string> { "Divine", "Occult" },
            School = "Enchantment",
            Traits = new List<string> { "enchantment", "mental" },
            ActionCost = "2",
            Range = "30 feet",
            Area = "5-foot emanation, which affects you",
            Duration = "1 minute",
            Components = new List<string> { "somatic", "verbal" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "support" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "3rd",
                    Effect = "The bonus increases to +2."
                }
            }
        };
        
        // Sleep - 1st Level Enchantment
        _spells["sleep"] = new PfSpell
        {
            Id = "sleep",
            Name = "Sleep",
            Description = "Each creature in the area becomes drowsy and might fall asleep. A creature that falls asleep from this spell doesn't fall prone. The GM might allow the spell to have additional effects in certain areas or against certain targets.",
            Level = 1,
            Traditions = new List<string> { "Arcane", "Occult" },
            School = "Enchantment",
            Traits = new List<string> { "enchantment", "incapacitation", "mental", "sleep" },
            ActionCost = "2",
            Range = "30 feet",
            Area = "5-foot burst",
            Duration = "1 minute",
            Components = new List<string> { "somatic", "verbal" },
            SavingThrow = "Will",
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "control", "mental" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "4th",
                    Effect = "The creatures fall unconscious for 1 round on a failure (but not on a critical failure). They fall asleep for 1 minute on a critical failure.",
                    AdditionalValues = new Dictionary<string, object>
                    {
                        ["failure_duration"] = "1_round",
                        ["critical_failure_duration"] = "1_minute"
                    }
                }
            }
        };
        
        // Cure Wounds - 1st Level Necromancy  
        _spells["cure_wounds"] = new PfSpell
        {
            Id = "cure_wounds",
            Name = "Cure Wounds",
            Description = "You channel positive energy to heal the living or damage the undead. If the target is a living creature, you restore 1d8+4 Hit Points. If the target is undead, you deal that amount of positive damage to it, and it gets a basic Fortitude save.",
            Level = 1,
            Traditions = new List<string> { "Divine", "Primal" },
            School = "Necromancy",
            Traits = new List<string> { "necromancy", "healing", "positive" },
            ActionCost = "2",
            Range = "touch",
            Targets = "1 willing living creature or 1 undead",
            Duration = "instantaneous",
            Components = new List<string> { "somatic", "verbal" },
            SavingThrow = "basic Fortitude (undead only)",
            Healing = new List<PfSpellHealing>
            {
                new() {
                    DiceFormula = "1d8+4", 
                    Type = "Healing",
                    Condition = "target_is_living"
                }
            },
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "1d8+4",
                    DamageType = "positive", 
                    Condition = "target_is_undead"
                }
            },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "healing", "positive" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "+1",
                    Effect = "The amount of healing or damage increases by 1d8+4.",
                    AdditionalHealing = new List<PfSpellHealing>
                    {
                        new() {
                            DiceFormula = "1d8+4", 
                            Type = "Healing"
                        }
                    },
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "1d8+4",
                            DamageType = "positive"
                        }
                    }
                }
            }
        };
    }
    
    private void LoadCoreAncestries()
    {
        // Load comprehensive ancestry data from PathfinderGameData
        foreach (var (key, ancestry) in PathfinderGameData.Ancestries.CoreAncestriesData)
        {
            _ancestries[key] = ancestry;
        }
    }
    
    private void LoadCoreBackgrounds()
    {
        // Acolyte background
        _backgrounds["acolyte"] = new PfBackground
        {
            Id = "acolyte",
            Name = "Acolyte",
            Description = "You spent your early days in a religious monastery or cloister. You may have traveled out into the world to spread the message of your religion or because you cast away the teachings of your faith, but deep down you'll always carry within you the lessons you learned.",
            AbilityBoosts = new List<string> { "Intelligence", "Wisdom" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Religion" },
            LoreSkill = "Scribing Lore",
            GrantedFeat = "student_of_the_canon",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Noble background
        _backgrounds["noble"] = new PfBackground
        {
            Id = "noble", 
            Name = "Noble",
            Description = "To the common folk, the life of a noble seems one of idyllic luxury, but growing up as a noble or member of the aspiring gentry, you know the reality: a noble's lot is obligation and intrigue. Whether you seek to escape the burden of your duties or gain greater power to serve your noble family, you have trained in the intricacies of and politics of nobility.",
            AbilityBoosts = new List<string> { "Intelligence", "Charisma" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Society" },
            LoreSkill = "Heraldry Lore or Genealogy Lore",
            GrantedFeat = "courtly_graces",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Scholar background
        _backgrounds["scholar"] = new PfBackground
        {
            Id = "scholar",
            Name = "Scholar", 
            Description = "You have always been curious about the world around you and yearn to unlock the secrets of existence through diligent study, whether you've been raised as a scholar from birth, or you decided to become one later in life.",
            AbilityBoosts = new List<string> { "Intelligence", "Wisdom" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Arcana", "Nature", "Occultism", "Religion" }, // Choose one
            LoreSkill = "Academia Lore",
            GrantedFeat = "assurance_arcana_nature_occultism_or_religion",
            Category = "General",
            Source = "Core Rulebook",
            Features = new List<PfBackgroundFeature>
            {
                new() {
                    Id = "scholar_skill_choice",
                    Name = "Skill Training Choice",
                    Description = "Choose one: Arcana, Nature, Occultism, or Religion",
                    Type = "Choice",
                    Options = new List<string> { "Arcana", "Nature", "Occultism", "Religion" },
                    IsOptional = false
                }
            }
        };
        
        // Artisan background
        _backgrounds["artisan"] = new PfBackground
        {
            Id = "artisan",
            Name = "Artisan",
            Description = "You are a master of your craft, a learned artisan with an exceptional understanding of your chosen field and the ability to create valuable goods. Whether you set out on adventure to find rare materials to incorporate into your handiwork or used a different pursuit to finance your art, you remain a dedicated and skilled artisan.",
            AbilityBoosts = new List<string> { "Strength", "Intelligence" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Crafting" },
            LoreSkill = "Guild Lore",
            GrantedFeat = "specialty_crafting",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Criminal background  
        _backgrounds["criminal"] = new PfBackground
        {
            Id = "criminal",
            Name = "Criminal",
            Description = "You've lived a life of theft, violence, and other criminal activities. You might be a pickpocket who sneaks through crowds, a cat burglar who robs the rich, a highwayman who ambushes travelers on the road, or a killer who murders for money or pleasure. Your reasons for pursuing the criminal life may have been justified, or you may have been completely amoral.",
            AbilityBoosts = new List<string> { "Dexterity", "Intelligence" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Stealth" },
            LoreSkill = "Underworld Lore",
            GrantedFeat = "experienced_smuggler",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Detective background
        _backgrounds["detective"] = new PfBackground
        {
            Id = "detective", 
            Name = "Detective",
            Description = "You solved crimes as a town watch investigator or guard, finding clues others missed, or perhaps you were a traveling judge. You might have been a gumshoe in the big city, a guard in the city's watch, or a town sheriff. You have a keen sense for details and the skill to get information from reluctant witnesses.",
            AbilityBoosts = new List<string> { "Intelligence", "Wisdom" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Society" },
            LoreSkill = "Legal Lore",
            GrantedFeat = "streetwise",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Entertainer background
        _backgrounds["entertainer"] = new PfBackground
        {
            Id = "entertainer",
            Name = "Entertainer",
            Description = "Through an education in the arts or sheer dogged practice, you learned to entertain crowds. You might have been an actor, a dancer, a musician, a street magician, or any other sort of performer.",
            AbilityBoosts = new List<string> { "Dexterity", "Charisma" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Performance" },
            LoreSkill = "Theater Lore",
            GrantedFeat = "fascinating_performance",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Farmhand background
        _backgrounds["farmhand"] = new PfBackground
        {
            Id = "farmhand",
            Name = "Farmhand",
            Description = "With a strong back and an understanding of seasonal cycles, you tilled the land and tended crops. Your farm could have been razed by invaders, you could have lost the family tilling the land, or you might have simply yearned for a greater purpose.",
            AbilityBoosts = new List<string> { "Strength", "Constitution" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Athletics" },
            LoreSkill = "Farming Lore",
            GrantedFeat = "assurance_athletics",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Folk Hero background
        _backgrounds["folk_hero"] = new PfBackground
        {
            Id = "folk_hero",
            Name = "Folk Hero",
            Description = "You were once a simple apprentice, student, or village resident who saved your hometown. You might have stood up to a tyrant, slayed a monster, or dealt with some other supernatural threat. You are known throughout your hometown for your heroic deed.",
            AbilityBoosts = new List<string> { "Constitution", "Charisma" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Animal Handling", "Performance" },
            LoreSkill = "Regional Lore",
            GrantedFeat = "assurance_animal_handling_or_performance",
            Category = "General",
            Source = "Core Rulebook",
            Features = new List<PfBackgroundFeature>
            {
                new() {
                    Id = "folk_hero_skill_choice",
                    Name = "Skill Training Choice",
                    Description = "Choose one: Animal Handling or Performance",
                    Type = "Choice",
                    Options = new List<string> { "Animal Handling", "Performance" },
                    IsOptional = false
                }
            }
        };
        
        // Gladiator background
        _backgrounds["gladiator"] = new PfBackground
        {
            Id = "gladiator",
            Name = "Gladiator",
            Description = "The bloody games of the arena taught you the art of combat. Before you attained true fame, you departed—or escaped—the arena to pursue a greater destiny. You might have used an underground arena, or you might have been an animal trainer in a circus rather than a gladiator.",
            AbilityBoosts = new List<string> { "Strength", "Charisma" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Performance" },
            LoreSkill = "Gladiatorial Lore",
            GrantedFeat = "impressive_performance",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Guard background
        _backgrounds["guard"] = new PfBackground
        {
            Id = "guard",
            Name = "Guard",
            Description = "You served in the guard, out of either patriotism or need, and learned to look after other people. The training you received as a guard deepened your martial skill and gave you a head start in your new career.",
            AbilityBoosts = new List<string> { "Strength", "Charisma" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Intimidation" },
            LoreSkill = "Legal Lore",
            GrantedFeat = "quick_coercion",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Hermit background
        _backgrounds["hermit"] = new PfBackground
        {
            Id = "hermit",
            Name = "Hermit",
            Description = "In an isolated place—like a cave, remote oasis, or secluded island—you lived a life of solitude. You might have taken up this role in an act of devotion to a philosophical school or religion, or because you were exiled for a crime you did or didn't commit.",
            AbilityBoosts = new List<string> { "Constitution", "Intelligence", "Wisdom" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Nature", "Religion" },
            LoreSkill = "Herbalism Lore",
            GrantedFeat = "dubious_knowledge",
            Category = "General",
            Source = "Core Rulebook",
            Features = new List<PfBackgroundFeature>
            {
                new() {
                    Id = "hermit_skill_choice",
                    Name = "Skill Training Choice",
                    Description = "Choose one: Nature or Religion",
                    Type = "Choice",
                    Options = new List<string> { "Nature", "Religion" },
                    IsOptional = false
                }
            }
        };
        
        // Hunter background
        _backgrounds["hunter"] = new PfBackground
        {
            Id = "hunter",
            Name = "Hunter",
            Description = "You stalked and took down animals and other creatures of the wild. Skinning animals, harvesting their flesh, and cooking them were also part of your training, all of which gave you insight into their biology and behavior.",
            AbilityBoosts = new List<string> { "Dexterity", "Wisdom" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Survival" },
            LoreSkill = "Tanning Lore",
            GrantedFeat = "survey_wildlife",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Laborer background
        _backgrounds["laborer"] = new PfBackground
        {
            Id = "laborer",
            Name = "Laborer",
            Description = "You've spent years performing arduous physical labor. It was likely a thankless job, but you somehow survived the backbreaking work.",
            AbilityBoosts = new List<string> { "Strength", "Constitution" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Athletics" },
            LoreSkill = "Labor Lore",
            GrantedFeat = "hefty_hauler",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Merchant background
        _backgrounds["merchant"] = new PfBackground
        {
            Id = "merchant",
            Name = "Merchant",
            Description = "In a dusty shop, market stall, or merchant caravan, you bartered wares for coin and trade goods. The skills you picked up still apply in the field, where a good deal on a suit of armor could prevent your death.",
            AbilityBoosts = new List<string> { "Intelligence", "Charisma" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Diplomacy" },
            LoreSkill = "Mercantile Lore",
            GrantedFeat = "bargain_hunter",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Miner background
        _backgrounds["miner"] = new PfBackground
        {
            Id = "miner",
            Name = "Miner",
            Description = "You earned a living wrenching precious minerals from the lightless depths of the earth. Dangerous cave-ins, burrowing monsters, and toxic gases were all part of your job, and you learned to spot such hazards and quickly respond to their dangers.",
            AbilityBoosts = new List<string> { "Strength", "Wisdom" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Survival" },
            LoreSkill = "Mining Lore",
            GrantedFeat = "terrain_expertise_underground",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Sailor background
        _backgrounds["sailor"] = new PfBackground
        {
            Id = "sailor",
            Name = "Sailor",
            Description = "You heard the call of the sea from a young age. Perhaps you signed onto a merchant's vessel, joined the navy, or even fell in with a crew of pirates or smugglers.",
            AbilityBoosts = new List<string> { "Strength", "Dexterity" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Athletics" },
            LoreSkill = "Sailing Lore",
            GrantedFeat = "underwater_marauder",
            Category = "General",
            Source = "Core Rulebook"
        };
        
        // Street Urchin background
        _backgrounds["street_urchin"] = new PfBackground
        {
            Id = "street_urchin",
            Name = "Street Urchin",
            Description = "You eked out a living by picking pockets on the streets of a major city, never knowing where you'd find your next meal. While some folk adventure for the glory, you do so to survive.",
            AbilityBoosts = new List<string> { "Dexterity", "Constitution" },
            FreeAbilityBoosts = 1,
            SkillTraining = new List<string> { "Thievery" },
            LoreSkill = "City Lore",
            GrantedFeat = "pickpocket",
            Category = "General",
            Source = "Core Rulebook"
        };
    }
    
    private void LoadCoreArchetypes()
    {
        // Fighter multiclass archetype
        _archetypes["fighter_multiclass"] = new PfArchetype
        {
            Id = "fighter_multiclass",
            Name = "Fighter",
            Description = "You have spent time learning the art of warfare and know how to wield many weapons proficiently. This archetype is for non-fighters to gain some of the fighter's weapon expertise.",
            Type = ArchetypeType.Multiclass,
            AssociatedClassId = "fighter",
            DedicationFeatId = "fighter_dedication",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "AbilityScore", 
                    Target = "Strength",
                    Operator = ">=",
                    Value = "14",
                    Alternative = "Dexterity 14"
                }
            },
            ArchetypeFeatIds = new List<string>
            {
                "fighter_dedication",
                "basic_maneuver",
                "fighter_resiliency", 
                "opportunist",
                "advanced_maneuver",
                "diverse_weapon_expert"
            },
            Traits = new List<string> { "archetype", "multiclass" },
            Source = "Core Rulebook"
        };
    }
    
    private void LoadCoreTraits()
    {
        // Load essential trait definitions
        var coreTraits = new Dictionary<string, (string description, string category)>
        {
            ["magical"] = ("This magic comes from a magical tradition, such as arcane, divine, occult, or primal.", "Magical"),
            ["attack"] = ("An ability with this trait involves an attack roll.", "Combat"),
            ["evocation"] = ("Effects and magic items with this trait are associated with the evocation school of magic, typically involving energy and elemental forces.", "School"),
            ["force"] = ("Effects with this trait deal force damage or create objects made of pure magical force.", "Damage"),
            ["healing"] = ("A healing effect restores a creature's body, typically by restoring Hit Points, but sometimes by removing diseases or other debilitating effects.", "Effect"),
            ["positive"] = ("Effects with this trait heal living creatures with positive energy, deal positive energy damage to undead, or manipulate positive energy.", "Energy"),
            ["necromancy"] = ("Effects and magic items with this trait are associated with the necromancy school of magic, typically involving forces of life and death.", "School"),
            ["cantrip"] = ("A spell you can cast at will that is automatically heightened to half your level rounded up.", "Spell"),
            ["light"] = ("Light effects overcome non-magical darkness in the area, and can counteract magical darkness.", "Effect"),
            ["general"] = ("A type of feat that any character can select, regardless of ancestry and class, as long as they meet the prerequisites.", "Feat"),
            ["skill"] = ("A general feat with the skill trait improves your skills and their associated actions or gives you new actions for a skill.", "Feat"),
            ["fighter"] = ("This indicates abilities from the fighter class.", "Class"),
            ["flourish"] = ("Flourish actions finish an encounter with a spectacular display. You can use only one action with the flourish trait per turn.", "Combat"),
            ["open"] = ("These maneuvers work only as the first salvo on your turn. You can use an open only if you haven't used an action with the attack or open trait yet this turn.", "Combat"),
            ["human"] = ("A creature with this trait is a member of the human ancestry.", "Ancestry"),
            ["humanoid"] = ("Humanoid creatures reason and act much like humans. They typically stand upright and have two arms and two legs.", "Creature Type"),
            ["elf"] = ("A creature with this trait is a member of the elf ancestry.", "Ancestry"),
            ["archetype"] = ("This feat belongs to an archetype and isn't available at 1st level.", "Feat"),
            ["multiclass"] = ("Archetypes with the multiclass trait represent diversifying your training into another class's specialties.", "Archetype")
        };
        
        foreach (var (id, (description, category)) in coreTraits)
        {
            _traits[id] = new PfTrait
            {
                Id = id,
                Name = id.Replace("_", " ").Replace("-", " "),
                Description = description,
                Category = category,
                Source = "Core Rulebook",
                HasMechanicalEffect = id switch
                {
                    "cantrip" => true,
                    "flourish" => true, 
                    "open" => true,
                    "healing" => true,
                    "attack" => true,
                    _ => false
                }
            };
        }
    }
    
    private async Task LoadWeaponsAsync()
    {
        LoadCoreWeapons();
        await Task.CompletedTask;
    }
    
    private void LoadCoreWeapons()
    {
        // Simple Weapons
        _weapons["club"] = new PfWeapon
        {
            Name = "Club",
            Description = "This is a piece of stout wood shaped or repurposed to bludgeon an enemy. Clubs can be intricately carved pieces passed down through generations or simple branches from a tree.",
            Type = PfItemType.Weapon,
            Level = 0,
            Price = "0 gp",
            Bulk = "1",
            Category = "Simple",
            Group = "Club",
            Damage = "1d6 B",
            DamageType = "Bludgeoning",
            Range = 10,
            Traits = new List<string> { "simple", "weapon" },
            WeaponTraits = new List<string> { "thrown" },
            Rarity = "Common",
            Source = "Core Rulebook"
        };
        
        _weapons["dagger"] = new PfWeapon
        {
            Name = "Dagger",
            Description = "This small, bladed weapon is held in one hand and used to stab a creature in close combat. It can also be thrown.",
            Type = PfItemType.Weapon,
            Level = 0,
            Price = "2 sp",
            Bulk = "L",
            Category = "Simple",
            Group = "Knife",
            Damage = "1d4 P",
            DamageType = "Piercing",
            Range = 10,
            Traits = new List<string> { "simple", "weapon" },
            WeaponTraits = new List<string> { "agile", "finesse", "thrown", "versatile S" },
            Rarity = "Common",
            Source = "Core Rulebook"
        };
        
        _weapons["shortbow"] = new PfWeapon
        {
            Name = "Shortbow",
            Description = "This smaller bow is made of a single piece of wood and favored by skirmishers and hunters.",
            Type = PfItemType.Weapon,
            Level = 0,
            Price = "3 gp",
            Bulk = "1",
            Category = "Simple",
            Group = "Bow",
            Damage = "1d6 P",
            DamageType = "Piercing",
            Range = 60,
            Reload = "-",
            Traits = new List<string> { "simple", "weapon" },
            WeaponTraits = new List<string> { "deadly d10" },
            Rarity = "Common",
            Source = "Core Rulebook"
        };
        
        // Martial Weapons
        _weapons["longsword"] = new PfWeapon
        {
            Name = "Longsword",
            Description = "Longswords can be one-edged or two‑edged swords. Their blades are heavy and they're between 3 and 4 feet in length.",
            Type = PfItemType.Weapon,
            Level = 0,
            Price = "1 gp",
            Bulk = "1",
            Category = "Martial",
            Group = "Sword",
            Damage = "1d8 S",
            DamageType = "Slashing",
            Range = 0,
            Traits = new List<string> { "martial", "weapon" },
            WeaponTraits = new List<string> { "versatile P" },
            Rarity = "Common",
            Source = "Core Rulebook"
        };
        
        _weapons["battleaxe"] = new PfWeapon
        {
            Name = "Battleaxe",
            Description = "These axes are designed explicitly as weapons, rather than tools. They typically weigh less than the axes favored by woodcutters.",
            Type = PfItemType.Weapon,
            Level = 0,
            Price = "1 gp", 
            Bulk = "1",
            Category = "Martial",
            Group = "Axe",
            Damage = "1d8 S",
            DamageType = "Slashing",
            Range = 0,
            Traits = new List<string> { "martial", "weapon" },
            WeaponTraits = new List<string> { "sweep" },
            Rarity = "Common",
            Source = "Core Rulebook"
        };
        
        _weapons["longbow"] = new PfWeapon
        {
            Name = "Longbow",
            Description = "This 5-foot-tall bow, usually made of yew, is long and powerful, capable of accurately sending arrows to great distances.",
            Type = PfItemType.Weapon,
            Level = 0,
            Price = "6 gp",
            Bulk = "2",
            Category = "Martial",
            Group = "Bow",
            Damage = "1d8 P",
            DamageType = "Piercing",
            Range = 100,
            Reload = "0",
            Traits = new List<string> { "martial", "weapon" },
            WeaponTraits = new List<string> { "deadly d10", "propulsive", "volley 30 ft." },
            Rarity = "Common",
            Source = "Core Rulebook"
        };
    }
    
    private async Task LoadArmorAsync()
    {
        LoadCoreArmor();
        LoadCoreShields();
        await Task.CompletedTask;
    }
    
    private void LoadCoreArmor()
    {
        // Unarmored
        _armor["unarmored"] = new PfArmor
        {
            Name = "Unarmored",
            Description = "You're not wearing armor.",
            Type = PfItemType.Armor,
            Level = 0,
            Price = "0 gp",
            Bulk = "0",
            Category = ArmorCategory.Unarmored,
            ArmorClass = 0,
            DexCap = 10,
            CheckPenalty = 0,
            SpeedPenalty = 0,
            Strength = 0,
            Group = "Unarmored",
            Traits = new List<string>(),
            ArmorTraits = new List<string>(),
            Source = "Core Rulebook"
        };
        
        // Light Armor
        _armor["leather"] = new PfArmor
        {
            Name = "Leather Armor",
            Description = "A mix of flexible and molded boiled leather, this armor provides some protection with maximum flexibility.",
            Type = PfItemType.Armor,
            Level = 0,
            Price = "2 gp",
            Bulk = "1",
            Category = ArmorCategory.Light,
            ArmorClass = 1,
            DexCap = 4,
            CheckPenalty = -1,
            SpeedPenalty = 0,
            Strength = 10,
            Group = "Leather",
            Traits = new List<string> { "light", "armor" },
            ArmorTraits = new List<string>(),
            Source = "Core Rulebook"
        };
        
        _armor["studded_leather"] = new PfArmor
        {
            Name = "Studded Leather Armor",
            Description = "This leather armor is reinforced with metal studs and sometimes small metal plates, providing additional protection.",
            Type = PfItemType.Armor,
            Level = 0,
            Price = "3 gp",
            Bulk = "1",
            Category = ArmorCategory.Light,
            ArmorClass = 2,
            DexCap = 3,
            CheckPenalty = -1,
            SpeedPenalty = 0,
            Strength = 12,
            Group = "Leather",
            Traits = new List<string> { "light", "armor" },
            ArmorTraits = new List<string>(),
            Source = "Core Rulebook"
        };
        
        // Medium Armor
        _armor["chain_shirt"] = new PfArmor
        {
            Name = "Chain Shirt",
            Description = "Sometimes called a hauberk, this is a long shirt constructed of interlocking metal rings.",
            Type = PfItemType.Armor,
            Level = 0,
            Price = "5 gp",
            Bulk = "1",
            Category = ArmorCategory.Medium,
            ArmorClass = 3,
            DexCap = 2,
            CheckPenalty = -1,
            SpeedPenalty = -5,
            Strength = 12,
            Group = "Chain",
            Traits = new List<string> { "medium", "armor" },
            ArmorTraits = new List<string> { "flexible", "noisy" },
            Source = "Core Rulebook"
        };
        
        _armor["scale_mail"] = new PfArmor
        {
            Name = "Scale Mail",
            Description = "Scale mail consists of many metal scales sewn onto a reinforcing backing of leather, making it flexible but somewhat heavy.",
            Type = PfItemType.Armor,
            Level = 0,
            Price = "4 gp",
            Bulk = "2",
            Category = ArmorCategory.Medium,
            ArmorClass = 3,
            DexCap = 2,
            CheckPenalty = -2,
            SpeedPenalty = -5,
            Strength = 14,
            Group = "Composite",
            Traits = new List<string> { "medium", "armor" },
            ArmorTraits = new List<string>(),
            Source = "Core Rulebook"
        };
        
        // Heavy Armor
        _armor["chain_mail"] = new PfArmor
        {
            Name = "Chain Mail",
            Description = "A suit of chain mail consists of interlocking metal rings, including a layer of padding underneath.",
            Type = PfItemType.Armor,
            Level = 0,
            Price = "6 gp",
            Bulk = "2",
            Category = ArmorCategory.Heavy,
            ArmorClass = 4,
            DexCap = 1,
            CheckPenalty = -2,
            SpeedPenalty = -5,
            Strength = 14,
            Group = "Chain",
            Traits = new List<string> { "heavy", "armor" },
            ArmorTraits = new List<string> { "flexible", "noisy" },
            Source = "Core Rulebook"
        };
        
        _armor["plate"] = new PfArmor
        {
            Name = "Plate Armor",
            Description = "The classic heavily armored suit, plate armor consists of large metal plates over vital areas with chainmail or leather protection over joints and gaps.",
            Type = PfItemType.Armor,
            Level = 2,
            Price = "30 gp",
            Bulk = "3",
            Category = ArmorCategory.Heavy,
            ArmorClass = 6,
            DexCap = 0,
            CheckPenalty = -3,
            SpeedPenalty = -10,
            Strength = 16,
            Group = "Plate",
            Traits = new List<string> { "heavy", "armor" },
            ArmorTraits = new List<string> { "bulwark" },
            Source = "Core Rulebook"
        };
    }
    
    private void LoadCoreShields()
    {
        _shields["buckler"] = new PfShield
        {
            Name = "Buckler",
            Description = "This very small shield is a favorite of duelists and quick, lightly armored warriors. It's typically made of steel and strapped to your forearm.",
            Type = PfItemType.Shield,
            Level = 0,
            Price = "1 gp",
            Bulk = "L",
            ArmorClass = 1,
            HardnessPoints = 3,
            BrokenThreshold = 6,
            HitPoints = 6,
            Traits = new List<string> { "shield" },
            Source = "Core Rulebook"
        };
        
        _shields["wooden_shield"] = new PfShield
        {
            Name = "Wooden Shield",
            Description = "Though they come in a variety of shapes and sizes, the protection offered by wooden shields comes from the stoutness of their materials.",
            Type = PfItemType.Shield,
            Level = 0,
            Price = "1 gp",
            Bulk = "1",
            ArmorClass = 2,
            HardnessPoints = 3,
            BrokenThreshold = 8,
            HitPoints = 12,
            Traits = new List<string> { "shield" },
            Source = "Core Rulebook"
        };
        
        _shields["steel_shield"] = new PfShield
        {
            Name = "Steel Shield",
            Description = "Like wooden shields, steel shields come in a variety of shapes and sizes. Though more expensive than wooden shields, they are much more durable.",
            Type = PfItemType.Shield,
            Level = 0,
            Price = "2 gp",
            Bulk = "1",
            ArmorClass = 2,
            HardnessPoints = 5,
            BrokenThreshold = 10,
            HitPoints = 20,
            Traits = new List<string> { "shield" },
            Source = "Core Rulebook"
        };
    }
    
    private async Task LoadEquipmentAsync()
    {
        LoadCoreEquipment();
        await Task.CompletedTask;
    }
    
    private void LoadCoreEquipment()
    {
        // Adventuring Gear
        _equipment["backpack"] = new PfEquipment
        {
            Name = "Backpack",
            Description = "A backpack holds up to 4 Bulk of items. If you're carrying or stowing the pack rather than wearing it on your back, its Bulk is light instead of negligible.",
            Type = PfItemType.Gear,
            Level = 0,
            Price = "1 sp",
            Bulk = "—",
            Category = EquipmentCategory.AdventuringGear,
            Traits = new List<string>(),
            Usage = "worn on back",
            Source = "Core Rulebook"
        };
        
        _equipment["rope"] = new PfEquipment
        {
            Name = "Rope",
            Description = "Rope has 2 Hit Points per inch of thickness and Hardness 2.",
            Type = PfItemType.Gear,
            Level = 0,
            Price = "2 sp",
            Bulk = "L",
            Category = EquipmentCategory.AdventuringGear,
            Traits = new List<string>(),
            Usage = "held in hands",
            Source = "Core Rulebook"
        };
        
        _equipment["torch"] = new PfEquipment
        {
            Name = "Torch",
            Description = "A torch sheds bright light in a 20-foot radius (and dim light to the next 20 feet) for 6 hours.",
            Type = PfItemType.Gear,
            Level = 0,
            Price = "1 cp",
            Bulk = "L",
            Category = EquipmentCategory.AdventuringGear,
            Traits = new List<string>(),
            Usage = "held in 1 hand",
            Source = "Core Rulebook"
        };
        
        // Tools
        _equipment["thieves_tools"] = new PfEquipment
        {
            Name = "Thieves' Tools",
            Description = "You need thieves' tools to Pick Locks or Disable a Device (of the mechanical variety) that has a very simple lock.",
            Type = PfItemType.Tool,
            Level = 0,
            Price = "3 gp",
            Bulk = "L",
            Category = EquipmentCategory.Held,
            Traits = new List<string>(),
            Usage = "held in 2 hands",
            Source = "Core Rulebook"
        };
        
        _equipment["healers_tools"] = new PfEquipment
        {
            Name = "Healer's Tools",
            Description = "This kit of bandages, herbs, and suturing tools is necessary for Medicine checks to Administer First Aid, Treat Disease, Treat Poison, or Treat Wounds.",
            Type = PfItemType.Tool,
            Level = 0,
            Price = "5 gp",
            Bulk = "1",
            Category = EquipmentCategory.Held,
            Traits = new List<string>(),
            Usage = "held in 2 hands",
            Source = "Core Rulebook"
        };
    }
    
    private async Task LoadAfflictionsAsync()
    {
        LoadCoreAfflictions();
        await Task.CompletedTask;
    }

    private async Task LoadMonstersAsync()
    {
        LoadCoreMonsters();
        await Task.CompletedTask;
    }
    
    private void LoadCoreAfflictions()
    {
        // Load comprehensive affliction data from PathfinderGameData
        foreach (var (key, affliction) in PathfinderGameData.Afflictions.CoreAfflictionsData)
        {
            _afflictions[key] = affliction;
        }
        foreach (var (key, condition) in PathfinderGameData.Conditions.CoreConditionsData)
        {
            _conditions[key] = condition;
        }
    }
    private void LoadCoreMonsters()
    {
        // Goblin Warrior - CR -1 
        _monsters["goblin-warrior"] = new PfMonster
        {
            Id = "goblin-warrior",
            Name = "Goblin Warrior",
            Description = "The frontline fighters of goblin tribes prefer to fight in large groups—especially when they can outnumber their foes at least three to one.",
            Level = -1,
            Traits = new List<string> { "CE", "Small", "Goblin", "Humanoid" },
            Size = "Small",
            Alignment = "CE",
            ArmorClass = 16,
            ArmorClassNotes = new List<string>(),
            HitPoints = 6,
            HitPointsFormula = "1d6",
            Immunities = new List<string>(),
            Resistances = new List<string>(),
            Weaknesses = new List<string>(),
            FortitudeSave = 2,
            ReflexSave = 8,
            WillSave = 1,
            SaveNotes = new List<string>(),
            Perception = 5,
            PerceptionNotes = new List<string> { "darkvision" },
            Skills = new Dictionary<string, int>
            {
                ["Acrobatics"] = 6,
                ["Athletics"] = 2,
                ["Nature"] = 1,
                ["Stealth"] = 6
            },
            Languages = new List<string> { "Goblin" },
            Senses = new List<string> { "darkvision" },
            Speeds = new Dictionary<string, int> { ["land"] = 25 },
            SpeedNotes = new List<string>(),
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 10,
                ["Dexterity"] = 18,
                ["Constitution"] = 12,
                ["Intelligence"] = 10,
                ["Wisdom"] = 12,
                ["Charisma"] = 8
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Dogslicer",
                    AttackBonus = 6,
                    Traits = new List<string> { "agile", "backstabber", "finesse" },
                    DamageFormula = "1d6",
                    DamageType = "slashing",
                    Range = "melee",
                    Effects = new List<string>(),
                    Notes = "Agile, backstabber, finesse"
                },
                new()
                {
                    Name = "Shortbow",
                    AttackBonus = 6,
                    Traits = new List<string> { "deadly d10" },
                    DamageFormula = "1d6",
                    DamageType = "piercing",
                    Range = "ranged",
                    RangeIncrement = 60,
                    MaxRange = 600,
                    Effects = new List<string>(),
                    Notes = "Deadly d10, range increment 60 feet, reload 0"
                }
            },
            Actions = new List<PfMonsterAction>(),
            Reactions = new List<PfMonsterAction>(),
            PassiveAbilities = new List<PfMonsterAction>(),
            Spellcasting = new List<PfMonsterSpellcasting>(),
            Items = new List<string> { "leather armor", "dogslicer", "shortbow", "10 arrows" },
            Source = "Bestiary"
        };
        
        // Orc Warrior - CR 0
        _monsters["orc-warrior"] = new PfMonster
        {
            Id = "orc-warrior",
            Name = "Orc Warrior",
            Description = "The typical orc warrior is a violent brute who serves as a foot soldier in an orc war band.",
            Level = 0,
            Traits = new List<string> { "CE", "Medium", "Humanoid", "Orc" },
            Size = "Medium",
            Alignment = "CE",
            ArmorClass = 16,
            HitPoints = 15,
            HitPointsFormula = "2d8+2",
            FortitudeSave = 6,
            ReflexSave = 2,
            WillSave = 1,
            Perception = 5,
            PerceptionNotes = new List<string> { "darkvision" },
            Skills = new Dictionary<string, int>
            {
                ["Athletics"] = 7,
                ["Intimidation"] = 4
            },
            Languages = new List<string> { "Orcish" },
            Senses = new List<string> { "darkvision" },
            Speeds = new Dictionary<string, int> { ["land"] = 25 },
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 16,
                ["Dexterity"] = 10,
                ["Constitution"] = 13,
                ["Intelligence"] = 8,
                ["Wisdom"] = 11,
                ["Charisma"] = 10
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Falchion",
                    AttackBonus = 7,
                    Traits = new List<string> { "forceful", "sweep" },
                    DamageFormula = "1d10+3",
                    DamageType = "slashing",
                    Range = "melee"
                },
                new()
                {
                    Name = "Javelin",
                    AttackBonus = 3,
                    Traits = new List<string> { "thrown 30 ft." },
                    DamageFormula = "1d6+3",
                    DamageType = "piercing",
                    Range = "melee/ranged",
                    RangeIncrement = 30
                }
            },
            Actions = new List<PfMonsterAction>
            {
                new()
                {
                    Name = "Ferocity",
                    Description = "When the orc warrior is reduced to 0 Hit Points, it remains conscious and can continue acting normally until the end of its next turn, but it is slowed 1. At the end of its turn, if it hasn't taken the Recover action, it dies. If it takes enough damage to die instantly, it dies and doesn't remain conscious.",
                    ActionType = ActionType.Reaction,
                    Traits = new List<string>(),
                    Trigger = "The orc warrior is reduced to 0 Hit Points."
                }
            },
            Items = new List<string> { "chain mail", "falchion", "javelin (6)" },
            Source = "Bestiary"
        };
        
        // Giant Rat - CR -1
        _monsters["giant-rat"] = new PfMonster
        {
            Id = "giant-rat",
            Name = "Giant Rat",
            Description = "Giant rats are enormous versions of the common vermin. They're typically found in dungeons and sewers where food is plentiful. Although they'll avoid confrontation with larger creatures when possible, giant rats are fierce when cornered.",
            Level = -1,
            Traits = new List<string> { "N", "Small", "Animal" },
            Size = "Small",
            Alignment = "N",
            ArmorClass = 15,
            HitPoints = 8,
            HitPointsFormula = "1d8+1",
            FortitudeSave = 3,
            ReflexSave = 7,
            WillSave = 1,
            Perception = 3,
            PerceptionNotes = new List<string> { "low-light vision", "scent (imprecise) 30 feet" },
            Skills = new Dictionary<string, int>
            {
                ["Acrobatics"] = 5,
                ["Athletics"] = 2,
                ["Stealth"] = 5
            },
            Languages = new List<string>(),
            Senses = new List<string> { "low-light vision", "scent (imprecise) 30 feet" },
            Speeds = new Dictionary<string, int> { ["land"] = 30, ["climb"] = 10 },
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 10,
                ["Dexterity"] = 17,
                ["Constitution"] = 12,
                ["Intelligence"] = -4,
                ["Wisdom"] = 12,
                ["Charisma"] = 4
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Jaws",
                    AttackBonus = 6,
                    Traits = new List<string> { "finesse" },
                    DamageFormula = "1d6+1",
                    DamageType = "piercing",
                    Range = "melee",
                    AdditionalDamage = new List<PfStrikeDamage>
                    {
                        new()
                        {
                            Formula = "1",
                            DamageType = "piercing",
                            Condition = "persistent",
                            IsPersistent = true
                        }
                    }
                }
            },
            Source = "Bestiary"
        };
    }

    // Monster repository methods
    public async Task<Result<IEnumerable<PfMonster>>> GetMonstersAsync()
    {
        await InitializeAsync();
        return Result<IEnumerable<PfMonster>>.Success(_monsters.Values.AsEnumerable());
    }

    public async Task<Result<PfMonster>> GetMonsterByIdAsync(string id)
    {
        await InitializeAsync();
        
        if (_monsters.TryGetValue(id, out var monster))
        {
            return Result<PfMonster>.Success(monster);
        }
        
        return Result.Failure<PfMonster>(GeneralErrors.NotFound);
    }

    public async Task<Result<IEnumerable<PfMonster>>> SearchMonstersAsync(string searchTerm, int? level = null)
    {
        await InitializeAsync();
        
        var monsters = _monsters.Values.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            monsters = monsters.Where(m => 
                m.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                m.Traits.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }
        
        if (level.HasValue)
        {
            monsters = monsters.Where(m => m.Level == level.Value);
        }
        
        return Result<IEnumerable<PfMonster>>.Success(monsters);
    }
}