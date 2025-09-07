using MediatR;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Behaviors;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Sessions.Queries;

public record SessionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DMUserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int CharacterCount { get; set; }
    public int EncounterCount { get; set; }
}

public record GetSessionQuery(Guid SessionId) : IRequest<Result<SessionDto>>, IAuthorizedRequest
{
    public async Task<Result> AuthorizeAsync(ICurrentUserService currentUser, IAuthorizationService authService, CancellationToken cancellationToken)
    {
        var canAccess = await authService.IsSessionMemberAsync(SessionId, cancellationToken);
        return canAccess ? Result.Success() : Result.Failure(SessionErrors.AccessDenied);
    }
}

public class GetSessionQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetSessionQuery, Result<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<SessionDto>> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(request.SessionId, cancellationToken);

        if (session == null)
            return Result.Failure<SessionDto>(SessionErrors.NotFound);

        var sessionDto = new SessionDto
        {
            Id = session.Id,
            Code = session.Code.Value,
            Name = session.Name,
            Description = session.Description,
            DMUserId = session.DMUserId,
            IsActive = session.IsActive,
            CreatedAt = session.CreatedAt,
            MemberCount = session.Members.Count,
            CharacterCount = session.Characters.Count,
            EncounterCount = session.Encounters.Count
        };

        return Result.Success(sessionDto);
    }
}