using MediatR;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Behaviors;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Encounters.Queries;

public record GetEncountersBySessionQuery(Guid SessionId) : IRequest<Result<IEnumerable<Encounter>>>, IAuthorizedRequest
{
    public async Task<Result> AuthorizeAsync(ICurrentUserService currentUser, IAuthorizationService authService, CancellationToken cancellationToken)
    {
        var isDM = await authService.IsDMOfSessionAsync(SessionId, cancellationToken);
        return isDM ? Result.Success() : Result.Failure(AuthorizationErrors.Forbidden);
    }
}

public class GetEncountersBySessionQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetEncountersBySessionQuery, Result<IEnumerable<Encounter>>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<IEnumerable<Encounter>>> Handle(GetEncountersBySessionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var encounters = await _unitOfWork.Encounters.FindAsync(
                e => e.SessionId == request.SessionId, 
                cancellationToken);

            return encounters.ToList();
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Encounter>>(DomainError.From(ex));
        }
    }
}