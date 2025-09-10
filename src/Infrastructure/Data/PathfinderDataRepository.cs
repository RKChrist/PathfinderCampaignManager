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
        // Load all core classes with full implementations
        _classes["alchemist"] = CreateAlchemistClass();
        _classes["animist"] = CreateAnimistClass();
        _classes["barbarian"] = CreateBarbarianClass();
        _classes["bard"] = CreateBardClass();
        _classes["champion"] = CreateChampionClass();
        _classes["cleric"] = CreateClericClass();
        _classes["commander"] = CreateCommanderClass();
        _classes["druid"] = CreateDruidClass();
        _classes["exemplar"] = CreateExemplarClass();
        _classes["fighter"] = CreateFighterClass();
        _classes["guardian"] = CreateGuardianClass();
        _classes["gunslinger"] = CreateGunslingerClass();
        _classes["investigator"] = CreateInvestigatorClass();
        _classes["inventor"] = CreateInventorClass();
        _classes["kineticist"] = CreateKineticistClass();
        _classes["magus"] = CreateMagusClass();
        _classes["monk"] = CreateMonkClass();
        _classes["oracle"] = CreateOracleClass();
        _classes["psychic"] = CreatePsychicClass();
        _classes["ranger"] = CreateRangerClass();
        _classes["rogue"] = CreateRogueClass();
        _classes["sorcerer"] = CreateSorcererClass();
        _classes["summoner"] = CreateSummonerClass();
        _classes["swashbuckler"] = CreateSwashbucklerClass();
        _classes["thaumaturge"] = CreateThaumaturgeClass();
        _classes["witch"] = CreateWitchClass();
        _classes["wizard"] = CreateWizardClass();
        
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
    
    private PfClass CreateAlchemistClass()
    {
        return new PfClass
        {
            Id = "alchemist",
            Name = "Alchemist",
            Description = "You enjoy tinkering with alchemical formulas and substances in your spare time, and your studies have progressed beyond mere experimentation.",
            KeyAbilities = new List<string> { "Intelligence" },
            HitPoints = 8,
            SkillRanks = 3,
            Source = "Player Core 2",
            Rarity = "Common",
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            FortitudeProgression = new Progression<SaveProgression>("alchemist_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("alchemist_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("alchemist_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("alchemist_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(7, Proficiency.Expert),
                new(15, Proficiency.Master)
            }),
            
            ClassDcProgression = new Progression<SpellcastingProgression>("alchemist_class_dc", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master),
                new(19, Proficiency.Legendary)
            })
        };
    }
    
    private PfClass CreateBarbarianClass()
    {
        return new PfClass
        {
            Id = "barbarian",
            Name = "Barbarian",
            Description = "You have learned to harness and control your anger, gaining the ability to enter a rage-like state to augment your combat prowess.",
            KeyAbilities = new List<string> { "Strength" },
            HitPoints = 12,
            SkillRanks = 3,
            Source = "Player Core 2",
            Rarity = "Common",
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            FortitudeProgression = new Progression<SaveProgression>("barbarian_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("barbarian_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("barbarian_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("barbarian_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            })
        };
    }
    
    private PfClass CreateBardClass()
    {
        return new PfClass
        {
            Id = "bard",
            Name = "Bard",
            Description = "A muse has called you to dabble in occult lore, allowing you to cast a few spells. The deeper you delve, the more powerful your performances become.",
            KeyAbilities = new List<string> { "Charisma" },
            HitPoints = 8,
            SkillRanks = 4,
            Source = "Player Core",
            Rarity = "Common",
            Traits = new List<string>(),
            
            // Spellcasting info
            IsSpellcaster = true,
            SpellcastingTradition = "Occult",
            SpellcastingAbility = "Charisma",
            IsSpontaneousCaster = true,
            IsPreparedCaster = false,
            
            // Class feat levels
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            FortitudeProgression = new Progression<SaveProgression>("bard_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("bard_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("bard_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("bard_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            // Class features by level (minimal for compilation)
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>()
        };
    }
    
    private PfClass CreateWizardClass()
    {
        return new PfClass
        {
            Id = "wizard",
            Name = "Wizard",
            Description = "You are an eternal student of the arcane secrets of the universe, using your mastery of magic to cast powerful and devastating spells.",
            KeyAbilities = new List<string> { "Intelligence" },
            HitPoints = 6,
            SkillRanks = 2,
            Source = "Player Core",
            Rarity = "Common",
            Traits = new List<string>(),
            
            // Spellcasting info
            IsSpellcaster = true,
            SpellcastingTradition = "Arcane",
            SpellcastingAbility = "Intelligence",
            IsPreparedCaster = true,
            IsSpontaneousCaster = false,
            
            // Class feat levels
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            FortitudeProgression = new Progression<SaveProgression>("wizard_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("wizard_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("wizard_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("wizard_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(11, Proficiency.Expert),
                new(19, Proficiency.Master)
            }),
            
            // Class features by level (minimal for compilation)
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>()
        };
    }
    
    private PfClass CreateChampionClass()
    {
        return new PfClass
        {
            Id = "champion",
            Name = "Champion",
            Description = "You have sworn a solemn oath to your deity, gaining divine power to protect the innocent and fight for justice and righteousness.",
            KeyAbilities = new List<string> { "Strength", "Dexterity" },
            HitPoints = 10,
            SkillRanks = 2,
            Source = "Player Core 2",
            Rarity = "Common",
            Traits = new List<string>(),
            
            // Spellcasting info - limited focus spells
            IsSpellcaster = true,
            SpellcastingTradition = "Divine",
            SpellcastingAbility = "Charisma",
            IsPreparedCaster = false,
            IsSpontaneousCaster = false,
            
            // Class feat levels
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            FortitudeProgression = new Progression<SaveProgression>("champion_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("champion_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("champion_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("champion_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(11, Proficiency.Expert),
                new(19, Proficiency.Master)
            }),
            
            WeaponProgressions = new List<Progression<WeaponProgression>>
            {
                new("champion_simple_weapons", new List<ProgressionStep>
                {
                    new(1, Proficiency.Trained),
                    new(5, Proficiency.Expert),
                    new(13, Proficiency.Master)
                }),
                new("champion_martial_weapons", new List<ProgressionStep>
                {
                    new(1, Proficiency.Trained),
                    new(5, Proficiency.Expert),
                    new(13, Proficiency.Master)
                })
            },
            
            ArmorProgressions = new List<Progression<ArmorProgression>>
            {
                new("champion_all_armor", new List<ProgressionStep>
                {
                    new(1, Proficiency.Trained),
                    new(13, Proficiency.Expert),
                    new(17, Proficiency.Master)
                })
            },
            
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new() {
                        Id = "champion_deity",
                        Name = "Deity and Cause",
                        Description = "Champions are divine servants of a deity. Choose a deity to serve; your alignment must be one allowed by your deity. Your cause must match your alignment exactly.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "champion" },
                        Source = "Player Core 2"
                    },
                    new() {
                        Id = "champion_devotion_spells",
                        Name = "Devotion Spells",
                        Description = "Your deity's power grants you special devotion spells called focus spells. You start with a focus pool of 1 Focus Point.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "champion" },
                        Source = "Player Core 2"
                    },
                    new() {
                        Id = "champion_aura",
                        Name = "Champion's Aura",
                        Description = "Your champion's aura is a 15-foot emanation that has a special effect based on your cause.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "champion", "aura" },
                        Source = "Player Core 2"
                    },
                    new() {
                        Id = "shield_block",
                        Name = "Shield Block",
                        Description = "You gain the Shield Block general feat.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "champion" },
                        Source = "Player Core 2"
                    }
                }
            }
        };
    }
    
    private PfClass CreateClericClass()
    {
        return new PfClass
        {
            Id = "cleric",
            Name = "Cleric",
            Description = "Acting as an intermediary between the realm of the holy and the mortal world, you serve as a conduit for divine magic.",
            KeyAbilities = new List<string> { "Wisdom" },
            HitPoints = 8,
            SkillRanks = 2,
            Source = "Player Core",
            Rarity = "Common",
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            SpellcastingTradition = "Divine",
            IsPreparedCaster = true,
            IsSpontaneousCaster = false,
            
            FortitudeProgression = new Progression<SaveProgression>("cleric_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("cleric_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("cleric_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("cleric_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(11, Proficiency.Expert),
                new(19, Proficiency.Master)
            }),
            
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new() {
                        Id = "cleric_spellcasting",
                        Name = "Divine Spellcasting",
                        Description = "You can cast divine spells using the Cast a Spell activity, and you can supply material, somatic, and verbal components when casting spells.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "cleric" },
                        Source = "Player Core"
                    },
                    new() {
                        Id = "divine_font",
                        Name = "Divine Font",
                        Description = "Through your deity's blessing, you gain additional spell slots specifically for heal or harm spells.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "cleric" },
                        Source = "Player Core"
                    },
                    new() {
                        Id = "doctrine",
                        Name = "Doctrine",
                        Description = "Even among followers of the same deity, there are numerous doctrines and beliefs that shape how you practice your religion.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "cleric" },
                        Source = "Player Core"
                    }
                }
            }
        };
    }
    
    private PfClass CreateRogueClass()
    {
        return new PfClass
        {
            Id = "rogue",
            Name = "Rogue",
            Description = "You are skilled and opportunistic. Using your sharp wits and quick reactions, you take advantage of your opponents' missteps and strike where it hurts most.",
            KeyAbilities = new List<string> { "Dexterity" },
            HitPoints = 8,
            SkillRanks = 7,
            Source = "Player Core",
            Rarity = "Common",
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            FortitudeProgression = new Progression<SaveProgression>("rogue_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("rogue_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("rogue_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("rogue_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new() {
                        Id = "sneak_attack",
                        Name = "Sneak Attack",
                        Description = "When your enemy can't properly defend itself, you can deal devastating damage with a precise strike. You deal an extra 1d6 precision damage when you Strike a creature that's off-guard with an agile or finesse melee weapon, unarmed attack, or ranged weapon attack.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "rogue" },
                        Source = "Player Core"
                    },
                    new() {
                        Id = "surprise_attack",
                        Name = "Surprise Attack",
                        Description = "You spring into combat faster than foes can react. On the first round of combat, if you roll Deception or Stealth for initiative, creatures that haven't acted are off-guard to you.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "rogue" },
                        Source = "Player Core"
                    },
                    new() {
                        Id = "rogues_racket",
                        Name = "Rogue's Racket",
                        Description = "As you started on the path of the rogue, you began to develop your own specialized area of expertise.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "rogue" },
                        Source = "Player Core"
                    }
                }
            }
        };
    }
    
    private PfClass CreateRangerClass()
    {
        return new PfClass
        {
            Id = "ranger",
            Name = "Ranger",
            Description = "Some rangers believe civilization wears down the soul, but still needs to be protected from wild creatures. Others say nature needs to be protected from the greedy, who wish to tame its beauty and plunder its treasures.",
            KeyAbilities = new List<string> { "Strength", "Dexterity" },
            HitPoints = 10,
            SkillRanks = 4,
            Source = "Player Core",
            Rarity = "Common",
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            SpellcastingTradition = "Primal",
            IsSpellcaster = true,
            
            FortitudeProgression = new Progression<SaveProgression>("ranger_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("ranger_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("ranger_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("ranger_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new() {
                        Id = "hunt_prey",
                        Name = "Hunt Prey",
                        Description = "When you focus your attention on a single foe, you become very effective at hunting that target. Use the Hunt Prey action to designate a single creature you can see and hear, or one you're Tracking, as your prey.",
                        Level = 1,
                        Type = "Class Feature",
                        ActionCost = "1",
                        Traits = new List<string> { "ranger", "concentrate" },
                        Source = "Player Core"
                    },
                    new() {
                        Id = "hunters_edge",
                        Name = "Hunter's Edge",
                        Description = "You have trained for countless hours to become a more skilled hunter and tracker, gaining an edge when pursuing your prey.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "ranger" },
                        Source = "Player Core"
                    }
                }
            }
        };
    }
    
    private PfClass CreateMonkClass()
    {
        return new PfClass
        {
            Id = "monk",
            Name = "Monk",
            Description = "The strength of your fist flows from your mind and spirit. You seek perfection—honing your body into a flawless instrument and your mind into an orderly bastion of wisdom.",
            KeyAbilities = new List<string> { "Strength", "Dexterity" },
            HitPoints = 10,
            SkillRanks = 4,
            Source = "Player Core 2",
            Rarity = "Common",
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            SpellcastingTradition = "Divine",
            IsSpellcaster = true,
            
            FortitudeProgression = new Progression<SaveProgression>("monk_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(17, Proficiency.Master)
            }),
            
            ReflexProgression = new Progression<SaveProgression>("monk_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            WillProgression = new Progression<SaveProgression>("monk_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Expert),
                new(13, Proficiency.Master)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>("monk_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(11, Proficiency.Expert),
                new(19, Proficiency.Master)
            }),
            
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new() {
                        Id = "flurry_of_blows",
                        Name = "Flurry of Blows",
                        Description = "You can attack rapidly with fists, feet, elbows, knees, and other unarmed attacks. Make two unarmed Strikes. If both hit the same creature, combine their damage for the purpose of resistances and weaknesses.",
                        Level = 1,
                        Type = "Class Feature",
                        ActionCost = "1",
                        Traits = new List<string> { "monk", "flourish" },
                        Source = "Player Core 2"
                    },
                    new() {
                        Id = "qi_spells",
                        Name = "Qi Spells",
                        Description = "A qi spell is a type of focus spell that works by manipulating your internal energy. You learn a 1st-level qi spell based on your ki tradition, and you have a focus pool with 1 Focus Point.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "monk" },
                        Source = "Player Core 2"
                    }
                }
            }
        };
    }
    
    // Remaining classes with basic implementations
    private PfClass CreateAnimistClass()
    {
        return new PfClass
        {
            Id = "animist",
            Name = "Animist",
            Description = "The world is filled with spirits—those of ancestors, of items long used by mortals, of animals, and of the land itself. You can perceive these spirits and gain magical power by serving as an intermediary between the spirit world and the mortal world. Through a kinship with spirits, you can draw upon their collective knowledge and power.",
            KeyAbilities = new List<string> { "Wisdom" },
            HitPoints = 8,
            SkillRanks = 3,
            Source = "War of Immortals",
            Rarity = "Common",
            Traits = new List<string> { "animist" },
            
            // Spellcasting info
            IsSpellcaster = true,
            SpellcastingTradition = "Divine",
            SpellcastingAbility = "Wisdom",
            IsPreparedCaster = true,
            IsSpontaneousCaster = false,
            
            // Class feat levels
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            // Class features by level
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Id = "animist_spellcasting",
                        Name = "Animist Spellcasting",
                        Description = "Your connection to the spirit world grants you the ability to cast spells. You can cast divine spells using the Cast a Spell activity, and you can supply material, somatic, and verbal components when casting spells.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "animist" },
                        Source = "War of Immortals"
                    },
                    new PfClassFeature
                    {
                        Id = "spirit_dwelling",
                        Name = "Spirit Dwelling",
                        Description = "A powerful spirit has taken up residence within you, granting you power but also exerting some influence over you. You choose a spirit to dwell within you from one of several different spiritual essences, each of which provides different benefits and restrictions.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "animist" },
                        Source = "War of Immortals"
                    },
                    new PfClassFeature
                    {
                        Id = "vessel_spells",
                        Name = "Vessel Spells",
                        Description = "Your spirit dwelling grants you special spells called vessel spells. Each day when you make your daily preparations, you can substitute any one spell you would normally be able to prepare with a vessel spell of the same rank.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "animist" },
                        Source = "War of Immortals"
                    },
                    new PfClassFeature
                    {
                        Id = "commune_with_spirits",
                        Name = "Commune with Spirits",
                        Description = "You can commune with spirits around you to gain insights and aid. You can use the Recall Knowledge action to learn about any topic, not just those you're trained in, though you still can't use it to learn about topics forbidden to your spirit.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "animist" },
                        Source = "War of Immortals"
                    }
                }
            }
        };
    }
    private PfClass CreateCommanderClass()
    {
        return new PfClass
        {
            Id = "commander",
            Name = "Commander",
            Description = "You are a master of warfare and tactics, leading from the front and inspiring your allies to greatness. Whether you're a knight leading a charge, a general coordinating troops, or a rebel organizing resistance, you understand that victory comes through teamwork, strategy, and decisive action.",
            KeyAbilities = new List<string> { "Intelligence", "Charisma" },
            HitPoints = 8,
            SkillRanks = 4,
            Source = "Battlecry!",
            Rarity = "Common",
            Traits = new List<string> { "commander" },
            
            // Non-spellcaster
            IsSpellcaster = false,
            
            // Class feat levels
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            // Class features by level
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Id = "warfare_lore",
                        Name = "Warfare Lore",
                        Description = "You have studied the art of war and tactics extensively. You're trained in Warfare Lore, a special Lore skill that can be used to Recall Knowledge about military tactics, famous battles, siege techniques, and other topics related to organized combat.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "commander" },
                        Source = "Battlecry!"
                    },
                    new PfClassFeature
                    {
                        Id = "tactical_cadence",
                        Name = "Tactical Cadence",
                        Description = "You can guide your allies' actions through tactical commands and battle experience. You gain a number of tactical actions that can be used to direct your allies in combat, providing them with bonuses and special maneuvers.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "commander" },
                        Source = "Battlecry!"
                    },
                    new PfClassFeature
                    {
                        Id = "banner",
                        Name = "Banner",
                        Description = "You carry a banner, standard, or other symbol that represents your cause and rallies your allies. While your banner is displayed, allies within 30 feet gain a +1 circumstance bonus to saves against fear effects.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "commander" },
                        Source = "Battlecry!"
                    },
                    new PfClassFeature
                    {
                        Id = "formation_training",
                        Name = "Formation Training",
                        Description = "You and your allies have trained to fight as a cohesive unit. When you and at least one ally are adjacent to an enemy, you both gain a +1 circumstance bonus to attack rolls against that enemy.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "commander" },
                        Source = "Battlecry!"
                    }
                }
            }
        };
    }
    private PfClass CreateDruidClass()
    {
        return new PfClass
        {
            Id = "druid",
            Name = "Druid",
            Description = "The power of nature is impossible to resist. It can bring ruin to the stoutest fortress in minutes, reducing even the mightiest works to rubble, burning them to ash, burying them beneath an avalanche of snow, or drowning them beneath the waves. It can provide endless bounty and breathtaking splendor to those who respect it—and an agonizing death to those who take it too lightly. You are one of those who hear nature's call. You stand in reverence of the majesty of its power and give yourself over to its service.",
            KeyAbilities = new List<string> { "Wisdom" },
            HitPoints = 8,
            SkillRanks = 2,
            SpellcastingTradition = "Primal",
            SpellcastingAbility = "Wisdom",
            IsSpellcaster = true,
            IsPreparedCaster = true,
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Druidcraft",
                        Description = "You know druidcraft, a primal cantrip.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Primal Spellcasting",
                        Description = "You can cast primal spells using the Cast a Spell activity, and you can supply material, somatic, and verbal components when casting spells. At 1st level, you can prepare two 1st-rank spells and five cantrips each morning from the common spells on the primal spell list or from other primal spells to which you have access. Prepared spells remain available to you until you cast them or until you prepare your spells again.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Anathema",
                        Description = "As stewards of the natural order, druids find themselves bound by nature itself to avoid actions that would go against nature. The specifics of what acts are anathema vary by druid, but all include despoiling natural places, killing animals unnecessarily, and using fire or metal armor and weapons.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Druidic Language",
                        Description = "You know Druidic, a secret language known to only druids, in addition to any languages you know through your ancestry. Druidic has its own alphabet.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Shield Block",
                        Description = "You gain the Shield Block general feat.",
                        Level = 1
                    }
                }
            },
            Traits = new List<string> { "druid" },
            Source = "Player Core",
            Rarity = "common"
        };
    }
    private PfClass CreateGuardianClass()
    {
        return new PfClass
        {
            Id = "guardian",
            Name = "Guardian",
            Description = "Some fight for glory, others for coin, but you fight to protect. Whether you stand as a stalwart shield between enemies and your allies or serve as the master of the battlefield, controlling every engagement, your true strength lies not in your weapon, but in your unyielding determination to keep others safe.",
            KeyAbilities = new List<string> { "Strength", "Constitution" },
            HitPoints = 12,
            SkillRanks = 2,
            SpellcastingTradition = "None",
            IsSpellcaster = false,
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Taunt",
                        Description = "You can draw your enemies' attacks to yourself to protect your allies. You gain the Taunt reaction, which allows you to force an enemy to attack you instead of an ally.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Guardian's Resolve",
                        Description = "Your determination to protect others grants you incredible resilience. You gain a +1 circumstance bonus to saves when an ally within 30 feet would be reduced to 0 Hit Points.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Guardian's Edge",
                        Description = "Choose one guardian's edge that represents your particular approach to protecting others. This choice affects many of your guardian abilities and determines some of your class feats.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Shield Block",
                        Description = "You gain the Shield Block general feat.",
                        Level = 1
                    }
                }
            },
            Traits = new List<string> { "guardian" },
            Source = "Battlecry!",
            Rarity = "common"
        };
    }
    private PfClass CreateGunslingerClass()
    {
        return new PfClass
        {
            Id = "gunslinger",
            Name = "Gunslinger",
            Description = "While some fear projectile weapons, you savor the searing flash, wild kick, and cloying smoke that accompanies a gunshot, or snap of the cable and telltale thunk of your crossbow just before your bolt finds purchase. Ready to draw a bead on an enemy at every turn, you rely on your reflexes, steady hand, and knowledge of your weapons to riddle your foes with holes.",
            KeyAbilities = new List<string> { "Dexterity" },
            HitPoints = 8,
            SkillRanks = 3,
            SpellcastingTradition = "None",
            IsSpellcaster = false,
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Gunslinger's Way",
                        Description = "All gunslingers have a particular way they follow, a combination of philosophy and combat style that defines both how they fight and how they view their weapon. Your way grants you an initial deed, a unique reload action called a slinger's reload, and access to way-specific deeds.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Initial Deed",
                        Description = "Your way provides you with an initial deed—a special type of action that can have powerful effects in combat. You can use an initial deed any number of times per round, but only once per turn. If your initial deed ever requires a roll, use your class DC.",
                        Level = 1
                    },
                    new PfClassFeature
                    {
                        Name = "Gunslinger Feats",
                        Description = "At 1st level and every even-numbered level, you gain a gunslinger class feat.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Singular Expertise",
                        Description = "You have particular expertise with guns and crossbows that grants you greater proficiency with them and the ability to deal more damage. You gain a +1 circumstance bonus to damage rolls with firearms and crossbows.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "gunslinger" },
            Source = "Guns & Gears",
            Rarity = "common"
        };
    }
    private PfClass CreateInvestigatorClass()
    {
        return new PfClass
        {
            Id = "investigator",
            Name = "Investigator",
            Description = "You live to solve mysteries and find answers to complicated problems. Whether the mystery is a murder, a disappearance, or something more esoteric, such as the strange rumors that surround a cursed ruin, you're driven to get to the truth. You collect evidence, connect dots, and solve cases others can't.",
            KeyAbilities = new List<string> { "Intelligence" },
            HitPoints = 8,
            SkillRanks = 4,
            SpellcastingTradition = "None",
            IsSpellcaster = false,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "On the Case",
                        Description = "When you're pursuing a case, you get a +1 circumstance bonus to checks to investigate, to interact socially with people involved in the case, and to Recall Knowledge about the subject of your case.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Devise a Stratagem",
                        Description = "You can play out a fight in your head, using deduction and combat experience to predict how your enemy will react to your attacks. Choose a creature you can see and roll a d20. If you Strike the chosen creature with the same type of weapon or unarmed attack before the end of your turn, you must use the result of the roll you made when you Devised the Stratagem for your attack roll and damage roll. You don't apply your ability modifier to the damage roll when using Devise a Stratagem, but you do still add it for the purpose of determining whether you meet requirements, such as those of precision damage or striking runes.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Methodology",
                        Description = "Your studies have made you particularly effective with a specific approach to investigating mysteries and solving problems. Choose a methodology that represents how you approach problems when you're On the Case.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Strategic Strike",
                        Description = "When you strike carefully and with forethought, you deal a telling blow. When you make a Strike that adds your Intelligence modifier on the attack roll due to Devise a Stratagem, you deal an additional 1d6 precision damage.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "investigator" },
            Source = "Player Core 2",
            Rarity = "common"
        };
    }
    private PfClass CreateInventorClass()
    {
        return new PfClass
        {
            Id = "inventor",
            Name = "Inventor",
            Description = "Any tinkerer can follow a diagram to make a device, but you invent the impossible! Every strange contraption you dream up is a unique experiment pushing the edge of possibility, a mysterious machine that seems to work for only you. You're always on the verge of the next great breakthrough, and every trial and tribulation is another opportunity to test and tune.",
            KeyAbilities = new List<string> { "Intelligence" },
            HitPoints = 8,
            SkillRanks = 3,
            SpellcastingTradition = "None",
            IsSpellcaster = false,
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Overdrive",
                        Description = "You have a bevy of smaller devices of your own invention, from muscle stimulants to reactionenhancing drugs. When it's necessary, you can throw caution to the wind and spend resources to push your devices beyond their normal limits.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Innovation",
                        Description = "While you're always creating inventions, there's one that represents your preeminent work, the one that you hope to perfect and revolutionize the world. Choose one innovation. Your innovation must be something that can be used in combat.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Peerless Inventor",
                        Description = "You are constantly inventing, and your skill at crafting is unparalleled. You gain the Inventor feat even if you don't meet the prerequisite, and you gain a +1 circumstance bonus to Craft checks. You can Craft in increments as short as 1 minute.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Shield Block",
                        Description = "You gain the Shield Block general feat.",
                        Level = 1
                    }
                }
            },
            Traits = new List<string> { "inventor" },
            Source = "Guns & Gears",
            Rarity = "common"
        };
    }
    private PfClass CreateKineticistClass()
    {
        return new PfClass
        {
            Id = "kineticist",
            Name = "Kineticist",
            Description = "The power of the elements flows through you. A kinetic gate inextricably tied to your body channels power directly from the elemental planes, causing elements to leap to your hand, whirl around your body, and blast foes at your whim. As your connection to the planes grows stronger, you attain true mastery over your chosen elements.",
            KeyAbilities = new List<string> { "Constitution" },
            HitPoints = 8,
            SkillRanks = 3,
            SpellcastingTradition = "None",
            IsSpellcaster = false,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master,
                    ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Kinetic Gate",
                        Description = "As a kineticist, you've awakened or opened a kinetic gate, a supernatural conduit within your body that can channel the pure elemental forces of the Elemental Planes. Choose one element to be your kinetic element, and select one of the kinetic gates for that element.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Kinetic Aura",
                        Description = "Through your kinetic gate, elements flow around you in a tight kinetic aura. You can activate your kinetic aura in a number of ways, called impulses; each element has different impulse options. Each round, you can use a single action to activate impulses of any number and type, though some restrictions apply.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Impulses",
                        Description = "An impulse is a special type of magical action available only to kineticists, with the impulse trait. You can use impulses only if you have a kinetic aura active. You start each encounter with your kinetic aura inactive, but when you use an impulse, you automatically become gathered and can then channel your power into further impulses.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Base Kinesis",
                        Description = "You can project your power onto the world around you without needing to use a more complex impulse. All kineticists can use base kinesis impulses associated with their kinetic elements.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "kineticist" },
            Source = "Rage of Elements",
            Rarity = "common"
        };
    }
    private PfClass CreateMagusClass()
    {
        return new PfClass
        {
            Id = "magus",
            Name = "Magus",
            Description = "Combining the physicality and technique of a warrior with the ability to cast arcane magic, you seek to perfect the art of fusing spell and strike. While the hefty tome you carry reflects hours conducting arcane research, your enemies need no reminder of your training. They recognize it as you take them down.",
            KeyAbilities = new List<string> { "Strength", "Dexterity" },
            HitPoints = 8,
            SkillRanks = 2,
            SpellcastingTradition = "Arcane",
            IsPreparedCaster = true,
            IsSpontaneousCaster = false,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Arcane Spellcasting",
                        Description = "You study spells so you can combine them with your attacks or solve problems that strength of arms alone can't handle. You can cast arcane spells using the Cast a Spell activity, and you can supply material, somatic, and verbal components when casting spells.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Arcane Cascade",
                        Description = "After you cast an arcane spell, or use a magical weapon or shield, magical feedback grants you a +1 status bonus to weapon and unarmed damage rolls until the end of your next turn. The bonus increases to +2 if the spell or item is 3rd rank or higher.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Conflux Spells",
                        Description = "You learn a conflux spell from your hybrid study, and you can cast additional conflux spells by selecting certain feats. Conflux spells are magus-specific spells created for combat and are a type of focus spell.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Hybrid Study",
                        Description = "Your extensive physical training and carefully chosen magic combine to form a unique and dangerous fighting style that's more than the sum of its parts. Choose a hybrid study to represent your particular balance of martial and magical training.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Spellstrike",
                        Description = "You've learned the fundamental magus technique that lets you combine spell and weapon into a single deadly attack. You gain the Spellstrike activity.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "magus" },
            Source = "Secrets of Magic",
            Rarity = "common"
        };
    }
    private PfClass CreateOracleClass()
    {
        return new PfClass
        {
            Id = "oracle",
            Name = "Oracle",
            Description = "The divine mysteries came to you without your choice, and you've been forced to live with the burden ever since. You have a curse, but you also have blessings. You can channel divine power through your mystery, but at a price. Your abilities may stem from a divine revelation, a cursed ancestor, or exposure to raw divine power.",
            KeyAbilities = new List<string> { "Charisma" },
            HitPoints = 8,
            SkillRanks = 2,
            SpellcastingTradition = "Divine",
            IsPreparedCaster = false,
            IsSpontaneousCaster = true,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Curse",
                        Description = "Your mystery's gift comes at a cost. You have a curse related to your mystery that gives you penalties but also grants you benefits when you accept its effects.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Mystery",
                        Description = "An oracle wields divine power drawn from a potent concept or ideal, represented by their mystery. Choose the mystery that empowers your magic.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Oracular Spellcasting",
                        Description = "Your mystery provides you with divine magical power. You can cast spells using the Cast a Spell activity, and you can supply material, somatic, and verbal components when casting spells. At 1st level, you can cast two 1st-rank spells and five cantrips each day from spells in your repertoire.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Spell Repertoire",
                        Description = "The collection of spells you can cast is called your spell repertoire. At 1st level, you learn two 1st-rank spells of your choice and five cantrips of your choice from the divine spell list, or from other divine spells to which you have access.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "oracle" },
            Source = "Player Core 2",
            Rarity = "common"
        };
    }
    private PfClass CreatePsychicClass()
    {
        return new PfClass
        {
            Id = "psychic",
            Name = "Psychic",
            Description = "The mind can perceive truths hidden from fine-tuned instruments, house more secrets than any tome, and move objects and hearts more deftly than any lever. By applying your mind, you can perform wonders beyond the capacities of the physical world. While others rely on swords and sorcery, you have sharpened your mind to a deadly degree, allowing you to perform feats of mental magic.",
            KeyAbilities = new List<string> { "Intelligence", "Charisma" },
            HitPoints = 6,
            SkillRanks = 3,
            SpellcastingTradition = "Occult",
            IsPreparedCaster = false,
            IsSpontaneousCaster = true,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Psychic Spellcasting",
                        Description = "Your mind has opened to occult mysteries. You can cast occult spells using the Cast a Spell activity. At 1st level, you can cast two 1st-rank spells and five cantrips each day. You know these spells and can cast them at will, though you're limited in how many times per day you can do so.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Spell Repertoire",
                        Description = "The collection of spells you can cast is called your spell repertoire. At 1st level, you learn two 1st-rank occult spells of your choice and five occult cantrips of your choice.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Psyche",
                        Description = "Your mind can enter a special state that allows you to channel even more power. You can Unleash your Psyche as a free action. While your Psyche is Unleashed, you get a benefit from your conscious mind, take 1 damage per spell rank for each occult spell you cast, and are drained 1 when the psyche ends.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Conscious Mind",
                        Description = "Your conscious mind represents the way you think—the source of your rational mind, creativity, and will to live. Choose a conscious mind that best represents your character's psychology.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Subconscious Mind",
                        Description = "A psychic's power is born in the depths of their psyche, far from the surface. Your subconscious mind might represent your lower impulses and unconscious biases, or it could be a repository of memories, knowledge, instinct, and drives you don't consciously access.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "psychic" },
            Source = "Dark Archive",
            Rarity = "common"
        };
    }
    private PfClass CreateSorcererClass()
    {
        return new PfClass
        {
            Id = "sorcerer",
            Name = "Sorcerer",
            Description = "You didn't choose to become a spellcaster—you were born one. There's magic in your blood, whether a draconic ancestor, a supernatural event, or something else has left its touch on your lineage. Magic is a part of your being, and you've learned to harness and shape its power.",
            KeyAbilities = new List<string> { "Charisma" },
            HitPoints = 6,
            SkillRanks = 2,
            SpellcastingTradition = "Varies by bloodline",
            IsPreparedCaster = false,
            IsSpontaneousCaster = true,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Bloodline",
                        Description = "Choose a bloodline that gives you your magical power. Your bloodline determines your spell tradition, grants you a bloodline spell, and gives you a blood magic effect.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Sorcerous Spellcasting",
                        Description = "Your bloodline provides you with incredible magical power. You can cast spells using the Cast a Spell activity, and you can supply material, somatic, and verbal components when casting spells. At 1st level, you can cast two 1st-rank spells and five cantrips each day. You know these spells; they aren't prepared in advance. As you gain levels, your number of spells per day and the highest rank of spells you can cast both increase.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Spell Repertoire",
                        Description = "The collection of spells you can cast is called your spell repertoire. At 1st level, you learn two 1st-rank spells of your choice and five cantrips of your choice, as well as an additional cantrip and spell from your bloodline.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "sorcerer" },
            Source = "Player Core 2",
            Rarity = "common"
        };
    }
    private PfClass CreateSummonerClass()
    {
        return new PfClass
        {
            Id = "summoner",
            Name = "Summoner",
            Description = "You can magically beckon a powerful being called an eidolon to your side, serving as the mortal anchor that keeps it in the world and drawing on its power in exchange. Whether your eidolon is a friend, a servant, or even a personal rival, your connection to it defines both of your existences.",
            KeyAbilities = new List<string> { "Charisma" },
            HitPoints = 10,
            SkillRanks = 2,
            SpellcastingTradition = "Arcane",
            IsPreparedCaster = false,
            IsSpontaneousCaster = true,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Eidolon",
                        Description = "You have a connection with a powerful and usually otherworldly entity called an eidolon, and you can use your life force as a conduit to manifest this ephemeral entity into the mortal world.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Link Spells",
                        Description = "Your connection to your eidolon allows you both to cast link spells, special spells that have been forged through your shared connection. Link spells are focus spells, but you don't need to Refocus to regain them; you automatically regain all spent link spells when you make your next daily preparations.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Spell Repertoire",
                        Description = "Your eidolon connection allows you to cast spells. You know one cantrip, which you choose from the common cantrips from your eidolon's tradition, or from other cantrips from that tradition to which you have access.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Evolution Feat",
                        Description = "Evolution feats are special feats that can enhance your eidolon in various ways. Your eidolon gains one evolution feat.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "summoner" },
            Source = "Secrets of Magic",
            Rarity = "common"
        };
    }
    private PfClass CreateSwashbucklerClass()
    {
        return new PfClass
        {
            Id = "swashbuckler",
            Name = "Swashbuckler",
            Description = "Many warriors rely on heavy armor and weapons. You've learned that in battle, the best offense is a good defense, and the best defense is not getting hit at all. You practice an elegant form of combat that grants you exceptional mobility, flourishing strikes, and elaborate tactics that bewilder your foes.",
            KeyAbilities = new List<string> { "Dexterity" },
            HitPoints = 10,
            SkillRanks = 4,
            SpellcastingTradition = "None",
            IsSpellcaster = false,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Panache",
                        Description = "You care as much about the way you accomplish something as whether you actually accomplish it in the first place. When you perform an action with particular flair, you can leverage this moment of triumph to perform spectacular, deadly maneuvers. This state of confidence and composure is called panache.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Precise Strike",
                        Description = "When you have panache and you Strike with an agile or finesse melee weapon or agile or finesse unarmed attack, you deal 2 additional precision damage. If the strike is part of a finisher, the additional damage is 2d6 precision damage instead.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Confident Finisher",
                        Description = "You gain the Confident Finisher action. Requirements: You have panache. You make a melee Strike. If it hits and deals damage, you can regain up to 1 Focus Point, and you lose your panache. If the Strike doesn't hit or doesn't deal damage, you don't lose panache.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Swashbuckler Style",
                        Description = "You've developed your own distinctive fighting style that blends martial prowess with practiced panache. Choose a swashbuckler style. This grants you a swashbuckler feat, additional skills, and defines actions that make you gain panache.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "swashbuckler" },
            Source = "Player Core 2",
            Rarity = "common"
        };
    }
    private PfClass CreateThaumaturgeClass()
    {
        return new PfClass
        {
            Id = "thaumaturge",
            Name = "Thaumaturge",
            Description = "The world is full of the unexplainable: ancient magic, dead gods, and stranger things. In response, you've scavenged trinkets and made tools that can give you an edge when dealing with the impossible. By knowing about the impossible, you can exploit it.",
            KeyAbilities = new List<string> { "Charisma" },
            HitPoints = 8,
            SkillRanks = 3,
            SpellcastingTradition = "None",
            IsSpellcaster = false,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Esoterica",
                        Description = "You understand the myriad ways magic and the supernatural world interact with mortal lives, and you carry a collection of esoterica: Amulets, talismans, and other items that can give you an edge.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "First Implement and Esoterica",
                        Description = "Your implement is a special object of symbolic importance to you: your badge of office, a particular family heirloom, or some other meaningful possession. You begin play with a first implement, which grants you the initiate benefit for that implement.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Exploit Vulnerability",
                        Description = "You know how to take advantage of your enemies' weak points and flaws. You can spend actions to perform various exploration and knowledge-gathering activities to learn about your foes and then use this knowledge in combat through the Exploit Vulnerability action.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "thaumaturge" },
            Source = "Dark Archive",
            Rarity = "common"
        };
    }
    private PfClass CreateWitchClass()
    {
        return new PfClass
        {
            Id = "witch",
            Name = "Witch",
            Description = "You are a witch, one of the most misunderstood practitioners of magic. While the amateur dabbler might muddle through with a few cantrips and a spell or two, you work magic the way it's meant to be worked: deliberately, with care and forethought, after extensive study, and according to a sacred tradition passed down over generations.",
            KeyAbilities = new List<string> { "Intelligence" },
            HitPoints = 6,
            SkillRanks = 2,
            SpellcastingTradition = "Varies by patron",
            IsPreparedCaster = true,
            IsSpontaneousCaster = false,
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Reflex"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained,
                    ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained, ProficiencyRank.Trained
                },
                ["Will"] = new List<ProficiencyRank>
                {
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Expert,
                    ProficiencyRank.Expert, ProficiencyRank.Expert, ProficiencyRank.Master, ProficiencyRank.Master, ProficiencyRank.Master
                }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Name = "Patron",
                        Description = "You forge a pact with an otherworldly being such as a fey lord, archfiend, or similar entity. Your patron grants you a familiar and teaches you spells, but the terms of your pact are known only to you and your patron.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Familiar",
                        Description = "Your patron grants you a familiar, a spirit or otherworldly being that aids your magic. The familiar uses your saves and AC, but most of its other statistics are based on its type.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Witch Spellcasting",
                        Description = "Using your familiar as a conduit, you can cast spells of your patron's tradition. At 1st level, you can prepare two 1st-rank spells and five cantrips each morning from spells your familiar knows, which you can find in your familiar.",
                        Level = 1},
                    new PfClassFeature
                    {
                        Name = "Hexes",
                        Description = "You learn your first hex, which is a special type of focus spell. Hexes are a type of focus spell that have the hex trait. You can cast focus spells only after you Refocus, which restores your focus points.",
                        Level = 1}
                }
            },
            Traits = new List<string> { "witch" },
            Source = "Player Core",
            Rarity = "common"
        };
    }

    private PfClass CreateExemplarClass()
    {
        return new PfClass
        {
            Id = "exemplar",
            Name = "Exemplar",
            Description = "You are a mortal touched by divine power, wielding a spark of immortal essence that sets you apart. Whether through heroic deeds, divine intervention, or a spark of destiny, you have begun walking the path toward true immortality and legend.",
            KeyAbilities = new List<string> { "Constitution" },
            HitPoints = 10,
            SkillRanks = 3,
            Source = "War of Immortals",
            Rarity = "uncommon",
            SaveProgressions = new Dictionary<string, List<ProficiencyRank>>
            {
                ["Fortitude"] = new List<ProficiencyRank> { ProficiencyRank.Expert, ProficiencyRank.Master },
                ["Reflex"] = new List<ProficiencyRank> { ProficiencyRank.Trained, ProficiencyRank.Expert },
                ["Will"] = new List<ProficiencyRank> { ProficiencyRank.Expert, ProficiencyRank.Master }
            },
            ClassFeaturesByLevel = new Dictionary<int, List<PfClassFeature>>
            {
                [1] = new List<PfClassFeature>
                {
                    new PfClassFeature
                    {
                        Id = "exemplar_spark",
                        Name = "Exemplar's Spark",
                        Description = "You possess a spark of divine essence that grants you supernatural resilience and the potential for immortality. This spark manifests as enhanced vitality and resistance to mortal weaknesses.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "exemplar", "divine" },
                        Source = "War of Immortals"
                    },
                    new PfClassFeature
                    {
                        Id = "immortal_prowess",
                        Name = "Immortal Prowess", 
                        Description = "Your divine spark enhances your physical and mental capabilities beyond mortal limits. You gain bonuses to specific abilities based on your immortal path.",
                        Level = 1,
                        Type = "Class Feature",
                        Traits = new List<string> { "exemplar" },
                        Source = "War of Immortals"
                    }
                }
            },
            Traits = new List<string> { "exemplar" }
        };
    }

    private PfClass CreateBasicClass(string id, string name, string keyAbility, int hitPoints, string source)
    {
        return new PfClass
        {
            Id = id,
            Name = name,
            Description = $"A {name.ToLower()} character with unique abilities and features.",
            KeyAbilities = new List<string> { keyAbility },
            HitPoints = hitPoints,
            SkillRanks = 3,
            Source = source,
            Rarity = "Common",
            ClassFeatLevels = new List<int> { 1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 },
            
            FortitudeProgression = new Progression<SaveProgression>($"{id}_fortitude", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert)
            }),
            
            ReflexProgression = new Progression<SaveProgression>($"{id}_reflex", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert)
            }),
            
            WillProgression = new Progression<SaveProgression>($"{id}_will", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(9, Proficiency.Expert)
            }),
            
            PerceptionProgression = new Progression<PerceptionProgression>($"{id}_perception", new List<ProgressionStep>
            {
                new(1, Proficiency.Trained),
                new(7, Proficiency.Expert)
            })
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
        
        // Magic Missile - Evocation 1st Level (underscore version)
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
        
        // Magic Missile - Dash version for client compatibility
        _spells["magic-missile"] = new PfSpell
        {
            Id = "magic-missile", 
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
        
        // Lightning Bolt - Evocation 3rd Level
        _spells["lightning-bolt"] = new PfSpell
        {
            Id = "lightning-bolt",
            Name = "Lightning Bolt",
            Description = "A bolt of lightning strikes outward from your hand, dealing 4d12 electricity damage. The bolt forms a line from you to the target, and you must make a spell attack roll to target a creature. If you critically succeed, the target takes double damage and is stunned 1. All other creatures in the line must attempt a basic Reflex save or take the same damage as the target.",
            Level = 3,
            Traditions = new List<string> { "Arcane", "Primal" },
            School = "Evocation",
            Traits = new List<string> { "evocation", "electricity" },
            ActionCost = "2",
            Range = "120 feet",
            Area = "120-foot line",
            Duration = "instantaneous",
            Components = new List<string> { "somatic", "verbal" },
            HasAttackRoll = true,
            AttackType = "spell",
            SavingThrow = "basic Reflex",
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "4d12",
                    DamageType = "electricity"
                }
            },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "offense", "electricity" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "+1",
                    Effect = "The damage increases by 1d12.",
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "1d12",
                            DamageType = "electricity"
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
        
        // Mage Armor - 1st Level Abjuration
        _spells["mage_armor"] = new PfSpell
        {
            Id = "mage_armor",
            Name = "Mage Armor",
            Description = "You ward yourself with shimmering magical energy, gaining a +1 item bonus to AC and a maximum Dexterity modifier of +5. While wearing mage armor, you use your unarmored proficiency to calculate your AC.",
            Level = 1,
            Traditions = new List<string> { "Arcane", "Occult" },
            School = "Abjuration",
            Traits = new List<string> { "abjuration" },
            ActionCost = "2",
            Range = "touch",
            Targets = "1 creature",
            Duration = "until the next time you make your daily preparations",
            Components = new List<string> { "somatic", "verbal" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "defense", "buff" },
            Effects = new List<PfSpellEffect>
            {
                new() {
                    Type = "Modify",
                    Target = "AC",
                    Value = "+1",
                    BonusType = "item",
                    Duration = "daily_preparations"
                },
                new() {
                    Type = "Modify",
                    Target = "MaxDexterityModifier",
                    Value = "5",
                    Duration = "daily_preparations"
                }
            },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "4th",
                    Effect = "The item bonus to AC increases to +2, and the maximum Dexterity modifier increases to +6.",
                    AdditionalEffects = new List<PfSpellEffect>
                    {
                        new() {
                            Type = "Modify",
                            Target = "AC", 
                            Value = "+2",
                            BonusType = "item"
                        },
                        new() {
                            Type = "Modify",
                            Target = "MaxDexterityModifier",
                            Value = "6"
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
            Area = "5-foot emanation",
            Duration = "1 minute",
            Components = new List<string> { "somatic", "verbal" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "buff", "mental", "area" },
            Effects = new List<PfSpellEffect>
            {
                new() {
                    Type = "Modify",
                    Target = "AttackRolls",
                    Value = "+1",
                    BonusType = "status",
                    Duration = "1_minute",
                    Area = "5_foot_emanation"
                }
            }
        };
        
        // Detect Magic - 1st Level Divination
        _spells["detect_magic"] = new PfSpell
        {
            Id = "detect_magic",
            Name = "Detect Magic",
            Description = "You send out a pulse that registers the presence of magic. You receive no information beyond the presence or absence of magic. You can choose to ignore magic you're fully aware of, such as the magic items and ongoing spells of you and your allies.",
            Level = 1,
            Traditions = new List<string> { "Arcane", "Divine", "Occult", "Primal" },
            School = "Divination",
            Traits = new List<string> { "divination", "detection" },
            ActionCost = "2",
            Area = "30-foot emanation",
            Duration = "instantaneous",
            Components = new List<string> { "somatic", "verbal" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "detection", "utility" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "3rd",
                    Effect = "You learn the school of magic for the highest-level effect within range that the spell detects.",
                    AdditionalValues = new Dictionary<string, object>
                    {
                        ["detection_detail"] = "school_of_magic"
                    }
                },
                new() {
                    Level = "4th",
                    Effect = "As 3rd level, but you also pinpoint the source of the highest-level magic. Like for an imprecise sense, you don't learn the exact location, but can narrow down the source to within a 5-foot cube (or the nearest if larger than that).",
                    AdditionalValues = new Dictionary<string, object>
                    {
                        ["detection_detail"] = "school_and_location",
                        ["precision"] = "5_foot_cube"
                    }
                }
            }
        };
        
        // Fear - 1st Level Enchantment
        _spells["fear"] = new PfSpell
        {
            Id = "fear",
            Name = "Fear",
            Description = "You plant fear in the target's mind; it must attempt a Will save.",
            Level = 1,
            Traditions = new List<string> { "Arcane", "Divine", "Occult", "Primal" },
            School = "Enchantment",
            Traits = new List<string> { "enchantment", "emotion", "fear", "mental" },
            ActionCost = "2",
            Range = "30 feet",
            Targets = "1 creature",
            Duration = "varies",
            Components = new List<string> { "somatic", "verbal" },
            SavingThrow = "Will",
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "mental", "fear", "debuff" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "3rd",
                    Effect = "You can target up to 5 creatures.",
                    AdditionalValues = new Dictionary<string, object>
                    {
                        ["max_targets"] = 5
                    }
                }
            }
        };
        
        // True Strike - 1st Level Divination  
        _spells["true_strike"] = new PfSpell
        {
            Id = "true_strike",
            Name = "True Strike",
            Description = "A glimpse into the future ensures your next blow strikes true. The next time you make an attack roll before the end of your turn, roll the attack twice and use the better result. The attack ignores circumstance penalties to the attack roll and any flat check required due to the target being concealed or hidden.",
            Level = 1,
            Traditions = new List<string> { "Arcane", "Occult" },
            School = "Divination",
            Traits = new List<string> { "divination", "fortune" },
            ActionCost = "1",
            Range = "self",
            Duration = "until the end of your turn",
            Components = new List<string> { "verbal" },
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "fortune", "buff", "attack" },
            Effects = new List<PfSpellEffect>
            {
                new() {
                    Type = "Allow",
                    Target = "NextAttackRoll",
                    Value = "roll_twice_use_better",
                    Duration = "end_of_turn",
                    Parameters = new Dictionary<string, object>
                    {
                        ["ignore_circumstance_penalties"] = true,
                        ["ignore_concealment_checks"] = true
                    }
                }
            }
        };
        
        // Color Spray - 1st Level Illusion
        _spells["color_spray"] = new PfSpell
        {
            Id = "color_spray",
            Name = "Color Spray", 
            Description = "Vivid colors spray in a 15-foot cone from your hand. Each creature in the area must attempt a Will save. Creatures are dazzled for 1 round on all results but a critical success.",
            Level = 1,
            Traditions = new List<string> { "Arcane", "Occult" },
            School = "Illusion",
            Traits = new List<string> { "illusion", "incapacitation", "visual" },
            ActionCost = "2",
            Area = "15-foot cone",
            Duration = "varies",
            Components = new List<string> { "somatic", "verbal" },
            SavingThrow = "Will",
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "incapacitation", "visual", "dazzle" },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "2nd",
                    Effect = "Creatures are blinded for 1 round on a critical failure and stunned 1 on a failure.",
                    AdditionalValues = new Dictionary<string, object>
                    {
                        ["critical_failure"] = "blinded_1_round",
                        ["failure"] = "stunned_1"
                    }
                }
            }
        };
        
        // Ray of Frost - Evocation Cantrip
        _spells["ray_of_frost"] = new PfSpell
        {
            Id = "ray_of_frost",
            Name = "Ray of Frost",
            Description = "You blast an icy ray. Make a spell attack roll. The ray deals cold damage equal to 1d4 + your spellcasting ability modifier, with a +1 status bonus to damage if the target is taking persistent fire damage. On a critical hit, the target is slowed 1 until the end of your next turn.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Primal" },
            School = "Evocation",
            Traits = new List<string> { "attack", "cantrip", "cold", "evocation" },
            ActionCost = "2",
            Range = "120 feet",
            Targets = "1 creature",
            Duration = "instantaneous",
            Components = new List<string> { "somatic", "verbal" },
            IsCantrip = true,
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "attack", "cold", "cantrip" },
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "1d4",
                    DamageType = "cold",
                    AddCasterModifier = true
                }
            },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "cantrip",
                    Effect = "The damage increases by 1d4.",
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "1d4",
                            DamageType = "cold"
                        }
                    }
                }
            }
        };
        
        // Prestidigitation - Transmutation Cantrip
        _spells["prestidigitation"] = new PfSpell
        {
            Id = "prestidigitation",
            Name = "Prestidigitation",
            Description = "The simplest magic does your bidding. You can perform simple magical effects for as long as you Sustain the Spell. Each time you Sustain the Spell, you can choose one of four options.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Divine", "Occult", "Primal" },
            School = "Transmutation",
            Traits = new List<string> { "cantrip", "transmutation" },
            ActionCost = "2",
            Range = "10 feet",
            Targets = "1 object (cook, lift, or clean)",
            Duration = "sustained",
            Components = new List<string> { "somatic", "verbal" },
            IsCantrip = true,
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "utility", "cantrip" }
        };
        
        // Produce Flame - Evocation Cantrip
        _spells["produce_flame"] = new PfSpell
        {
            Id = "produce_flame",
            Name = "Produce Flame",
            Description = "A small ball of flame appears in the palm of your hand, and you can use it as a thrown weapon or a light source. The flame sheds bright light in a 20-foot radius and dim light for an additional 20 feet. If you attack with the flame, you make a spell attack roll. This is normally a ranged attack, but you can also make a melee spell attack against a creature in your unarmed reach.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Primal" },
            School = "Evocation",
            Traits = new List<string> { "attack", "cantrip", "evocation", "fire" },
            ActionCost = "2",
            Range = "30 feet",
            Targets = "1 creature",
            Duration = "until the start of your next turn",
            Components = new List<string> { "somatic", "verbal" },
            IsCantrip = true,
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "attack", "fire", "cantrip", "light" },
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "1d4",
                    DamageType = "fire",
                    AddCasterModifier = true
                }
            },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "cantrip",
                    Effect = "The damage increases by 1d4.",
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "1d4",
                            DamageType = "fire"
                        }
                    }
                }
            }
        };
        
        // Add dash-separated versions for client compatibility (to fix hover card lookups)
        // These are aliases that reference the existing spell objects
        
        // Mage Hand dash version
        _spells["mage-hand"] = _spells["mage_hand"];
        
        // Detect Magic dash version
        _spells["detect-magic"] = _spells["detect_magic"];
        
        // Acid Splash - add both underscore and dash versions
        var acidSplash = new PfSpell
        {
            Id = "acid_splash",
            Name = "Acid Splash",
            Description = "You splash a glob of acid that splatters your target and nearby creatures. Make a spell attack roll against your target. On a hit, you deal acid damage equal to 1d6 + your spellcasting ability modifier. On a critical hit, double the damage. If your attack roll is a natural 1, you splash yourself and take 1 damage.",
            Level = 0,
            Traditions = new List<string> { "Arcane", "Primal" },
            School = "Evocation",
            Traits = new List<string> { "acid", "attack", "cantrip", "evocation" },
            ActionCost = "2",
            Range = "30 feet",
            Targets = "1 creature",
            Components = new List<string> { "somatic", "verbal" },
            IsCantrip = true,
            Source = "Core Rulebook",
            Rarity = "Common",
            Tags = new List<string> { "attack", "acid", "cantrip" },
            Damage = new List<PfSpellDamage>
            {
                new() {
                    DiceFormula = "1d6",
                    DamageType = "acid",
                    AddCasterModifier = true
                }
            },
            Heightening = new List<PfSpellHeightening>
            {
                new() {
                    Level = "cantrip",
                    Effect = "The damage increases by 1d6.",
                    AdditionalDamage = new List<PfSpellDamage>
                    {
                        new() {
                            DiceFormula = "1d6",
                            DamageType = "acid"
                        }
                    }
                }
            }
        };
        
        _spells["acid_splash"] = acidSplash;
        _spells["acid-splash"] = acidSplash;
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
        
        // Alchemist multiclass archetype
        _archetypes["alchemist_multiclass"] = new PfArchetype
        {
            Id = "alchemist_multiclass",
            Name = "Alchemist",
            Description = "You enjoy tinkering with alchemical formulas and substances in your spare time, and your studies have progressed beyond mere experimentation.",
            Type = ArchetypeType.Multiclass,
            AssociatedClassId = "alchemist",
            DedicationFeatId = "alchemist_dedication",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "AbilityScore", 
                    Target = "Intelligence",
                    Operator = ">=",
                    Value = "14"
                }
            },
            ArchetypeFeatIds = new List<string>
            {
                "alchemist_dedication",
                "basic_alchemy",
                "quick_alchemy",
                "advanced_alchemy",
                "expert_alchemy"
            },
            Traits = new List<string> { "archetype", "multiclass" },
            Source = "Core Rulebook"
        };
        
        // Barbarian multiclass archetype
        _archetypes["barbarian_multiclass"] = new PfArchetype
        {
            Id = "barbarian_multiclass",
            Name = "Barbarian",
            Description = "You have learned to harness and control your anger, gaining the ability to enter a rage-like state to augment your combat prowess.",
            Type = ArchetypeType.Multiclass,
            AssociatedClassId = "barbarian",
            DedicationFeatId = "barbarian_dedication",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "AbilityScore",
                    Target = "Strength",
                    Operator = ">=",
                    Value = "14"
                },
                new() {
                    Type = "AbilityScore",
                    Target = "Constitution", 
                    Operator = ">=",
                    Value = "14"
                }
            },
            ArchetypeFeatIds = new List<string>
            {
                "barbarian_dedication",
                "basic_fury",
                "barbarian_resiliency",
                "advanced_fury",
                "instinct_ability",
                "juggernaut_fortitude"
            },
            Traits = new List<string> { "archetype", "multiclass" },
            Source = "Core Rulebook"
        };
        
        // Bard multiclass archetype
        _archetypes["bard_multiclass"] = new PfArchetype
        {
            Id = "bard_multiclass",
            Name = "Bard",
            Description = "A muse has called you to dabble in occult lore, allowing you to cast a few spells. The deeper you delve, the more powerful your performances become.",
            Type = ArchetypeType.Multiclass,
            AssociatedClassId = "bard",
            DedicationFeatId = "bard_dedication",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "AbilityScore",
                    Target = "Charisma",
                    Operator = ">=",
                    Value = "14"
                }
            },
            ArchetypeFeatIds = new List<string>
            {
                "bard_dedication",
                "basic_bard_spellcasting",
                "basic_occult_spellcasting",
                "bard_counter_performance",
                "advanced_occult_spellcasting",
                "expert_bard_spellcasting"
            },
            Traits = new List<string> { "archetype", "multiclass" },
            Source = "Core Rulebook"
        };
        
        // Champion multiclass archetype
        _archetypes["champion_multiclass"] = new PfArchetype
        {
            Id = "champion_multiclass",
            Name = "Champion",
            Description = "You have sworn a solemn oath to your deity, gaining divine power to protect the innocent and fight for justice and righteousness.",
            Type = ArchetypeType.Multiclass,
            AssociatedClassId = "champion",
            DedicationFeatId = "champion_dedication",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "AbilityScore",
                    Target = "Strength",
                    Operator = ">=",
                    Value = "14"
                },
                new() {
                    Type = "AbilityScore",
                    Target = "Charisma",
                    Operator = ">=",
                    Value = "14"
                }
            },
            ArchetypeFeatIds = new List<string>
            {
                "champion_dedication",
                "basic_devotion",
                "champion_resiliency",
                "divine_ally",
                "advanced_devotion",
                "champion_expertise"
            },
            Traits = new List<string> { "archetype", "multiclass" },
            Source = "Core Rulebook"
        };
        
        // Acrobat archetype
        _archetypes["acrobat"] = new PfArchetype
        {
            Id = "acrobat",
            Name = "Acrobat",
            Description = "You have trained your body to perform incredible feats of agility, contorting and twisting through obstacles with ease.",
            Type = ArchetypeType.General,
            AssociatedClassId = null,
            DedicationFeatId = "acrobat_dedication",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "Proficiency",
                    Target = "Acrobatics",
                    Operator = ">=",
                    Value = "Trained"
                }
            },
            ArchetypeFeatIds = new List<string>
            {
                "acrobat_dedication",
                "contortionist",
                "dodge_away",
                "graceful_leaper",
                "tumbling_strike"
            },
            Traits = new List<string> { "archetype" },
            Source = "Advanced Player's Guide"
        };
        
        // Archer archetype
        _archetypes["archer"] = new PfArchetype
        {
            Id = "archer",
            Name = "Archer",
            Description = "Whether you favor a bow, crossbow, or other ranged weapon, you've honed your skill with these weapons to a degree that few can match.",
            Type = ArchetypeType.General,
            AssociatedClassId = null,
            DedicationFeatId = "archer_dedication",
            Prerequisites = new List<PfPrerequisite>(),
            ArchetypeFeatIds = new List<string>
            {
                "archer_dedication",
                "archer_reload",
                "crossbow_terror",
                "double_shot",
                "triple_shot"
            },
            Traits = new List<string> { "archetype" },
            Source = "Advanced Player's Guide"
        };
        
        // Assassin archetype
        _archetypes["assassin"] = new PfArchetype
        {
            Id = "assassin",
            Name = "Assassin",
            Description = "You've trained to assassinate your foes, and you do so with tenacious dedication. You may have trained in formal assassin schools or learned the trade on your own.",
            Type = ArchetypeType.General,
            AssociatedClassId = null,
            DedicationFeatId = "assassin_dedication",
            Prerequisites = new List<PfPrerequisite>
            {
                new() {
                    Type = "Proficiency",
                    Target = "Deception",
                    Operator = ">=",
                    Value = "Trained"
                },
                new() {
                    Type = "Proficiency",
                    Target = "Stealth",
                    Operator = ">=",
                    Value = "Trained"
                }
            },
            ArchetypeFeatIds = new List<string>
            {
                "assassin_dedication",
                "surprise_attack",
                "sneak_savant",
                "angel_of_death",
                "assassinate"
            },
            Traits = new List<string> { "archetype" },
            Source = "Advanced Player's Guide"
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
        
        // Kobold Warrior - CR -1
        _monsters["kobold-warrior"] = new PfMonster
        {
            Id = "kobold-warrior",
            Name = "Kobold Warrior",
            Description = "The typical kobold warrior is a cunning and cowardly reptilian humanoid that dwells in dark places.",
            Level = -1,
            Traits = new List<string> { "LE", "Small", "Kobold", "Humanoid" },
            Size = "Small",
            Alignment = "LE",
            ArmorClass = 16,
            HitPoints = 8,
            HitPointsFormula = "1d6+2",
            FortitudeSave = 4,
            ReflexSave = 6,
            WillSave = 2,
            Perception = 4,
            PerceptionNotes = new List<string> { "darkvision" },
            Skills = new Dictionary<string, int>
            {
                ["Acrobatics"] = 6,
                ["Athletics"] = 0,
                ["Crafting"] = 2,
                ["Stealth"] = 6
            },
            Languages = new List<string> { "Draconic" },
            Senses = new List<string> { "darkvision" },
            Speeds = new Dictionary<string, int> { ["land"] = 25 },
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 8,
                ["Dexterity"] = 17,
                ["Constitution"] = 12,
                ["Intelligence"] = 10,
                ["Wisdom"] = 12,
                ["Charisma"] = 8
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Spear",
                    AttackBonus = 6,
                    Traits = new List<string> { "thrown 20 ft." },
                    DamageFormula = "1d6-1",
                    DamageType = "piercing",
                    Range = "melee",
                    RangeIncrement = 20
                },
                new()
                {
                    Name = "Sling",
                    AttackBonus = 6,
                    Traits = new List<string> { "propulsive", "range increment 50 feet", "reload 1" },
                    DamageFormula = "1d6",
                    DamageType = "bludgeoning",
                    Range = "ranged",
                    RangeIncrement = 50
                }
            },
            Items = new List<string> { "leather armor", "spear", "sling with 20 bullets" },
            Source = "Bestiary"
        };
        
        // Skeleton Warrior - CR -1
        _monsters["skeleton-warrior"] = new PfMonster
        {
            Id = "skeleton-warrior",
            Name = "Skeleton Warrior",
            Description = "These undead are animated bones of dead warriors, often created by necromancers as guardians or soldiers.",
            Level = -1,
            Traits = new List<string> { "NE", "Medium", "Mindless", "Skeleton", "Undead" },
            Size = "Medium",
            Alignment = "NE",
            ArmorClass = 16,
            HitPoints = 4,
            HitPointsFormula = "1d6-1",
            Immunities = new List<string> { "death effects", "disease", "mental", "paralyzed", "poison", "unconscious" },
            Resistances = new List<string> { "cold 5", "electricity 5", "fire 5", "piercing 5", "slashing 5" },
            FortitudeSave = 2,
            ReflexSave = 4,
            WillSave = 0,
            Perception = 2,
            PerceptionNotes = new List<string> { "darkvision" },
            Skills = new Dictionary<string, int>
            {
                ["Acrobatics"] = 4,
                ["Athletics"] = 2
            },
            Languages = new List<string>(),
            Senses = new List<string> { "darkvision" },
            Speeds = new Dictionary<string, int> { ["land"] = 25 },
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 14,
                ["Dexterity"] = 14,
                ["Constitution"] = -3,
                ["Intelligence"] = -5,
                ["Wisdom"] = 10,
                ["Charisma"] = 4
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Scimitar",
                    AttackBonus = 4,
                    Traits = new List<string> { "forceful", "sweep" },
                    DamageFormula = "1d6+2",
                    DamageType = "slashing",
                    Range = "melee"
                },
                new()
                {
                    Name = "Shortbow",
                    AttackBonus = 4,
                    Traits = new List<string> { "deadly d10", "range increment 60 feet", "reload 0" },
                    DamageFormula = "1d6",
                    DamageType = "piercing",
                    Range = "ranged",
                    RangeIncrement = 60
                }
            },
            Items = new List<string> { "scimitar", "shortbow with 20 arrows" },
            Source = "Bestiary"
        };
        
        // Wolf - CR 1
        _monsters["wolf"] = new PfMonster
        {
            Id = "wolf",
            Name = "Wolf",
            Description = "Wolves roam forests, hills, and other wild lands in packs, hunting for sustenance and defining territory.",
            Level = 1,
            Traits = new List<string> { "N", "Medium", "Animal" },
            Size = "Medium",
            Alignment = "N",
            ArmorClass = 17,
            HitPoints = 24,
            HitPointsFormula = "4d8+4",
            FortitudeSave = 7,
            ReflexSave = 9,
            WillSave = 5,
            Perception = 7,
            PerceptionNotes = new List<string> { "low-light vision", "scent (imprecise) 30 feet" },
            Skills = new Dictionary<string, int>
            {
                ["Acrobatics"] = 7,
                ["Athletics"] = 5,
                ["Stealth"] = 7,
                ["Survival"] = 7
            },
            Languages = new List<string>(),
            Senses = new List<string> { "low-light vision", "scent (imprecise) 30 feet" },
            Speeds = new Dictionary<string, int> { ["land"] = 35 },
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 12,
                ["Dexterity"] = 17,
                ["Constitution"] = 13,
                ["Intelligence"] = -4,
                ["Wisdom"] = 15,
                ["Charisma"] = 6
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Jaws",
                    AttackBonus = 7,
                    Traits = new List<string>(),
                    DamageFormula = "1d8+1",
                    DamageType = "piercing",
                    Range = "melee",
                    AdditionalEffects = new List<string> { "Knockdown" }
                }
            },
            Actions = new List<PfMonsterAction>
            {
                new()
                {
                    Name = "Pack Attack",
                    Description = "The wolf's Strikes deal 1d4 extra damage to creatures within reach of at least two of the wolf's allies.",
                    ActionType = ActionType.Passive
                }
            },
            Source = "Bestiary"
        };
        
        // Zombie Shambler - CR -1  
        _monsters["zombie-shambler"] = new PfMonster
        {
            Id = "zombie-shambler",
            Name = "Zombie Shambler",
            Description = "A zombie shambler is a slow-moving horror created from a corpse.",
            Level = -1,
            Traits = new List<string> { "NE", "Medium", "Mindless", "Undead", "Zombie" },
            Size = "Medium",
            Alignment = "NE",
            ArmorClass = 12,
            HitPoints = 20,
            HitPointsFormula = "3d8+6",
            Immunities = new List<string> { "death effects", "disease", "mental", "paralyzed", "poison", "unconscious" },
            FortitudeSave = 6,
            ReflexSave = 0,
            WillSave = 2,
            Perception = 2,
            PerceptionNotes = new List<string> { "darkvision" },
            Skills = new Dictionary<string, int>(),
            Languages = new List<string>(),
            Senses = new List<string> { "darkvision" },
            Speeds = new Dictionary<string, int> { ["land"] = 25 },
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 16,
                ["Dexterity"] = 6,
                ["Constitution"] = 16,
                ["Intelligence"] = -5,
                ["Wisdom"] = 10,
                ["Charisma"] = 5
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Fist",
                    AttackBonus = 5,
                    Traits = new List<string>(),
                    DamageFormula = "1d6+3",
                    DamageType = "bludgeoning",
                    Range = "melee"
                }
            },
            Actions = new List<PfMonsterAction>
            {
                new()
                {
                    Name = "Slow",
                    Description = "A zombie shambler is permanently slowed 1 and can't use reactions.",
                    ActionType = ActionType.Passive
                }
            },
            Source = "Bestiary"
        };
        
        // Ogre Warrior - CR 3
        _monsters["ogre-warrior"] = new PfMonster
        {
            Id = "ogre-warrior",
            Name = "Ogre Warrior",
            Description = "These brutish giants are dim-witted and violent, preferring to solve problems with their fists.",
            Level = 3,
            Traits = new List<string> { "CE", "Large", "Giant", "Humanoid" },
            Size = "Large",
            Alignment = "CE",
            ArmorClass = 17,
            HitPoints = 50,
            HitPointsFormula = "6d10+12",
            FortitudeSave = 11,
            ReflexSave = 5,
            WillSave = 7,
            Perception = 7,
            PerceptionNotes = new List<string> { "darkvision" },
            Skills = new Dictionary<string, int>
            {
                ["Athletics"] = 11,
                ["Intimidation"] = 7
            },
            Languages = new List<string> { "Jotun" },
            Senses = new List<string> { "darkvision" },
            Speeds = new Dictionary<string, int> { ["land"] = 25 },
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 18,
                ["Dexterity"] = 10,
                ["Constitution"] = 15,
                ["Intelligence"] = 8,
                ["Wisdom"] = 11,
                ["Charisma"] = 8
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Ogre Hook",
                    AttackBonus = 13,
                    Traits = new List<string> { "deadly d10", "reach 10 feet", "trip" },
                    DamageFormula = "1d10+7",
                    DamageType = "piercing",
                    Range = "melee",
                    Reach = 10
                },
                new()
                {
                    Name = "Javelin",
                    AttackBonus = 7,
                    Traits = new List<string> { "thrown 30 feet" },
                    DamageFormula = "1d6+7",
                    DamageType = "piercing",
                    Range = "ranged",
                    RangeIncrement = 30
                }
            },
            Items = new List<string> { "hide armor", "ogre hook", "javelin (4)" },
            Source = "Bestiary"
        };
        
        // Giant Spider - CR 1
        _monsters["giant-spider"] = new PfMonster
        {
            Id = "giant-spider",
            Name = "Giant Spider",
            Description = "These massive arachnids are deadly hunters that weave webs to catch prey.",
            Level = 1,
            Traits = new List<string> { "N", "Large", "Animal" },
            Size = "Large",
            Alignment = "N",
            ArmorClass = 17,
            HitPoints = 16,
            HitPointsFormula = "2d10+4",
            FortitudeSave = 7,
            ReflexSave = 10,
            WillSave = 4,
            Perception = 7,
            PerceptionNotes = new List<string> { "darkvision", "web sense" },
            Skills = new Dictionary<string, int>
            {
                ["Acrobatics"] = 7,
                ["Athletics"] = 5,
                ["Stealth"] = 7
            },
            Languages = new List<string>(),
            Senses = new List<string> { "darkvision", "web sense" },
            Speeds = new Dictionary<string, int> { ["land"] = 25, ["climb"] = 25 },
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 12,
                ["Dexterity"] = 17,
                ["Constitution"] = 14,
                ["Intelligence"] = -4,
                ["Wisdom"] = 12,
                ["Charisma"] = 4
            },
            Strikes = new List<PfStrike>
            {
                new()
                {
                    Name = "Fangs",
                    AttackBonus = 7,
                    Traits = new List<string> { "poison" },
                    DamageFormula = "1d8+1",
                    DamageType = "piercing",
                    Range = "melee",
                    AdditionalEffects = new List<string> { "giant spider venom" }
                }
            },
            Actions = new List<PfMonsterAction>
            {
                new()
                {
                    Name = "Web Trap",
                    Description = "A creature hit by the spider's web attack is immobilized and stuck to the nearest surface until it Escapes (DC 17).",
                    ActionType = ActionType.Action,
                    ActionCost = "2"
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