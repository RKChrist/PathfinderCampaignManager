using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Application.NpcMonsters.Queries;

public record GetNpcMonstersByOwnerQuery(Guid OwnerUserId) : IRequest<Result<IEnumerable<NpcMonster>>>;

public class GetNpcMonstersByOwnerQueryHandler : IRequestHandler<GetNpcMonstersByOwnerQuery, Result<IEnumerable<NpcMonster>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNpcMonstersByOwnerQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<NpcMonster>>> Handle(GetNpcMonstersByOwnerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var npcMonsters = await _unitOfWork.NpcMonsters.GetByOwnerIdAsync(request.OwnerUserId, cancellationToken);
            return Result.Success(npcMonsters);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<NpcMonster>>($"Failed to retrieve NPC/Monsters by owner: {ex.Message}");
        }
    }
}