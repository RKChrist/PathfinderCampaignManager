using Microsoft.Extensions.Logging;
using PathfinderCampaignManager.Application.Common.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Infrastructure.Services;

public class CharacterCalculator : ICharacterCalculator
{
    private readonly IRuleModuleRegistry _moduleRegistry;
    private readonly ICacheService _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CharacterCalculator> _logger;

    public CharacterCalculator(
        IRuleModuleRegistry moduleRegistry,
        ICacheService cache,
        IUnitOfWork unitOfWork,
        ILogger<CharacterCalculator> logger)
    {
        _moduleRegistry = moduleRegistry;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CalculatedCharacter>> CalculateAsync(Character character, Guid campaignId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get campaign variant rules
            var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(campaignId, cancellationToken);
            if (campaign == null)
                return Result.Failure<CalculatedCharacter>("Campaign not found");

            var variantRules = campaign.GetVariantRules();
            return await CalculateWithRules(character, variantRules, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate character {CharacterId} for campaign {CampaignId}", character.Id, campaignId);
            return Result.Failure<CalculatedCharacter>($"Failed to calculate character: {ex.Message}");
        }
    }

    public async Task<Result<CalculatedCharacter>> CalculateAsync(Character character, Dictionary<string, object> variantRules, CancellationToken cancellationToken = default)
    {
        try
        {
            var rules = ConvertToVariantRules(variantRules);
            return await CalculateWithRules(character, rules, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate character {CharacterId}", character.Id);
            return Result.Failure<CalculatedCharacter>($"Failed to calculate character: {ex.Message}");
        }
    }

    private async Task<Result<CalculatedCharacter>> CalculateWithRules(
        Character character, 
        Dictionary<VariantRuleType, bool> variantRules, 
        CancellationToken cancellationToken)
    {
        var cacheKey = $"calculated_character_{character.Id}_{string.Join(",", variantRules.Where(r => r.Value).Select(r => r.Key))}_{character.UpdatedAt:yyyyMMddHHmmss}";
        
        // Try cache first
        var cached = await _cache.GetAsync<CalculatedCharacter>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Using cached calculated character for {CharacterId}", character.Id);
            return Result.Success(cached);
        }

        var calculated = new CalculatedCharacter
        {
            Id = character.Id,
            Name = character.Name,
            Level = character.Level
        };

        // Initialize base stats
        InitializeBaseStats(character, calculated);

        // Get active rule modules
        var ruleDict = variantRules.ToDictionary(kvp => kvp.Key.ToString(), kvp => (object)kvp.Value);
        var modules = _moduleRegistry.GetActiveModules(ruleDict).OrderBy(m => m.Priority);

        // Apply rule modules in order
        foreach (var module in modules)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                module.OnScores(character, calculated);
                module.OnProficiency(character, calculated);
                module.OnFeats(character, calculated);
                module.OnSlots(character, calculated);
                module.OnEncumbrance(character, calculated);
                module.OnValidation(character, calculated);

                var duration = DateTime.UtcNow - startTime;
                if (duration.TotalMilliseconds > 50) // Log slow modules
                {
                    _logger.LogWarning("Rule module {ModuleName} took {Duration}ms to execute", module.Name, duration.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rule module {ModuleName} failed for character {CharacterId}", module.Name, character.Id);
                calculated.ValidationIssues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Category = "Rule Module",
                    Message = $"Rule module '{module.Name}' failed: {ex.Message}"
                });
            }
        }

        // Final calculations
        CalculateDerivedStats(calculated);

        // Cache result for 5 minutes
        await _cache.SetAsync(cacheKey, calculated, TimeSpan.FromMinutes(5), cancellationToken);

