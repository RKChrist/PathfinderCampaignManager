using MediatR;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Application.Notes.Commands;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Notes.Queries;

public record GetCharacterNoteByIdQuery(
    Guid NoteId,
    Guid UserId
) : IRequest<Result<CharacterNoteDto>>;

public class GetCharacterNoteByIdQueryHandler : IRequestHandler<GetCharacterNoteByIdQuery, Result<CharacterNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCharacterNoteByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CharacterNoteDto>> Handle(GetCharacterNoteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _unitOfWork.Repository<CharacterNote>().GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null)
            {
                return Result.Failure<CharacterNoteDto>("Note not found");
            }

            // Verify character exists
            var character = await _unitOfWork.Repository<Character>().GetByIdAsync(note.CharacterId, cancellationToken);
            if (character == null)
            {
                return Result.Failure<CharacterNoteDto>("Associated character not found");
            }

            // Determine user permissions
            var campaigns = await _unitOfWork.Repository<Campaign>()
                .FindAsync(c => c.Members.Any(m => m.UserId == character.OwnerUserId), cancellationToken);
            var campaign = campaigns.FirstOrDefault();
            
            bool isUserDM = campaign?.Members
                .Any(m => m.UserId == request.UserId && m.Role == CampaignRole.DM) ?? false;
            
            bool isCharacterOwner = character.OwnerUserId == request.UserId;

            // Check if user can view this note
            if (!note.CanBeViewedBy(request.UserId, isUserDM, isCharacterOwner))
            {
                return Result.Failure<CharacterNoteDto>("You don't have permission to view this note");
            }

            // Get author information
            var author = await _unitOfWork.Repository<User>().GetByIdAsync(note.AuthorId, cancellationToken);

            var noteDto = new CharacterNoteDto
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

            return Result.Success(noteDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<CharacterNoteDto>($"Failed to retrieve note: {ex.Message}");
        }
    }
}