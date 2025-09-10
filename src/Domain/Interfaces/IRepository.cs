using System.Linq.Expressions;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}

public interface IReadOnlyRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}

// Specialized repository interfaces
public interface ISessionRepository : IRepository<Session>
{
    Task<Session?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Session>> GetByDMUserIdAsync(Guid dmUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Session>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
}

public interface ICharacterRepository : IRepository<Character>
{
    Task<IEnumerable<Character>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Character>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Character?> GetWithAuditLogsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IEncounterRepository : IRepository<Encounter>
{
    Task<IEnumerable<Encounter>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Encounter?> GetWithCombatantsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Encounter>> GetActiveEncountersAsync(CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
}

public interface IRulesVersionRepository : IRepository<RulesVersion>
{
    Task<RulesVersion?> GetByVersionAsync(string version, CancellationToken cancellationToken = default);
    Task<RulesVersion?> GetLatestPublishedAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RulesVersion>> GetPublishedVersionsAsync(CancellationToken cancellationToken = default);
    Task<RulesVersion?> GetWithClassesAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface INpcMonsterRepository : IRepository<NpcMonster>
{
    Task<IEnumerable<NpcMonster>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NpcMonster>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NpcMonster>> GetByTypeAsync(NpcMonsterType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<NpcMonster>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<NpcMonster>> GetLibraryNpcsAsync(CancellationToken cancellationToken = default);
}

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<Campaign?> GetByJoinTokenAsync(Guid joinToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<Campaign>> GetByDMUserIdAsync(Guid dmUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Campaign>> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Campaign?> GetWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Campaign?> GetWithSessionsAndCharactersAsync(Guid id, CancellationToken cancellationToken = default);
}