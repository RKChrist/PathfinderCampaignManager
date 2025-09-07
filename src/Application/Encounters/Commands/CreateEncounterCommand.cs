using FluentValidation;
using MediatR;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Behaviors;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Encounters.Commands;

public record CreateEncounterCommand(
    Guid SessionId,
    string Name,
    string? Description = null) : IRequest<Result<Guid>>, IAuthorizedRequest
{
    public async Task<Result> AuthorizeAsync(ICurrentUserService currentUser, IAuthorizationService authService, CancellationToken cancellationToken)
    {
        var isDM = await authService.IsDMOfSessionAsync(SessionId, cancellationToken);
        return isDM ? Result.Success() : Result.Failure(AuthorizationErrors.Forbidden);
    }
}

public class CreateEncounterCommandValidator : AbstractValidator<CreateEncounterCommand>
{
    public CreateEncounterCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("Session ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Encounter name is required")
            .MaximumLength(100)
            .WithMessage("Encounter name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class CreateEncounterCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateEncounterCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(CreateEncounterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify session exists
            var session = await _unitOfWork.Sessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
                return Result.Failure<Guid>(SessionErrors.NotFound);

            var encounter = Encounter.Create(request.SessionId, request.Name, request.Description);

            await _unitOfWork.Encounters.AddAsync(encounter, cancellationToken);

            return encounter.Id;
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(DomainError.From(ex));
        }
    }
}