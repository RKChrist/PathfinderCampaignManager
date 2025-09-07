using MediatR;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Notes.Commands;

public record DeleteCharacterNoteCommand(
    Guid NoteId,
    Guid DeletedBy
) : IRequest<Result>;

public class DeleteCharacterNoteCommandHandler : IRequestHandler<DeleteCharacterNoteCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCharacterNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCharacterNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _unitOfWork.Repository<CharacterNote>().GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null)
            {
                return Result.Failure("Note not found");
            }

            // Check permissions - only author or DMs can delete notes
            var character = await _unitOfWork.Repository<Character>().GetByIdAsync(note.CharacterId, cancellationToken);
            if (character == null)
            {
                return Result.Failure("Character not found");
            }
            
            var campaigns = await _unitOfWork.Repository<Campaign>()
                .FindAsync(c => c.Members.Any(m => m.UserId == character.OwnerUserId), cancellationToken);
            var campaign = campaigns.FirstOrDefault();
            
            bool isUserDM = campaign?.Members
                .Any(m => m.UserId == request.DeletedBy && m.Role == CampaignRole.DM) ?? false;

            if (!note.CanBeEditedBy(request.DeletedBy, isUserDM))
            {
                return Result.Failure("You don't have permission to delete this note");
            }

            _unitOfWork.Repository<CharacterNote>().Remove(note);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete note: {ex.Message}");
        }
    }
}