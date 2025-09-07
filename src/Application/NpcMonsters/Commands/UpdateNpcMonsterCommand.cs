using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Application.NpcMonsters.Commands;

public record UpdateNpcMonsterCommand(
    Guid Id,
    string Name,
    NpcMonsterType Type,
    int Level,
    string? Description = null,
    int? ArmorClass = null,
    int? HitPoints = null,
    int? Speed = null,
    bool? IsTemplate = null
) : IRequest<Result>;

public class UpdateNpcMonsterCommandHandler : IRequestHandler<UpdateNpcMonsterCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNpcMonsterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateNpcMonsterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var npcMonster = await _unitOfWork.NpcMonsters.GetByIdAsync(request.Id, cancellationToken);
            if (npcMonster == null)
            {
                return Result.Failure("NPC/Monster not found");
            }

            npcMonster.UpdateDetails(
                request.Name,
                request.Type,
                request.Level,
                request.Description,
                request.ArmorClass,
                request.HitPoints,
                request.Speed,
                request.IsTemplate
            );

            _unitOfWork.NpcMonsters.Update(npcMonster);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update NPC/Monster: {ex.Message}");
        }
    }
}