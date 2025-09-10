using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Rules;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Errors;
using static PathfinderCampaignManager.Domain.Errors.GeneralErrors;

namespace PathfinderCampaignManager.Infrastructure.Data;

public class RulesRepository : IRulesRepository
{
    private readonly Dictionary<RuleId, RuleRecord> _rules = new();
    private readonly Dictionary<RuleId, RuleTable> _tables = new();
    private readonly Dictionary<RuleId, RuleFormula> _formulas = new();
    private bool _isInitialized = false;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        
        await LoadCoreRules();
        await LoadRuleTables();
        await LoadRuleFormulas();
        
        _isInitialized = true;
    }

    public async Task<Result<RuleRecord>> GetRuleByIdAsync(RuleId id)
    {
        await InitializeAsync();
        
        if (_rules.TryGetValue(id, out var rule))
        {
            return Result.Success(rule);
        }
        
        return Result.Failure<RuleRecord>(GeneralErrors.NotFound);
    }

    public async Task<Result<IEnumerable<RuleRecord>>> GetRulesAsync(RuleCategory? category = null, string? searchTerm = null, IEnumerable<string>? tags = null)
    {
        await InitializeAsync();
        
        var rules = _rules.Values.AsEnumerable();
        
        if (category.HasValue)
        {
            rules = rules.Where(r => r.Category == category.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            rules = rules.Where(r => 
                r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.Summary.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.Tags.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }
        
        if (tags != null && tags.Any())
        {
            rules = rules.Where(r => tags.Any(tag => r.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
        }
        
        return Result<IEnumerable<RuleRecord>>.Success(rules.OrderBy(r => r.Name).AsEnumerable());
    }

    public async Task<Result<IEnumerable<RuleRecord>>> GetRulesByCategoryAsync(RuleCategory category)
    {
        await InitializeAsync();
        
        var rules = _rules.Values.Where(r => r.Category == category).OrderBy(r => r.Name);
        return Result<IEnumerable<RuleRecord>>.Success(rules.AsEnumerable());
    }

    public async Task<Result<RuleTable>> GetRuleTableAsync(RuleId ruleId)
    {
        await InitializeAsync();
        
        if (_tables.TryGetValue(ruleId, out var table))
        {
            return Result.Success(table);
        }
        
        return Result.Failure<RuleTable>(GeneralErrors.NotFound);
    }

    public async Task<Result<IEnumerable<RuleTable>>> GetRuleTablesAsync()
    {
        await InitializeAsync();
        return Result.Success(_tables.Values.AsEnumerable());
    }

    public async Task<Result<RuleFormula>> GetRuleFormulaAsync(RuleId ruleId)
    {
        await InitializeAsync();
        
        if (_formulas.TryGetValue(ruleId, out var formula))
        {
            return Result.Success(formula);
        }
        
        return Result.Failure<RuleFormula>(GeneralErrors.NotFound);
    }

    public async Task<Result<IEnumerable<RuleFormula>>> GetRuleFormulasAsync()
    {
        await InitializeAsync();
        return Result<IEnumerable<RuleFormula>>.Success(_formulas.Values.AsEnumerable());
    }

    public async Task<Result<IEnumerable<RuleRecord>>> SearchRulesAsync(string searchTerm)
    {
        return await GetRulesAsync(searchTerm: searchTerm);
    }

    public async Task<Result<IEnumerable<RuleRecord>>> GetRulesByTagAsync(string tag)
    {
        return await GetRulesAsync(tags: new[] { tag });
    }

    public async Task<Result<IEnumerable<string>>> GetAllTagsAsync()
    {
        await InitializeAsync();
        
        var tags = _rules.Values
            .SelectMany(r => r.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t);
            
        return Result<IEnumerable<string>>.Success(tags.AsEnumerable());
    }

    public async Task<Result<Dictionary<RuleCategory, int>>> GetRuleCategoryCountsAsync()
    {
        await InitializeAsync();
        
        var counts = _rules.Values
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.Count());
            
        return Result<Dictionary<RuleCategory, int>>.Success(counts);
    }

    public async Task<Result<IEnumerable<string>>> GetTraitsAsync()
    {
        await InitializeAsync();
        
        var traits = _rules.Values
            .SelectMany(r => r.Traits)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t);
            
        return Result<IEnumerable<string>>.Success(traits.AsEnumerable());
    }

    public async Task<Result<bool>> ValidateRuleIntegrityAsync()
    {
        await InitializeAsync();
        
        // Validate no level references
        var hasLevelReferences = _rules.Values.Any(r => 
            r.Summary.Contains("level", StringComparison.OrdinalIgnoreCase) ||
            (r.DetailsMarkdown?.Contains("level", StringComparison.OrdinalIgnoreCase) ?? false));
            
        // Validate no campaign references  
        var hasCampaignReferences = _rules.Values.Any(r =>
            r.Summary.Contains("campaign", StringComparison.OrdinalIgnoreCase) ||
            r.Summary.Contains("character", StringComparison.OrdinalIgnoreCase) ||
            (r.DetailsMarkdown?.Contains("campaign", StringComparison.OrdinalIgnoreCase) ?? false));
            
        // Check for placeholders
        var hasPlaceholders = _rules.Values.Any(r =>
            r.Summary.Contains("TODO", StringComparison.OrdinalIgnoreCase) ||
            r.Summary.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase) ||
            r.Summary.Contains("TBD", StringComparison.OrdinalIgnoreCase));
            
        var isValid = !hasLevelReferences && !hasCampaignReferences && !hasPlaceholders;
        return Result<bool>.Success(isValid);
    }

    private async Task LoadCoreRules()
    {
        LoadCoreMechanicsRules();
        LoadCombatRules();
        LoadDefenseRules();
        LoadSaveRules();
        LoadConditionRules();
        LoadSenseRules();
        LoadEnvironmentRules();
        LoadSpellcastingRules();
        LoadExplorationRules();
        LoadItemRules();
        LoadSubsystemRules();
        LoadAdvancedRules();
        LoadGMRules();
        
        await Task.CompletedTask;
    }
    
    private void LoadCoreMechanicsRules()
    {
        // Action Economy
        _rules["action-economy"] = RuleRecord.Create(
            "action-economy",
            "Action Economy",
            RuleCategory.CoreMechanics,
            RuleContentType.Article,
            "Each turn, you can use three actions, one reaction, and any number of free actions.",
            @"**Three Actions**: Most activities require one, two, or three actions to perform.
**Reaction**: Triggered by specific circumstances outside your turn.
**Free Actions**: Simple actions that don't consume your main actions for the turn.",
            new[] { "Action" },
            new[] { "turn", "actions", "economy" },
            new[] { "CRB-462" }
        );

        // Degrees of Success
        _rules["degrees-of-success"] = RuleRecord.Create(
            "degrees-of-success",
            "Degrees of Success",
            RuleCategory.CoreMechanics,
            RuleContentType.Article,
            "Check results are categorized as critical success, success, failure, or critical failure.",
            @"**Critical Success**: Beat DC by 10 or more, or roll natural 20.
**Success**: Meet or beat the DC.
**Failure**: Fail to meet the DC.
**Critical Failure**: Fail DC by 10 or more, or roll natural 1.",
            Array.Empty<string>(),
            new[] { "checks", "results", "critical" },
            new[] { "CRB-445" }
        );

        // Proficiency
        _rules["proficiency"] = RuleRecord.Create(
            "proficiency",
            "Proficiency",
            RuleCategory.CoreMechanics,
            RuleContentType.Article,
            "Your training in skills, weapons, and other abilities is measured by proficiency ranks.",
            @"**Untrained**: No bonus to proficiency.
**Trained**: Proficiency bonus +2.
**Expert**: Proficiency bonus +4.  
**Master**: Proficiency bonus +6.
**Legendary**: Proficiency bonus +8.",
            Array.Empty<string>(),
            new[] { "training", "bonus", "ranks" },
            new[] { "CRB-444" }
        );

        // DCs and Checks
        _rules["dcs-and-checks"] = RuleRecord.Create(
            "dcs-and-checks",
            "DCs and Checks",
            RuleCategory.CoreMechanics,
            RuleContentType.Table,
            "Standard difficulty classes for various challenge ratings.",
            "DCs range from Trivial (5) to Nearly Impossible (40) based on the challenge presented.",
            Array.Empty<string>(),
            new[] { "difficulty", "checks", "dc" },
            new[] { "CRB-503" }
        );
    }

    private void LoadCombatRules()
    {
        // Cover
        _rules["cover"] = RuleRecord.Create(
            "cover",
            "Cover",
            RuleCategory.Combat,
            RuleContentType.Table,
            "Physical barriers between you and a target provide cover, granting bonuses to AC and Reflex saves.",
            @"Cover is determined by drawing a line from any corner of your square to any corner of the target's square.
The GM determines the degree of cover based on obstacles in the line.",
            new[] { "Circumstance" },
            new[] { "AC", "reflex", "defense", "barrier" },
            new[] { "CRB-477" }
        );

        // Flanking
        _rules["flanking"] = RuleRecord.Create(
            "flanking",
            "Flanking",
            RuleCategory.Combat,
            RuleContentType.Article,
            "When you and an ally are on opposite sides of an enemy, that enemy is flat-footed against your attacks.",
            @"Draw a line from the center of your square to the center of your ally's square. 
If the line passes through opposite sides of the enemy's space, the enemy is flanked and flat-footed against your melee attacks.",
            new[] { "Circumstance" },
            new[] { "positioning", "flat-footed", "melee" },
            new[] { "CRB-479" }
        );

        // Movement Types
        _rules["movement-types"] = RuleRecord.Create(
            "movement-types",
            "Movement Types",
            RuleCategory.Combat,
            RuleContentType.Article,
            "Creatures can move on land, climb, swim, fly, or burrow, each with specific rules.",
            @"**Land Speed**: Standard movement across solid ground.
**Climb Speed**: Move on vertical surfaces or difficult inclines.
**Swim Speed**: Move through liquid environments.
**Fly Speed**: Move through the air in three dimensions.
**Burrow Speed**: Move through solid matter like earth or sand.",
            new[] { "Move" },
            new[] { "speed", "terrain", "locomotion" },
            new[] { "CRB-463" }
        );
    }

    private void LoadDefenseRules()
    {
        // Damage Types
        _rules["damage-types"] = RuleRecord.Create(
            "damage-types",
            "Damage Types",
            RuleCategory.Defenses,
            RuleContentType.Glossary,
            "Damage is categorized by type, affecting how resistances, weaknesses, and immunities apply.",
            @"**Physical**: Bludgeoning, Piercing, Slashing
**Energy**: Acid, Cold, Electricity, Fire, Sonic
**Alignment**: Chaotic, Evil, Good, Lawful  
**Other**: Force, Mental, Poison, Positive, Negative",
            Array.Empty<string>(),
            new[] { "damage", "energy", "physical", "immunity" },
            new[] { "CRB-452" }
        );

        // Persistent Damage
        _rules["persistent-damage"] = RuleRecord.Create(
            "persistent-damage",
            "Persistent Damage",
            RuleCategory.Defenses,
            RuleContentType.Article,
            "Ongoing damage that continues each round until removed by a flat check or other means.",
            @"At the end of your turn, take the persistent damage, then attempt a DC 15 flat check. 
Success removes the persistent damage. The DC can be modified by circumstances or assistance.",
            Array.Empty<string>(),
            new[] { "ongoing", "flat-check", "removal" },
            new[] { "CRB-621" }
        );

        // Resistance and Weakness
        _rules["resistance-weakness-immunity"] = RuleRecord.Create(
            "resistance-weakness-immunity",
            "Resistance, Weakness, and Immunity",
            RuleCategory.Defenses,
            RuleContentType.Article,
            "Special defenses that modify incoming damage based on damage type.",
            @"**Resistance**: Reduce damage of the specified type by the resistance value.
**Weakness**: Increase damage of the specified type by the weakness value.
**Immunity**: Take no damage of the specified type and ignore associated effects.",
            Array.Empty<string>(),
            new[] { "damage-reduction", "vulnerability", "protection" },
            new[] { "CRB-453" }
        );
    }

    private void LoadSaveRules()
    {
        // Basic Saves
        _rules["basic-saves"] = RuleRecord.Create(
            "basic-saves",
            "Basic Saves",
            RuleCategory.Saves,
            RuleContentType.Formula,
            "A type of saving throw where the degree of success determines damage taken.",
            @"**Critical Success**: No damage.
**Success**: Half damage.
**Failure**: Full damage.
**Critical Failure**: Double damage.",
            Array.Empty<string>(),
            new[] { "saving-throw", "damage", "reduction" },
            new[] { "CRB-449" }
        );

        // Counteract
        _rules["counteract"] = RuleRecord.Create(
            "counteract",
            "Counteract",
            RuleCategory.Saves,
            RuleContentType.Article,
            "The process of using one magical effect to cancel or suppress another.",
            @"Compare your counteract level to the target's counteract level. Roll a counteract check using the appropriate modifier.
Success removes or suppresses the effect based on the relative levels.",
            new[] { "Magical" },
            new[] { "dispel", "suppress", "cancel", "magic" },
            new[] { "CRB-458" }
        );
    }

    private void LoadConditionRules()
    {
        // Load all SRD conditions as individual rules
        var conditions = new Dictionary<string, (string name, string summary, string details)>
        {
            ["blinded"] = ("Blinded", "You can't see and automatically critically fail Perception checks that require sight.", 
                "All terrain is difficult terrain. You're immune to visual effects. Blinded overrides dazzled."),
            ["clumsy"] = ("Clumsy", "Your nimbleness and reflexes are impaired.", 
                "Take a status penalty equal to the condition value to Dexterity-based checks, AC, Reflex saves, ranged attacks, and Dex-based skills."),
            ["dazzled"] = ("Dazzled", "Your eyes are overstimulated and can't see as clearly.", 
                "All creatures and objects are concealed from you."),
            ["dying"] = ("Dying", "You are bleeding out or otherwise at death's door.", 
                "While dying, you are unconscious. If you ever reach dying 4, you die."),
            ["enfeebled"] = ("Enfeebled", "You're physically weakened.", 
                "Take a status penalty equal to the condition value to Strength-based checks, melee damage rolls, and Athletics checks."),
            ["fascinated"] = ("Fascinated", "You are compelled to focus your attention on something.", 
                "Take a -2 status penalty to Perception and skill checks. Can't use concentrate actions unless related to the fascination."),
            ["flat-footed"] = ("Flat-Footed", "You're distracted or otherwise unable to defend yourself properly.", 
                "Take a -2 circumstance penalty to AC."),
            ["frightened"] = ("Frightened", "You're gripped by fear and struggle to control your nerves.", 
                "Take a status penalty equal to the condition value to all checks and DCs. Decreases by 1 at end of turn."),
            ["grabbed"] = ("Grabbed", "A creature, object, or magic holds you in place.", 
                "You can't move unless you Escape. You can still attack but must include hands in manipulate actions."),
            ["prone"] = ("Prone", "You're lying on the ground.", 
                "You're flat-footed and take a -2 circumstance penalty to attack rolls. Standing up ends prone."),
            ["sickened"] = ("Sickened", "You feel ill.", 
                "Take a status penalty equal to the condition value to all checks and DCs. Can't willingly ingest anything."),
            ["stunned"] = ("Stunned", "You've become senseless.", 
                "You can't act while stunned. Usually includes a value indicating actions lost."),
            ["unconscious"] = ("Unconscious", "You're sleeping or have been knocked out.", 
                "You can't act. Take a -4 status penalty to AC, Perception, and Reflex saves. You have blinded and flat-footed.")
        };

        foreach (var (id, (name, summary, details)) in conditions)
        {
            _rules[id] = RuleRecord.Create(
                id,
                name,
                RuleCategory.Conditions,
                RuleContentType.Article,
                summary,
                details,
                new[] { "Condition" },
                new[] { "status", "effect" },
                new[] { "CRB-618" }
            );
        }
    }

    private void LoadSenseRules()
    {
        // Vision and Concealment
        _rules["vision-concealment"] = RuleRecord.Create(
            "vision-concealment",
            "Vision and Concealment",
            RuleCategory.Senses,
            RuleContentType.Table,
            "Rules for how creatures perceive each other and the effects of concealment.",
            @"Creatures can be observed, hidden, undetected, or unnoticed based on their concealment and the observer's senses.",
            Array.Empty<string>(),
            new[] { "perception", "stealth", "concealment", "detection" },
            new[] { "CRB-467" }
        );

        // Special Senses
        _rules["special-senses"] = RuleRecord.Create(
            "special-senses",
            "Special Senses",
            RuleCategory.Senses,
            RuleContentType.Glossary,
            "Enhanced senses that allow creatures to perceive their environment in unique ways.",
            @"**Darkvision**: See in complete darkness as if it were dim light.
**Low-Light Vision**: See in dim light as if it were bright light.
**Scent**: Detect creatures and objects by smell within 30 feet.
**Tremorsense**: Detect vibrations through solid surfaces.",
            Array.Empty<string>(),
            new[] { "darkvision", "scent", "tremorsense", "detection" },
            new[] { "CRB-465" }
        );
    }

    private void LoadEnvironmentRules()
    {
        // Environmental Damage
        _rules["environmental-damage"] = RuleRecord.Create(
            "environmental-damage",
            "Environmental Damage",
            RuleCategory.Environment,
            RuleContentType.Table,
            "Damage from environmental hazards categorized by severity.",
            "Environmental effects deal damage based on their severity: Minor, Moderate, Major, or Massive.",
            Array.Empty<string>(),
            new[] { "hazard", "environment", "damage", "severity" },
            new[] { "GMG-218" }
        );

        // Falling Damage
        _rules["falling-damage"] = RuleRecord.Create(
            "falling-damage",
            "Falling Damage",
            RuleCategory.Environment,
            RuleContentType.Formula,
            "Creatures take bludgeoning damage when falling from significant heights.",
            "Take 1d6 bludgeoning damage for every 10 feet fallen, up to a maximum of 750 damage (250d6).",
            Array.Empty<string>(),
            new[] { "fall", "bludgeoning", "height", "damage" },
            new[] { "CRB-463" }
        );

        // Suffocation
        _rules["suffocation"] = RuleRecord.Create(
            "suffocation",
            "Suffocation and Drowning",
            RuleCategory.Environment,
            RuleContentType.Article,
            "Rules for holding breath and the effects of running out of air.",
            @"You can hold your breath for minutes equal to 6 + Constitution modifier. 
After this time, you must make Fortitude saves or begin suffocating.",
            Array.Empty<string>(),
            new[] { "breath", "drowning", "constitution", "fortitude" },
            new[] { "CRB-478" }
        );
    }

    private void LoadSpellcastingRules()
    {
        // Spell Components
        _rules["spell-components"] = RuleRecord.Create(
            "spell-components",
            "Spell Components",
            RuleCategory.Spellcasting,
            RuleContentType.Article,
            "The required elements needed to cast a spell: somatic, verbal, material, and focus components.",
            @"**Somatic**: Hand gestures and movements.
**Verbal**: Spoken incantations.
**Material**: Physical components consumed or used.
**Focus**: Special items that channel magical energy.",
            new[] { "Concentrate", "Manipulate" },
            new[] { "casting", "requirements", "components" },
            new[] { "CRB-303" }
        );

        // Heightening Spells
        _rules["heightening-spells"] = RuleRecord.Create(
            "heightening-spells",
            "Heightening Spells",
            RuleCategory.Spellcasting,
            RuleContentType.Article,
            "Casting spells using higher-rank spell slots to increase their power.",
            "Many spells have heightened effects when cast with spell slots higher than their base rank.",
            new[] { "Metamagic" },
            new[] { "spell-rank", "power", "enhancement" },
            new[] { "CRB-299" }
        );

        // Areas of Effect
        _rules["areas-of-effect"] = RuleRecord.Create(
            "areas-of-effect",
            "Areas of Effect",
            RuleCategory.Spellcasting,
            RuleContentType.Article,
            "Spells and effects that target areas rather than specific creatures.",
            @"**Burst**: Spreads out in all directions from a corner of the origin square.
**Emanation**: Spreads out in all directions from the caster.
**Line**: Extends in a straight line from the caster.
**Cone**: Spreads out in a quarter-circle from the caster.",
            Array.Empty<string>(),
            new[] { "targeting", "area", "burst", "emanation", "line", "cone" },
            new[] { "CRB-457" }
        );
    }

    private void LoadExplorationRules()
    {
        // Exploration Activities
        _rules["exploration-activities"] = RuleRecord.Create(
            "exploration-activities",
            "Exploration Activities",
            RuleCategory.Exploration,
            RuleContentType.Article,
            "Activities characters can perform while exploring between encounters.",
            @"**Avoid Notice**: Move stealthily to avoid detection.
**Investigate**: Look around for clues and hidden things.
**Search**: Thoroughly examine an area for hidden things.
**Scout**: Range ahead of the group to spot dangers.",
            new[] { "Concentrate", "Exploration" },
            new[] { "stealth", "investigation", "search", "scouting" },
            new[] { "CRB-479" }
        );

        // Treat Wounds
        _rules["treat-wounds"] = RuleRecord.Create(
            "treat-wounds",
            "Treat Wounds",
            RuleCategory.Exploration,
            RuleContentType.Article,
            "Use Medicine to provide healing and remove the wounded condition.",
            @"Attempt a Medicine check to restore Hit Points. Success removes wounded 1 or reduces higher wounded values by 1.
The same creature can only benefit from Treat Wounds once per hour.",
            new[] { "Healing", "Manipulate" },
            new[] { "medicine", "healing", "wounded", "recovery" },
            new[] { "CRB-248" }
        );
    }

    private void LoadItemRules()
    {
        // Bulk and Encumbrance
        _rules["bulk-encumbrance"] = RuleRecord.Create(
            "bulk-encumbrance",
            "Bulk and Encumbrance",
            RuleCategory.Items,
            RuleContentType.Article,
            "System for tracking carrying capacity and the effects of heavy loads.",
            @"Your Bulk limit equals 5 + Strength modifier. 
**Encumbered**: Carry more than limit; -10 foot Speed penalty and -1 to checks.
**Overloaded**: Carry more than double limit; can't move.",
            Array.Empty<string>(),
            new[] { "carrying-capacity", "strength", "movement", "penalty" },
            new[] { "CRB-271" }
        );

        // Item Hardness and HP
        _rules["item-hardness-hp"] = RuleRecord.Create(
            "item-hardness-hp",
            "Item Hardness and Hit Points",
            RuleCategory.Items,
            RuleContentType.Table,
            "Rules for damaging and destroying objects and structures.",
            @"**Hardness**: Reduces damage dealt to the object.
**Hit Points**: When reduced to 0, object is broken.
**Broken Threshold**: When reduced to this value, object becomes broken.",
            Array.Empty<string>(),
            new[] { "destruction", "hardness", "durability", "broken" },
            new[] { "CRB-272" }
        );
    }

    private void LoadSubsystemRules()
    {
        // Victory Points
        _rules["victory-points"] = RuleRecord.Create(
            "victory-points",
            "Victory Points",
            RuleCategory.Subsystems,
            RuleContentType.Article,
            "A subsystem for tracking progress toward long-term goals in structured challenges.",
            @"**Victory Points**: Track progress toward completing a complex task.
**Advantage Points**: Earned through smart tactics or good fortune.
**Threshold**: Total VP needed to succeed at the challenge.
**Time Pressure**: Challenges often have time limits or escalating consequences.",
            Array.Empty<string>(),
            new[] { "subsystem", "progress", "challenges", "advantage" },
            new[] { "GMG-148" }
        );

        // Influence
        _rules["influence"] = RuleRecord.Create(
            "influence",
            "Influence",
            RuleCategory.Subsystems,
            RuleContentType.Article,
            "A subsystem for social encounters involving persuading important NPCs.",
            @"**Discovery**: Learn NPC interests, preferences, and goals.
**Influence Points**: Gained through successful social interactions.
**Resistance**: NPCs have defense against social manipulation.
**Consequences**: Both success and failure have ongoing story effects.",
            Array.Empty<string>(),
            new[] { "social", "npc", "persuasion", "intrigue" },
            new[] { "GMG-151" }
        );

        // Research
        _rules["research"] = RuleRecord.Create(
            "research",
            "Research",
            RuleCategory.Subsystems,
            RuleContentType.Article,
            "A subsystem for investigating mysteries and uncovering hidden knowledge.",
            @"**Research Points**: Measure progress toward solving a mystery.
**Libraries**: Different sources provide varying research bonuses.
**Time Investment**: Research takes time and sustained effort.
**Breakthroughs**: Major discoveries that advance multiple research tracks.",
            Array.Empty<string>(),
            new[] { "investigation", "knowledge", "mystery", "library" },
            new[] { "GMG-154" }
        );

        // Chases
        _rules["chases"] = RuleRecord.Create(
            "chases",
            "Chases",
            RuleCategory.Subsystems,
            RuleContentType.Article,
            "A subsystem for dynamic pursuit scenes with multiple participants.",
            @"**Chase Points**: Measure distance between participants.
**Obstacles**: Environmental challenges that slow or hinder movement.
**Terrain**: Different environments provide unique chase mechanics.
**Catch Up/Escape**: Specific thresholds for resolving the chase.",
            Array.Empty<string>(),
            new[] { "movement", "pursuit", "obstacles", "terrain" },
            new[] { "GMG-156" }
        );

        // Infiltration
        _rules["infiltration"] = RuleRecord.Create(
            "infiltration",
            "Infiltration",
            RuleCategory.Subsystems,
            RuleContentType.Article,
            "A subsystem for stealth missions and covert operations.",
            @"**Awareness Points**: Track how close guards are to discovering intruders.
**Security Features**: Alarms, guards, locks, and magical protections.
**Stealth vs Detection**: Contested rolls determine success or discovery.
**Consequences**: Getting caught leads to escalation, not immediate failure.",
            Array.Empty<string>(),
            new[] { "stealth", "guards", "security", "detection" },
            new[] { "GMG-159" }
        );

        // Reputation
        _rules["reputation"] = RuleRecord.Create(
            "reputation",
            "Reputation",
            RuleCategory.Subsystems,
            RuleContentType.Article,
            "A subsystem for tracking how NPCs and organizations view the characters.",
            @"**Reputation Scales**: Different groups have separate reputation tracks.
**Fame vs Infamy**: Positive and negative recognition have different effects.
**Reputation Events**: Major actions that significantly impact standing.
**Benefits and Drawbacks**: Reputation affects prices, services, and opportunities.",
            Array.Empty<string>(),
            new[] { "social", "fame", "organizations", "consequences" },
            new[] { "GMG-164" }
        );
    }

    private void LoadAdvancedRules()
    {
        // Duels
        _rules["duels"] = RuleRecord.Create(
            "duels",
            "Duels",
            RuleCategory.Combat,
            RuleContentType.Article,
            "Formal combat between two participants with special rules and conditions.",
            @"**Duel Types**: Different styles with unique rules (first blood, to knockout, to death).
**Honor Code**: Breaking duel etiquette has social consequences.
**Witnesses**: Audience affects stakes and outcomes.
**Victory Conditions**: Clear parameters for determining the winner.",
            Array.Empty<string>(),
            new[] { "formal-combat", "honor", "witnesses", "victory" },
            new[] { "GMG-166" }
        );

        // Leadership
        _rules["leadership"] = RuleRecord.Create(
            "leadership",
            "Leadership",
            RuleCategory.GMTools,
            RuleContentType.Article,
            "Rules for characters who lead organizations, settlements, or groups.",
            @"**Leadership Activities**: Special downtime activities for leaders.
**Followers**: NPCs who serve under the character's command.
**Organization Management**: Running guilds, kingdoms, or other groups.
**Mass Combat**: Rules for battles between large forces.",
            Array.Empty<string>(),
            new[] { "organizations", "followers", "management", "mass-combat" },
            new[] { "GMG-168" }
        );

        // Hexploration
        _rules["hexploration"] = RuleRecord.Create(
            "hexploration",
            "Hexploration",
            RuleCategory.Subsystems,
            RuleContentType.Article,
            "A subsystem for exploring large wilderness areas using hex-based maps.",
            @"**Hex Movement**: Travel between adjacent hexes takes set amounts of time.
**Exploration Activities**: Special actions for discovering landmarks and resources.
**Random Encounters**: Tables for generating wilderness encounters.
**Weather and Hazards**: Environmental challenges that affect travel.",
            Array.Empty<string>(),
            new[] { "wilderness", "exploration", "travel", "encounters" },
            new[] { "GMG-170" }
        );

        // Vehicles
        _rules["vehicles"] = RuleRecord.Create(
            "vehicles",
            "Vehicles",
            RuleCategory.Items,
            RuleContentType.Article,
            "Rules for operating and fighting with ships, wagons, and other large conveyances.",
            @"**Vehicle Statistics**: AC, Hardness, HP, Speed, and Piloting DC.
**Vehicle Actions**: Special activities for operating vehicles in encounters.
**Crew Positions**: Different roles for multiple characters on large vehicles.
**Vehicle Combat**: Rules for battles between or aboard vehicles.",
            Array.Empty<string>(),
            new[] { "ships", "transportation", "piloting", "crew" },
            new[] { "GMG-174" }
        );
    }

    private void LoadGMRules()
    {
        // Building Encounters
        _rules["building-encounters"] = RuleRecord.Create(
            "building-encounters",
            "Building Encounters",
            RuleCategory.GMTools,
            RuleContentType.Article,
            "Guidelines for creating balanced and engaging combat encounters.",
            @"**Encounter Budget**: Point system for balancing creature difficulty.
**Threat Assessment**: Adjusting encounters for party size and level.
**Terrain Features**: Using environment to enhance tactical complexity.
**Dynamic Elements**: Hazards, timers, and changing conditions.",
            Array.Empty<string>(),
            new[] { "balance", "budget", "difficulty", "terrain" },
            new[] { "GMG-46" }
        );

        // NPC Creation
        _rules["npc-creation"] = RuleRecord.Create(
            "npc-creation",
            "NPC Creation",
            RuleCategory.GMTools,
            RuleContentType.Article,
            "Guidelines for creating memorable and mechanically appropriate NPCs.",
            @"**NPC Roles**: Different types serve different narrative functions.
**Quick NPCs**: Simplified stat blocks for minor characters.
**Detailed NPCs**: Full character builds for major antagonists and allies.
**Motivations**: Giving NPCs clear goals and personality traits.",
            Array.Empty<string>(),
            new[] { "npcs", "characters", "antagonists", "allies" },
            new[] { "GMG-72" }
        );

        // Campaign Structure
        _rules["campaign-structure"] = RuleRecord.Create(
            "campaign-structure",
            "Campaign Structure",
            RuleCategory.GMTools,
            RuleContentType.Article,
            "Advice for planning and organizing long-term campaigns.",
            @"**Campaign Themes**: Choosing the tone and focus of the campaign.
**Story Arcs**: Breaking large campaigns into manageable chapters.
**Character Development**: Supporting player character growth and goals.
**World Building**: Creating consistent and believable settings.",
            Array.Empty<string>(),
            new[] { "planning", "themes", "arcs", "worldbuilding" },
            new[] { "GMG-36" }
        );

        // Hazards and Traps
        _rules["hazards-traps"] = RuleRecord.Create(
            "hazards-traps",
            "Hazards and Traps",
            RuleCategory.GMTools,
            RuleContentType.Article,
            "Environmental dangers and mechanical traps that challenge parties.",
            @"**Detection**: Finding hazards before they trigger.
**Disabling**: Safely neutralizing dangerous devices.
**Triggers**: Conditions that activate traps and hazards.
**Damage Types**: Physical, magical, and environmental effects.",
            Array.Empty<string>(),
            new[] { "traps", "environmental", "detection", "disable" },
            new[] { "GMG-76" }
        );
    }

    private async Task LoadRuleTables()
    {
        // DCs and Checks Table
        _tables["dcs-and-checks"] = RuleTable.Create(
            "dcs-and-checks",
            "Difficulty Classes by Category",
            new[] { "Difficulty", "DC", "Example" },
            new List<IReadOnlyList<string>>
            {
                new[] { "Trivial", "5", "Notice something large in plain sight" },
                new[] { "Very Easy", "10", "Climb a knotted rope" },
                new[] { "Easy", "15", "Leap a 3-foot gap" },
                new[] { "Moderate", "20", "Balance on unsteady ground" },
                new[] { "Hard", "25", "Swim against a current" },
                new[] { "Very Hard", "30", "Leap a 10-foot gap" },
                new[] { "Extremely Hard", "35", "Track across hard ground after rainfall" },
                new[] { "Nearly Impossible", "40", "Leap a 20-foot gap" }
            }
        );

        // Cover Table
        _tables["cover"] = RuleTable.Create(
            "cover",
            "Cover and Bonuses",
            new[] { "Cover", "AC Bonus", "Reflex Save Bonus", "Example" },
            new List<IReadOnlyList<string>>
            {
                new[] { "Lesser Cover", "+1 circumstance", "+1 circumstance", "A pillar" },
                new[] { "Standard Cover", "+2 circumstance", "+2 circumstance", "A big tree or wall" },
                new[] { "Greater Cover", "+4 circumstance", "+4 circumstance", "Around a corner or behind cover" }
            }
        );

        // Environmental Damage Table
        _tables["environmental-damage"] = RuleTable.Create(
            "environmental-damage",
            "Environmental Damage by Category",
            new[] { "Category", "Damage", "Example" },
            new List<IReadOnlyList<string>>
            {
                new[] { "Minor", "1d6 to 2d6", "Minor fire, light acid splash" },
                new[] { "Moderate", "3d6 to 5d6", "Lava flow, moderate cold" },
                new[] { "Major", "6d6 to 10d6", "Intense heat, strong acid" },
                new[] { "Massive", "11d6 to 20d6", "Volcano, immersion in acid" }
            }
        );

        // Vision and Concealment Table
        _tables["vision-concealment"] = RuleTable.Create(
            "vision-concealment",
            "Detection States",
            new[] { "State", "Can Target?", "Flat Check", "Conditions" },
            new List<IReadOnlyList<string>>
            {
                new[] { "Observed", "Yes", "None", "No penalties" },
                new[] { "Hidden", "Yes", "DC 11", "Attacker doesn't know exact location" },
                new[] { "Undetected", "No", "DC 11", "Must guess target square" },
                new[] { "Unnoticed", "No", "—", "Completely unaware of target" }
            }
        );

        await Task.CompletedTask;
    }

    private async Task LoadRuleFormulas()
    {
        // Basic Saves Formula
        _formulas["basic-saves"] = RuleFormula.Create(
            "basic-saves",
            "CritSuccess:0, Success:Half, Failure:Full, CritFailure:Double",
            "Basic saves: crit success = no damage, success = half damage, failure = full damage, crit failure = double damage"
        );

        // Falling Damage Formula
        _formulas["falling-damage"] = RuleFormula.Create(
            "falling-damage",
            "1d6 per 10ft, max 750 (250d6)",
            "Falling damage: 1d6 bludgeoning per 10 feet fallen, maximum 750 damage"
        );

        // Environmental Damage Formulas
        _formulas["environmental-damage"] = RuleFormula.Create(
            "environmental-damage",
            "Minor:1d6-2d6, Moderate:3d6-5d6, Major:6d6-10d6, Massive:11d6-20d6",
            "Environmental damage ranges by category severity"
        );

        await Task.CompletedTask;
    }
}