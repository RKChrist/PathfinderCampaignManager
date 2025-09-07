using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Application.Abstractions;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string Username { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}

public interface IAuthorizationService
{
    Task<bool> CanViewCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task<bool> CanEditCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);
    Task<bool> IsDMOfSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> IsSessionMemberAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> IsAdminAsync(CancellationToken cancellationToken = default);
    Task<bool> CanManageEncounterAsync(Guid encounterId, CancellationToken cancellationToken = default);
}

public interface IRulesProvider
{
    Task<T?> GetRuleAsync<T>(string ruleType, string ruleId, CancellationToken cancellationToken = default) where T : class;
    Task<IEnumerable<T>> GetRulesAsync<T>(string ruleType, CancellationToken cancellationToken = default) where T : class;
    Task<string?> GetLatestVersionAsync(CancellationToken cancellationToken = default);
}