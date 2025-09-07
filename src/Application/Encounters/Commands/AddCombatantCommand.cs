using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;

namespace PathfinderCampaignManager.Application.Encounters.Commands;

public record AddCombatantCommand(
    Guid EncounterId,
    string Name,
    int Initiative,
    Guid? CharacterId = null,
    Guid? NpcMonsterId = null
) : IRequest<Result>;

public class AddCombatantCommandHandler : IRequestHandler<AddCombatantCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddCombatantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddCombatantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var encounter = await _unitOfWork.Encounters.GetByIdAsync(request.EncounterId, cancellationToken);
            if (encounter == null)
            {
                return Result.Failure("Encounter not found");
            }

            encounter.AddCombatant(
                request.Name,
                request.Initiative,
                request.CharacterId,
                request.NpcMonsterId
            );

            _unitOfWork.Encounters.Update(encounter);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to add combatant: {ex.Message}");
        }
    }
}