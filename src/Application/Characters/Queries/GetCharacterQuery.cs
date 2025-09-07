using MediatR;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Behaviors;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Errors;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Characters.Queries;

public record CharacterDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string ClassRef { get; set; } = string.Empty;
    public string AncestryRef { get; set; } = string.Empty;
    public string BackgroundRef { get; set; } = string.Empty;
    public CharacterVisibility Visibility { get; set; }
    public Guid? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record GetCharacterQuery(Guid CharacterId) : IRequest<Result<CharacterDto>>, IAuthorizedRequest
{
    public async Task<Result> AuthorizeAsync(ICurrentUserService currentUser, IAuthorizationService authService, CancellationToken cancellationToken)
    {
        var canView = await authService.CanViewCharacterAsync(CharacterId, cancellationToken);
        return canView ? Result.Success() : Result.Failure(CharacterErrors.AccessDenied);
    }
}

public class GetCharacterQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCharacterQuery, Result<CharacterDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CharacterDto>> Handle(GetCharacterQuery request, CancellationToken cancellationToken)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync(request.CharacterId, cancellationToken);

        if (character == null)
            return Result.Failure<CharacterDto>(CharacterErrors.NotFound);

        var characterDto = new CharacterDto
        {
            Id = character.Id,
            OwnerUserId = character.OwnerUserId,
            Name = character.Name,
            Level = character.Level,
            ClassRef = character.ClassRef,
            AncestryRef = character.AncestryRef,
            BackgroundRef = character.BackgroundRef,
            Visibility = character.Visibility,
            SessionId = character.SessionId,
            CreatedAt = character.CreatedAt,
            UpdatedAt = character.UpdatedAt
        };

        return Result.Success(characterDto);
    }
}