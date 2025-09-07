using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Application.NpcMonsters.Queries;

public record GetNpcMonsterByIdQuery(Guid Id) : IRequest<Result<NpcMonster>>;

public class GetNpcMonsterByIdQueryHandler : IRequestHandler<GetNpcMonsterByIdQuery, Result<NpcMonster>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNpcMonsterByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NpcMonster>> Handle(GetNpcMonsterByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var npcMonster = await _unitOfWork.NpcMonsters.GetByIdAsync(request.Id, cancellationToken);
            
            if (npcMonster == null)
            {
                return Result.Failure<NpcMonster>("NPC/Monster not found");
            }

            return Result.Success(npcMonster);
        }
        catch (Exception ex)
        {
            return Result.Failure<NpcMonster>($"Failed to retrieve NPC/Monster: {ex.Message}");
        }
    }
}