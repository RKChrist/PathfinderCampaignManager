using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Application.NpcMonsters.Commands;

public record CreateNpcMonsterCommand(
    string Name,
    NpcMonsterType Type,
    int Level,
    Guid? OwnerUserId = null,
    Guid? SessionId = null
) : IRequest<Result<Guid>>;

public class CreateNpcMonsterCommandHandler : IRequestHandler<CreateNpcMonsterCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateNpcMonsterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateNpcMonsterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var npcMonster = NpcMonster.Create(
                request.Name,
                request.Type,
                request.Level,
                request.OwnerUserId,
                request.SessionId
            );

            await _unitOfWork.NpcMonsters.AddAsync(npcMonster, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(npcMonster.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>($"Failed to create NPC/Monster: {ex.Message}");
        }
    }
}