using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Application.NpcMonsters.Queries;

public record GetNpcMonstersBySessionQuery(Guid SessionId) : IRequest<Result<IEnumerable<NpcMonster>>>;

public class GetNpcMonstersBySessionQueryHandler : IRequestHandler<GetNpcMonstersBySessionQuery, Result<IEnumerable<NpcMonster>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNpcMonstersBySessionQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<NpcMonster>>> Handle(GetNpcMonstersBySessionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var npcMonsters = await _unitOfWork.NpcMonsters.GetBySessionIdAsync(request.SessionId, cancellationToken);
            return Result.Success(npcMonsters);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<NpcMonster>>($"Failed to retrieve NPC/Monsters by session: {ex.Message}");
        }
    }
}