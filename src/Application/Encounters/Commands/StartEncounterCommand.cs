using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;

namespace PathfinderCampaignManager.Application.Encounters.Commands;

public record StartEncounterCommand(Guid EncounterId) : IRequest<Result>;

public class StartEncounterCommandHandler : IRequestHandler<StartEncounterCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public StartEncounterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(StartEncounterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var encounter = await _unitOfWork.Encounters.GetByIdAsync(request.EncounterId, cancellationToken);
            if (encounter == null)
            {
                return Result.Failure("Encounter not found");
            }

            encounter.StartEncounter();
            _unitOfWork.Encounters.Update(encounter);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to start encounter: {ex.Message}");
        }
    }
}