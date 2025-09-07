using MediatR;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Notes.Commands;

public record UpdateCharacterNoteCommand(
    Guid NoteId,
    Guid UpdatedBy,
    string Title,
    string Content,
    List<string>? Tags = null
) : IRequest<Result<CharacterNoteDto>>;

public class UpdateCharacterNoteCommandHandler : IRequestHandler<UpdateCharacterNoteCommand, Result<CharacterNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCharacterNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CharacterNoteDto>> Handle(UpdateCharacterNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _unitOfWork.Repository<CharacterNote>().GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null)
            {
                return Result.Failure<CharacterNoteDto>("Note not found");
            }

            var updater = await _unitOfWork.Repository<User>().GetByIdAsync(request.UpdatedBy, cancellationToken);
            if (updater == null)
            {
                return Result.Failure<CharacterNoteDto>("User not found");
            }

            // Check permissions
            var character = await _unitOfWork.Repository<Character>().GetByIdAsync(note.CharacterId, cancellationToken);
            if (character == null)
            {
                return Result.Failure<CharacterNoteDto>("Character not found");
            }
            
            var campaigns = await _unitOfWork.Repository<Campaign>()
                .FindAsync(c => c.Members.Any(m => m.UserId == character.OwnerUserId), cancellationToken);
            var campaign = campaigns.FirstOrDefault();
            
            bool isUserDM = campaign?.Members
                .Any(m => m.UserId == request.UpdatedBy && m.Role == CampaignRole.DM) ?? false;

            if (!note.CanBeEditedBy(request.UpdatedBy, isUserDM))
            {
                return Result.Failure<CharacterNoteDto>("You don't have permission to edit this note");
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Result.Failure<CharacterNoteDto>("Title is required");
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Result.Failure<CharacterNoteDto>("Content is required");
            }

            if (request.Title.Length > 100)
            {
                return Result.Failure<CharacterNoteDto>("Title must be 100 characters or less");
            }

            if (request.Content.Length > 5000)
            {
                return Result.Failure<CharacterNoteDto>("Content must be 5000 characters or less");
            }

            // Update note
            note.UpdateContent(request.Title.Trim(), request.Content.Trim(), request.UpdatedBy);

            if (request.Tags != null)
            {
                note.UpdateTags(request.Tags, request.UpdatedBy);
            }

            _unitOfWork.Repository<CharacterNote>().Update(note);
            await _unitOfWork.CommitAsync(cancellationToken);

            var author = await _unitOfWork.Repository<User>().GetByIdAsync(note.AuthorId, cancellationToken);
            
            var dto = new CharacterNoteDto
            {
                Id = note.Id,
                CharacterId = note.CharacterId,
                AuthorId = note.AuthorId,
                AuthorName = author?.DisplayName ?? "Unknown",
                Title = note.Title,
                Content = note.Content,
                Visibility = note.Visibility,
                Color = note.Color,
                IsPinned = note.IsPinned,
                Tags = note.GetTags(),
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Failure<CharacterNoteDto>($"Failed to update note: {ex.Message}");
        }
    }
}