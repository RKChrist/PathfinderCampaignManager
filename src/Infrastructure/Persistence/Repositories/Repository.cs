using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.CountAsync(cancellationToken);

        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}

public class SessionRepository : Repository<Session>, ISessionRepository
{
    public SessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Session?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Members)
            .Include(s => s.Characters)
            .Include(s => s.Encounters)
            .FirstOrDefaultAsync(s => s.Code.Value == code.ToUpper(), cancellationToken);
    }

    public async Task<IEnumerable<Session>> GetByDMUserIdAsync(Guid dmUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.DMUserId == dmUserId)
            .Include(s => s.Members)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .Include(s => s.Members)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}

public class CharacterRepository : Repository<Character>, ICharacterRepository
{
    public CharacterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Character>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.OwnerUserId == ownerId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Character>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.SessionId == sessionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Character?> GetWithAuditLogsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.AuditLogs)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}

public class EncounterRepository : Repository<Encounter>, IEncounterRepository
{
    public EncounterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Encounter>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.SessionId == sessionId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Encounter?> GetWithCombatantsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Combatants)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Encounter>> GetActiveEncountersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Include(e => e.Combatants)
            .ToListAsync(cancellationToken);
    }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(Domain.Enums.UserRole role, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(u => u.Role == role).ToListAsync(cancellationToken);
    }
}

public class RulesVersionRepository : Repository<RulesVersion>, IRulesVersionRepository
{
    public RulesVersionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RulesVersion?> GetByVersionAsync(string version, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(rv => rv.Version == version, cancellationToken);
    }

    public async Task<RulesVersion?> GetLatestPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(rv => rv.Status == Domain.Enums.RulesVersionStatus.Published)
            .OrderByDescending(rv => rv.PublishedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<RulesVersion>> GetPublishedVersionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(rv => rv.Status == Domain.Enums.RulesVersionStatus.Published)
            .OrderByDescending(rv => rv.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<RulesVersion?> GetWithClassesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(rv => rv.Classes)
            .FirstOrDefaultAsync(rv => rv.Id == id, cancellationToken);
    }
}

public class NpcMonsterRepository : Repository<NpcMonster>, INpcMonsterRepository
{
    public NpcMonsterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<NpcMonster>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(nm => nm.OwnerUserId == ownerId)
            .OrderBy(nm => nm.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NpcMonster>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(nm => nm.SessionId == sessionId)
            .OrderBy(nm => nm.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NpcMonster>> GetByTypeAsync(Domain.Enums.NpcMonsterType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(nm => nm.Type == type)
            .OrderBy(nm => nm.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NpcMonster>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(nm => nm.IsTemplate)
            .OrderBy(nm => nm.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NpcMonster>> GetLibraryNpcsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(nm => nm.OwnerUserId == null && nm.SessionId == null)
            .OrderBy(nm => nm.Name)
            .ToListAsync(cancellationToken);
    }
}