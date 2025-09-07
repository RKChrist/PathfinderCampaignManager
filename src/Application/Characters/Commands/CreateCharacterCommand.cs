using FluentValidation;
using MediatR;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Behaviors;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Characters.Commands;

public record CreateCharacterCommand(
    string Name,
    string ClassRef,
    string AncestryRef,
    string BackgroundRef) : IRequest<Result<Guid>>, IAuthorizedRequest
{
    public async Task<Result> AuthorizeAsync(ICurrentUserService currentUser, IAuthorizationService authService, CancellationToken cancellationToken)
    {
        // Any authenticated user can create a character
        return Result.Success();
    }
}

public class CreateCharacterCommandValidator : AbstractValidator<CreateCharacterCommand>
{
    public CreateCharacterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Character name is required")
            .MaximumLength(50)
            .WithMessage("Character name cannot exceed 50 characters");

        RuleFor(x => x.ClassRef)
            .NotEmpty()
            .WithMessage("Class is required");

        RuleFor(x => x.AncestryRef)
            .NotEmpty()
            .WithMessage("Ancestry is required");

        RuleFor(x => x.BackgroundRef)
            .NotEmpty()
            .WithMessage("Background is required");
    }
}

public class CreateCharacterCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : IRequestHandler<CreateCharacterCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Result<Guid>> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Validate class, ancestry, and background refs exist in rules
            
            var character = Character.Create(
                _currentUser.UserId,
                request.Name,
                request.ClassRef,
                request.AncestryRef,
                request.BackgroundRef);

            await _unitOfWork.Characters.AddAsync(character, cancellationToken);

            return character.Id;
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(DomainError.From(ex));
        }
    }
}