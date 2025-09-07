using MediatR;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Notes.Commands;

public record UpdateCharacterNoteAppearanceCommand(
    Guid NoteId,
    string? Color,
    bool? IsPinned,
    int? SortOrder,
    Guid UpdatedBy
) : IRequest<Result>;

public class UpdateCharacterNoteAppearanceCommandHandler : IRequestHandler<UpdateCharacterNoteAppearanceCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    
    private static readonly HashSet<string> ValidColors = new()
    {
        "#fef3c7", "#dbeafe", "#d1fae5", "#fce7f3", 
        "#e9d5ff", "#fed7aa", "#f3f4f6"
    };

    public UpdateCharacterNoteAppearanceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCharacterNoteAppearanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _unitOfWork.Repository<CharacterNote>().GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null)
            {
                return Result.Failure("Note not found");
            }

            // Check permissions
            var character = await _unitOfWork.Repository<Character>().GetByIdAsync(note.CharacterId, cancellationToken);
            if (character == null)
            {
                return Result.Failure("Character not found");
            }
            
            var campaigns = await _unitOfWork.Repository<Campaign>()
                .FindAsync(c => c.Members.Any(m => m.UserId == character.OwnerUserId), cancellationToken);
            var campaign = campaigns.FirstOrDefault();
            
            bool isUserDM = campaign?.Members
                .Any(m => m.UserId == request.UpdatedBy && m.Role == CampaignRole.DM) ?? false;

            if (!note.CanBeEditedBy(request.UpdatedBy, isUserDM))
            {
                return Result.Failure("You don't have permission to update this note");
            }

            // Validate color if provided
            if (!string.IsNullOrEmpty(request.Color) && !ValidColors.Contains(request.Color))
            {
                return Result.Failure("Invalid color selection");
            }

            // Validate sort order if provided
            if (request.SortOrder.HasValue && (request.SortOrder < 0 || request.SortOrder > 1000))
            {
                return Result.Failure("Sort order must be between 0 and 1000");
            }

            // Update appearance
            var color = request.Color ?? note.Color;
            var isPinned = request.IsPinned ?? note.IsPinned;
            var sortOrder = request.SortOrder ?? note.SortOrder;

            note.UpdateAppearance(color, isPinned, sortOrder, request.UpdatedBy);

            _unitOfWork.Repository<CharacterNote>().Update(note);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update note appearance: {ex.Message}");
        }
    }
}