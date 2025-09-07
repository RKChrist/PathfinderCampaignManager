using MediatR;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Behaviors;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Encounters.Queries;

public record GetEncounterByIdQuery(Guid Id) : IRequest<Result<Encounter>>, IAuthorizedRequest
{
    public async Task<Result> AuthorizeAsync(ICurrentUserService currentUser, IAuthorizationService authService, CancellationToken cancellationToken)
    {
        var canManage = await authService.CanManageEncounterAsync(Id, cancellationToken);
        return canManage ? Result.Success() : Result.Failure(AuthorizationErrors.Forbidden);
    }
}

public class GetEncounterByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetEncounterByIdQuery, Result<Encounter>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Encounter>> Handle(GetEncounterByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var encounter = await _unitOfWork.Encounters.GetByIdAsync(request.Id, cancellationToken);
            if (encounter == null)
                return Result.Failure<Encounter>(EncounterErrors.NotFound);

            return encounter;
        }
        catch (Exception ex)
        {
            return Result.Failure<Encounter>(DomainError.From(ex));
        }
    }
}