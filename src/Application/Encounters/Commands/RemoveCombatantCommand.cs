using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;

namespace PathfinderCampaignManager.Application.Encounters.Commands;

public record RemoveCombatantCommand(Guid EncounterId, Guid CombatantId) : IRequest<Result>;

public class RemoveCombatantCommandHandler : IRequestHandler<RemoveCombatantCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCombatantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveCombatantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var encounter = await _unitOfWork.Encounters.GetByIdAsync(request.EncounterId, cancellationToken);
            if (encounter == null)
            {
                return Result.Failure("Encounter not found");
            }

            encounter.RemoveCombatant(request.CombatantId);
            _unitOfWork.Encounters.Update(encounter);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to remove combatant: {ex.Message}");
        }
    }
}