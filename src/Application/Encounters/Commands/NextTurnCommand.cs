using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;

namespace PathfinderCampaignManager.Application.Encounters.Commands;

public record NextTurnCommand(Guid EncounterId) : IRequest<Result>;

public class NextTurnCommandHandler : IRequestHandler<NextTurnCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public NextTurnCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(NextTurnCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var encounter = await _unitOfWork.Encounters.GetByIdAsync(request.EncounterId, cancellationToken);
            if (encounter == null)
            {
                return Result.Failure("Encounter not found");
            }

            encounter.NextTurn();
            _unitOfWork.Encounters.Update(encounter);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to advance to next turn: {ex.Message}");
        }
    }
}