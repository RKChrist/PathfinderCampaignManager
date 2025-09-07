using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;

namespace PathfinderCampaignManager.Application.NpcMonsters.Commands;

public record DeleteNpcMonsterCommand(Guid Id) : IRequest<Result>;

public class DeleteNpcMonsterCommandHandler : IRequestHandler<DeleteNpcMonsterCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNpcMonsterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteNpcMonsterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var npcMonster = await _unitOfWork.NpcMonsters.GetByIdAsync(request.Id, cancellationToken);
            if (npcMonster == null)
            {
                return Result.Failure("NPC/Monster not found");
            }

            _unitOfWork.NpcMonsters.Remove(npcMonster);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete NPC/Monster: {ex.Message}");
        }
    }
}