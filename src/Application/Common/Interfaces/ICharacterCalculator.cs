using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Common.Interfaces;

public interface ICharacterCalculator
{
    Task<Result<CalculatedCharacter>> CalculateAsync(Character character, Guid campaignId, CancellationToken cancellationToken = default);
    Task<Result<CalculatedCharacter>> CalculateAsync(Character character, Dictionary<string, object> variantRules, CancellationToken cancellationToken = default);
}

public interface IRuleModuleRegistry
{
    void RegisterModule<T>(T module) where T : class, IRuleModule;
    void RegisterModules(IEnumerable<IRuleModule> modules);
    IEnumerable<IRuleModule> GetActiveModules(Dictionary<string, object> variantRules);
    IRuleModule? GetModule(string name);
    Task<Result<string>> ValidateModuleChain(IEnumerable<IRuleModule> modules);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<string> GetETagAsync(string key, CancellationToken cancellationToken = default);
}

public interface IIndexService
{
    Task<Result<RuleIndex>> GetRuleIndexAsync(CancellationToken cancellationToken = default);
    Task<Result<RuleIndex>> BuildIndexAsync(CancellationToken cancellationToken = default);
    Task InvalidateIndexAsync(CancellationToken cancellationToken = default);
}

public class RuleIndex
{
    public string Version { get; set; } = string.Empty;
    public string ETag { get; set; } = string.Empty;
    public Dictionary<string, List<RuleIndexEntry>> Traits { get; set; } = new();
    public Dictionary<int, List<RuleIndexEntry>> Levels { get; set; } = new();
    public Dictionary<string, List<RuleIndexEntry>> Sources { get; set; } = new();
    public Dictionary<string, List<RuleIndexEntry>> Categories { get; set; } = new();
    public List<RuleIndexEntry> All { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class RuleIndexEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> Traits { get; set; } = new();
    public string Rarity { get; set; } = "Common";
    public string Source { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public Dictionary<string, string> SearchTerms { get; set; } = new();
}