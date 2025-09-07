using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    
    ISessionRepository Sessions { get; }
    ICharacterRepository Characters { get; }
    IEncounterRepository Encounters { get; }
    IUserRepository Users { get; }
    IRulesVersionRepository RulesVersions { get; }
    INpcMonsterRepository NpcMonsters { get; }
    IRepository<T> Repository<T>() where T : BaseEntity;
}