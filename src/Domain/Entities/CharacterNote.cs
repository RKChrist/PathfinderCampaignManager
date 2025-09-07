using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Entities;

public class CharacterNote : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public NoteVisibility Visibility { get; private set; } = NoteVisibility.Private;
    public string TagsJson { get; private set; } = "[]";
    public string Color { get; private set; } = "#fef3c7"; // Default yellow
    public int SortOrder { get; private set; } = 0;
    public bool IsPinned { get; private set; } = false;

    // Navigation
    public Character Character { get; private set; } = null!;
    public User Author { get; private set; } = null!;

    private CharacterNote() { } // For EF Core

    public static CharacterNote Create(
        Guid characterId,
        Guid authorId,
        string title,
        string content,
        NoteVisibility visibility = NoteVisibility.Private)
    {
        var note = new CharacterNote
        {
            CharacterId = characterId,
            AuthorId = authorId,
            Title = title,
            Content = content,
            Visibility = visibility
        };

        note.RaiseDomainEvent(new CharacterNoteCreatedEvent(note.Id, characterId, authorId, visibility));
        return note;
    }

    public void UpdateContent(string title, string content, Guid updatedBy)
    {
        Title = title;
        Content = content;
        Touch();
        RaiseDomainEvent(new CharacterNoteUpdatedEvent(Id, "Content", updatedBy));
    }

    public void ChangeVisibility(NoteVisibility visibility, Guid changedBy)
    {
        var oldVisibility = Visibility;
        Visibility = visibility;
        Touch();
        RaiseDomainEvent(new CharacterNoteVisibilityChangedEvent(Id, oldVisibility, visibility, changedBy));
    }

    public void UpdateTags(List<string> tags, Guid updatedBy)
    {
        TagsJson = System.Text.Json.JsonSerializer.Serialize(tags);
        Touch();
        RaiseDomainEvent(new CharacterNoteUpdatedEvent(Id, "Tags", updatedBy));
    }

    public List<string> GetTags()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(TagsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public void UpdateAppearance(string color, bool isPinned, int sortOrder, Guid updatedBy)
    {
        Color = color;
        IsPinned = isPinned;
        SortOrder = sortOrder;
        Touch();
        RaiseDomainEvent(new CharacterNoteUpdatedEvent(Id, "Appearance", updatedBy));
    }

    public bool CanBeViewedBy(Guid userId, bool isUserDM, bool isCharacterOwner)
    {
        return Visibility switch
        {
            NoteVisibility.Private => AuthorId == userId,
            NoteVisibility.Shared => isCharacterOwner || isUserDM || AuthorId == userId,
            NoteVisibility.DMOnly => isUserDM,
            _ => false
        };
    }

    public bool CanBeEditedBy(Guid userId, bool isUserDM)
    {
        return AuthorId == userId || (isUserDM && Visibility != NoteVisibility.Private);
    }
}


// Domain Events
public sealed record CharacterNoteCreatedEvent(
    Guid NoteId,
    Guid CharacterId,
    Guid AuthorId,
    NoteVisibility Visibility) : DomainEvent;

public sealed record CharacterNoteUpdatedEvent(
    Guid NoteId,
    string UpdateType,
    Guid UpdatedBy) : DomainEvent;

public sealed record CharacterNoteVisibilityChangedEvent(
    Guid NoteId,
    NoteVisibility OldVisibility,
    NoteVisibility NewVisibility,
    Guid ChangedBy) : DomainEvent;