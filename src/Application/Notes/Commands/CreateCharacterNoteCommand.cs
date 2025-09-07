using MediatR;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Notes.Commands;

public record CreateCharacterNoteCommand(
    Guid CharacterId,
    Guid AuthorId,
    string Title,
    string Content,
    NoteVisibility Visibility = NoteVisibility.Private,
    string? Color = null,
    List<string>? Tags = null
) : IRequest<Result<CharacterNoteDto>>;

public class CreateCharacterNoteCommandHandler : IRequestHandler<CreateCharacterNoteCommand, Result<CharacterNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCharacterNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CharacterNoteDto>> Handle(CreateCharacterNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that character exists
            var character = await _unitOfWork.Repository<Character>().GetByIdAsync(request.CharacterId, cancellationToken);
            if (character == null)
            {
                return Result.Failure<CharacterNoteDto>("Character not found");
            }

            // Validate that author exists
            var author = await _unitOfWork.Repository<User>().GetByIdAsync(request.AuthorId, cancellationToken);
            if (author == null)
            {
                return Result.Failure<CharacterNoteDto>("Author not found");
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

            // Create note
            var note = CharacterNote.Create(
                request.CharacterId,
                request.AuthorId,
                request.Title.Trim(),
                request.Content.Trim(),
                request.Visibility
            );

            // Set optional properties
            if (!string.IsNullOrWhiteSpace(request.Color))
            {
                note.UpdateAppearance(request.Color, false, 0, request.AuthorId);
            }

            if (request.Tags?.Any() == true)
            {
                note.UpdateTags(request.Tags, request.AuthorId);
            }

            await _unitOfWork.Repository<CharacterNote>().AddAsync(note, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            var dto = new CharacterNoteDto
            {
                Id = note.Id,
                CharacterId = note.CharacterId,
                AuthorId = note.AuthorId,
                AuthorName = author.DisplayName,
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
            return Result.Failure<CharacterNoteDto>($"Failed to create note: {ex.Message}");
        }
    }
}

public class CharacterNoteDto
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NoteVisibility Visibility { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}