using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Application.NpcMonsters.Queries;

public record GetLibraryNpcMonstersQuery() : IRequest<Result<IEnumerable<NpcMonster>>>;

public class GetLibraryNpcMonstersQueryHandler : IRequestHandler<GetLibraryNpcMonstersQuery, Result<IEnumerable<NpcMonster>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLibraryNpcMonstersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<NpcMonster>>> Handle(GetLibraryNpcMonstersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var npcMonsters = await _unitOfWork.NpcMonsters.GetLibraryNpcsAsync(cancellationToken);
            return Result.Success(npcMonsters);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<NpcMonster>>($"Failed to retrieve library NPC/Monsters: {ex.Message}");
        }
    }
}