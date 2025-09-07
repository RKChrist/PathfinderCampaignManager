using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Entities;

public class Character : BaseEntity
{
    private readonly List<CharacterAuditLog> _auditLogs = new();

    public Guid OwnerUserId { get; private set; }
    public Guid? SessionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Level { get; private set; } = 1;
    public string ClassRef { get; private set; } = string.Empty;
    public string AncestryRef { get; private set; } = string.Empty;
    public string BackgroundRef { get; private set; } = string.Empty;
    public string AbilityScoresJson { get; private set; } = "{}";
    public string SkillsJson { get; private set; } = "{}";
    public string FeatsJson { get; private set; } = "{}";
    public string InventoryJson { get; private set; } = "{}";
    public string SpellsJson { get; private set; } = "{}";
    public string NotesJson { get; private set; } = "{}";
    public CharacterVisibility Visibility { get; private set; } = CharacterVisibility.Private;
    public bool IsTemplate { get; private set; } = false;

    public IReadOnlyCollection<CharacterAuditLog> AuditLogs => _auditLogs.AsReadOnly();
    public Session? Session { get; private set; }

    private Character() { } // For EF Core

    public static Character Create(Guid ownerUserId, string name, string classRef, string ancestryRef, string backgroundRef)
    {
        var character = new Character
        {
            OwnerUserId = ownerUserId,
            Name = name,
            ClassRef = classRef,
            AncestryRef = ancestryRef,
            BackgroundRef = backgroundRef
        };

        character.LogChange("Character created", ownerUserId);
        character.RaiseDomainEvent(new CharacterCreatedEvent(character.Id, ownerUserId, name));
        return character;
    }

    public void AssignToSession(Guid sessionId)
    {
        if (SessionId.HasValue)
            throw new InvalidOperationException("Character is already assigned to a session");

        SessionId = sessionId;
        Touch();
        LogChange($"Assigned to session {sessionId}", OwnerUserId);
        RaiseDomainEvent(new CharacterAssignedToSessionEvent(Id, sessionId));
    }

    public void RemoveFromSession(Guid removedByUserId)
    {
        if (!SessionId.HasValue)
            return;

        var previousSessionId = SessionId.Value;
        SessionId = null;
        Touch();
        LogChange($"Removed from session {previousSessionId}", removedByUserId);
        RaiseDomainEvent(new CharacterRemovedFromSessionEvent(Id, previousSessionId));
    }

    public void UpdateLevel(int newLevel, Guid updatedByUserId)
    {
        if (newLevel < 1 || newLevel > 20)
            throw new ArgumentException("Level must be between 1 and 20");

        var oldLevel = Level;
        Level = newLevel;
        Touch();
        LogChange($"Level changed from {oldLevel} to {newLevel}", updatedByUserId);
        RaiseDomainEvent(new CharacterLevelChangedEvent(Id, oldLevel, newLevel));
    }

    public void UpdateAbilityScores(string abilityScoresJson, Guid updatedByUserId)
    {
        AbilityScoresJson = abilityScoresJson;
        Touch();
        LogChange("Ability scores updated", updatedByUserId);
    }

    public void UpdateSkills(string skillsJson, Guid updatedByUserId)
    {
        SkillsJson = skillsJson;
        Touch();
        LogChange("Skills updated", updatedByUserId);
    }

    public void UpdateFeats(string featsJson, Guid updatedByUserId)
    {
        FeatsJson = featsJson;
        Touch();
        LogChange("Feats updated", updatedByUserId);
    }

    public void UpdateInventory(string inventoryJson, Guid updatedByUserId)
    {
        InventoryJson = inventoryJson;
        Touch();
        LogChange("Inventory updated", updatedByUserId);
    }

    public void UpdateSpells(string spellsJson, Guid updatedByUserId)
    {
        SpellsJson = spellsJson;
        Touch();
        LogChange("Spells updated", updatedByUserId);
    }

    public void UpdateNotes(string notesJson, Guid updatedByUserId)
    {
        NotesJson = notesJson;
        Touch();
        LogChange("Notes updated", updatedByUserId);
    }

    public void SetVisibility(CharacterVisibility visibility, Guid updatedByUserId)
    {
        Visibility = visibility;
        Touch();
        LogChange($"Visibility changed to {visibility}", updatedByUserId);
    }

    private void LogChange(string description, Guid userId)
    {
        var auditLog = new CharacterAuditLog
        {
            CharacterId = Id,
            UserId = userId,
            Description = description,
            Timestamp = DateTime.UtcNow
        };
        _auditLogs.Add(auditLog);
    }
}

public class CharacterAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? AdditionalData { get; set; }

    public Character Character { get; set; } = null!;
}

// Domain Events
public sealed record CharacterCreatedEvent(Guid CharacterId, Guid OwnerUserId, string Name) : DomainEvent;
public sealed record CharacterAssignedToSessionEvent(Guid CharacterId, Guid SessionId) : DomainEvent;
public sealed record CharacterRemovedFromSessionEvent(Guid CharacterId, Guid SessionId) : DomainEvent;
public sealed record CharacterLevelChangedEvent(Guid CharacterId, int OldLevel, int NewLevel) : DomainEvent;