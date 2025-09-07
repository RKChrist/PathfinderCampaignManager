using MediatR;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Notes.Commands;

public record ChangeCharacterNoteVisibilityCommand(
    Guid NoteId,
    NoteVisibility NewVisibility,
    Guid ChangedBy
) : IRequest<Result>;

public class ChangeCharacterNoteVisibilityCommandHandler : IRequestHandler<ChangeCharacterNoteVisibilityCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangeCharacterNoteVisibilityCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeCharacterNoteVisibilityCommand request, CancellationToken cancellationToken)
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
                .Any(m => m.UserId == request.ChangedBy && m.Role == CampaignRole.DM) ?? false;

            if (!note.CanBeEditedBy(request.ChangedBy, isUserDM))
            {
                return Result.Failure("You don't have permission to change this note's visibility");
            }

            // Additional validation for DM-only notes
            if (request.NewVisibility == NoteVisibility.DMOnly && !isUserDM)
            {
                return Result.Failure("Only DMs can create DM-only notes");
            }

            note.ChangeVisibility(request.NewVisibility, request.ChangedBy);

            _unitOfWork.Repository<CharacterNote>().Update(note);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to change note visibility: {ex.Message}");
        }
    }
}