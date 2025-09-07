using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Infrastructure.Persistence.Repositories;

namespace PathfinderCampaignManager.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    private bool _disposed;

    // Repository instances
    private ISessionRepository? _sessions;
    private ICharacterRepository? _characters;
    private IEncounterRepository? _encounters;
    private IUserRepository? _users;
    private IRulesVersionRepository? _rulesVersions;
    private INpcMonsterRepository? _npcMonsters;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ISessionRepository Sessions =>
        _sessions ??= new SessionRepository(_context);

    public ICharacterRepository Characters =>
        _characters ??= new CharacterRepository(_context);

    public IEncounterRepository Encounters =>
        _encounters ??= new EncounterRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IRulesVersionRepository RulesVersions =>
        _rulesVersions ??= new RulesVersionRepository(_context);

    public INpcMonsterRepository NpcMonsters =>
        _npcMonsters ??= new NpcMonsterRepository(_context);

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (_repositories.TryGetValue(type, out var repository))
        {
            return (IRepository<T>)repository;
        }

        var newRepository = new Repository<T>(_context);
        _repositories[type] = newRepository;
        return newRepository;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                    entry.Reload();
                    break;
            }
        }
        await Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}