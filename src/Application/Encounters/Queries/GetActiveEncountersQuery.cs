using MediatR;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Application.Encounters.Queries;

public record GetActiveEncountersQuery() : IRequest<Result<IEnumerable<Encounter>>>;

public class GetActiveEncountersQueryHandler : IRequestHandler<GetActiveEncountersQuery, Result<IEnumerable<Encounter>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActiveEncountersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<Encounter>>> Handle(GetActiveEncountersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var encounters = await _unitOfWork.Encounters.GetActiveEncountersAsync(cancellationToken);
            return Result.Success(encounters);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Encounter>>($"Failed to retrieve active encounters: {ex.Message}");
        }
    }
}