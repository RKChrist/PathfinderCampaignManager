using MediatR;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Application.Notes.Commands;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Notes.Queries;

public record GetCharacterNotesQuery(
    Guid CharacterId,
    Guid UserId,
    NoteVisibility? VisibilityFilter = null
) : IRequest<Result<List<CharacterNoteDto>>>;

public class GetCharacterNotesQueryHandler : IRequestHandler<GetCharacterNotesQuery, Result<List<CharacterNoteDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCharacterNotesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<CharacterNoteDto>>> Handle(GetCharacterNotesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify character exists
            var character = await _unitOfWork.Repository<Character>().GetByIdAsync(request.CharacterId, cancellationToken);
            if (character == null)
            {
                return Result.Failure<List<CharacterNoteDto>>("Character not found");
            }

            // Determine user permissions
            var campaigns = await _unitOfWork.Repository<Campaign>()
                .FindAsync(c => c.Members.Any(m => m.UserId == character.OwnerUserId), cancellationToken);
            var campaign = campaigns.FirstOrDefault();
            
            bool isUserDM = campaign?.Members
                .Any(m => m.UserId == request.UserId && m.Role == CampaignRole.DM) ?? false;
            
            bool isCharacterOwner = character.OwnerUserId == request.UserId;

            // Get all notes for the character
            var allNotes = await _unitOfWork.Repository<CharacterNote>()
                .FindAsync(n => n.CharacterId == request.CharacterId, cancellationToken);

            // Filter based on visibility permissions
            var visibleNotes = allNotes.Where(note => note.CanBeViewedBy(request.UserId, isUserDM, isCharacterOwner));

            // Apply additional visibility filter if specified
            if (request.VisibilityFilter.HasValue)
            {
                visibleNotes = visibleNotes.Where(n => n.Visibility == request.VisibilityFilter.Value);
            }

            // Get author information for all notes
            var authorIds = visibleNotes.Select(n => n.AuthorId).Distinct().ToList();
            var authors = await _unitOfWork.Repository<User>()
                .FindAsync(u => authorIds.Contains(u.Id), cancellationToken);
            var authorDict = authors.ToDictionary(a => a.Id, a => a.DisplayName);

            // Convert to DTOs
            var noteDtos = visibleNotes.Select(note => new CharacterNoteDto
            {
                Id = note.Id,
                CharacterId = note.CharacterId,
                AuthorId = note.AuthorId,
                AuthorName = authorDict.GetValueOrDefault(note.AuthorId, "Unknown"),
                Title = note.Title,
                Content = note.Content,
                Visibility = note.Visibility,
                Color = note.Color,
                IsPinned = note.IsPinned,
                Tags = note.GetTags(),
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            }).ToList();

            return Result.Success(noteDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<CharacterNoteDto>>($"Failed to retrieve notes: {ex.Message}");
        }
    }
}