        return Result.Success(calculated);
    }

    private void InitializeBaseStats(Character character, CalculatedCharacter calculated)
    {
        // Initialize ability scores with base values
        calculated.AbilityScores = new Dictionary<string, int>
        {
            ["Strength"] = 10,
            ["Dexterity"] = 10,
            ["Constitution"] = 10,
            ["Intelligence"] = 10,
            ["Wisdom"] = 10,
            ["Charisma"] = 10
        };

        // Calculate modifiers
        calculated.AbilityModifiers = calculated.AbilityScores.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value - 10) / 2
        );

        // Initialize proficiencies
        calculated.Proficiencies = new Dictionary<string, ProficiencyLevel>();
        calculated.ProficiencyBonuses = new Dictionary<string, int>();

        // Initialize feat slots
        calculated.FeatSlots = new Dictionary<string, List<FeatSlot>>();
        calculated.AvailableFeats = new List<string>();

        // Initialize encumbrance
        calculated.BulkLimit = 5 + calculated.AbilityModifiers["Strength"];
        calculated.CurrentBulk = 0;
        calculated.IsEncumbered = false;

        // Initialize validation
        calculated.ValidationIssues = new List<ValidationIssue>();
    }

    private void CalculateDerivedStats(CalculatedCharacter calculated)
    {
        // Calculate AC (simplified)
        var acBonus = calculated.ProficiencyBonuses.GetValueOrDefault("Armor", 0);
        calculated.ArmorClass = 10 + calculated.AbilityModifiers["Dexterity"] + acBonus;

        // Calculate HP (simplified)
        var classHP = calculated.Level * 8; // Assume 8 HP per level base
        var conBonus = calculated.AbilityModifiers["Constitution"] * calculated.Level;
        calculated.HitPoints = classHP + conBonus;

        // Calculate Initiative
        calculated.Initiative = calculated.AbilityModifiers["Dexterity"];

        // Check encumbrance
        calculated.IsEncumbered = calculated.CurrentBulk > calculated.BulkLimit;
    }

    private Dictionary<VariantRuleType, bool> ConvertToVariantRules(Dictionary<string, object> rules)
    {
        var result = new Dictionary<VariantRuleType, bool>();
        foreach (var rule in rules)
        {
            if (Enum.TryParse<VariantRuleType>(rule.Key, out var ruleType) && rule.Value is bool enabled)
            {
                result[ruleType] = enabled;
            }
        }
        return result;
    }
}

public class RuleModuleRegistry : IRuleModuleRegistry
{
    private readonly List<IRuleModule> _modules = new();
    private readonly ILogger<RuleModuleRegistry> _logger;

    public RuleModuleRegistry(ILogger<RuleModuleRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterModule<T>(T module) where T : class, IRuleModule
    {
        _modules.Add(module);
        _logger.LogInformation("Registered rule module: {ModuleName}", module.Name);
    }

    public void RegisterModules(IEnumerable<IRuleModule> modules)
    {
        foreach (var module in modules)
        {
            _modules.Add(module);
            _logger.LogInformation("Registered rule module: {ModuleName}", module.Name);
        }
    }

    public IEnumerable<IRuleModule> GetActiveModules(Dictionary<string, object> variantRules)
    {
        return _modules.Where(m => 
        {
            var ruleKey = m.RuleType.ToString();
            return variantRules.ContainsKey(ruleKey) && 
                   variantRules[ruleKey] is bool enabled && 
                   enabled;
        });
    }

    public IRuleModule? GetModule(string name)
    {
        return _modules.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public Task<Result<string>> ValidateModuleChain(IEnumerable<IRuleModule> modules)
    {
        try
        {
            var moduleList = modules.OrderBy(m => m.Priority).ToList();
            var conflicts = new List<string>();

            // Check for conflicting modules
            var priorities = moduleList.GroupBy(m => m.Priority).Where(g => g.Count() > 1).ToList();
            if (priorities.Any())
            {
                conflicts.AddRange(priorities.Select(p => $"Priority conflict at {p.Key}: {string.Join(", ", p.Select(m => m.Name))}"));
            }

            if (conflicts.Any())
            {
                return Task.FromResult(Result.Failure<string>($"Module chain validation failed: {string.Join("; ", conflicts)}"));
            }

            return Task.FromResult(Result.Success("Module chain is valid"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate module chain");
            return Task.FromResult(Result.Failure<string>($"Validation failed: {ex.Message}"));
        }
    }
}