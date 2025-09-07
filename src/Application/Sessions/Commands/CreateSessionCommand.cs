using FluentValidation;
using MediatR;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Behaviors;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Sessions.Commands;

public record CreateSessionCommand(string Name, string? Description = null) : IRequest<Result<Guid>>, IAuthorizedRequest
{
    public async Task<Result> AuthorizeAsync(ICurrentUserService currentUser, IAuthorizationService authService, CancellationToken cancellationToken)
    {
        // Any authenticated user can create a session
        return Result.Success();
    }
}

public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Session name is required")
            .MaximumLength(100)
            .WithMessage("Session name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class CreateSessionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : IRequestHandler<CreateSessionCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Result<Guid>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var session = Session.Create(_currentUser.UserId, request.Name, request.Description);

            await _unitOfWork.Sessions.AddAsync(session, cancellationToken);

            return session.Id;
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(DomainError.From(ex));
        }
    }
}