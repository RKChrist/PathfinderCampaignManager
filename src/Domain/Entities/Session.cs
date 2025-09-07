using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.ValueObjects;

namespace PathfinderCampaignManager.Domain.Entities;

public class Session : BaseEntity
{
    private readonly List<SessionMember> _members = new();
    private readonly List<Character> _characters = new();
    private readonly List<Encounter> _encounters = new();

    public SessionCode Code { get; private set; } = null!;
    public Guid DMUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string SettingsJson { get; private set; } = "{}";
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<SessionMember> Members => _members.AsReadOnly();
    public IReadOnlyCollection<Character> Characters => _characters.AsReadOnly();
    public IReadOnlyCollection<Encounter> Encounters => _encounters.AsReadOnly();

    private Session() { } // For EF Core

    public static Session Create(Guid dmUserId, string name, string? description = null)
    {
        var session = new Session
        {
            Code = SessionCode.Generate(),
            DMUserId = dmUserId,
            Name = name,
            Description = description
        };

        // Add DM as first member
        session.AddMember(dmUserId, SessionMemberRole.DM, "DM");

        session.RaiseDomainEvent(new SessionCreatedEvent(session.Id, session.Code, dmUserId));
        return session;
    }

    public void AddMember(Guid userId, SessionMemberRole role, string alias)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User is already a member of this session");

        if (role == SessionMemberRole.DM && _members.Any(m => m.Role == SessionMemberRole.DM))
            throw new InvalidOperationException("Session already has a DM");

        var member = new SessionMember
        {
            SessionId = Id,
            UserId = userId,
            Role = role,
            Alias = alias,
            JoinedAt = DateTime.UtcNow
        };

        _members.Add(member);
        Touch();
        RaiseDomainEvent(new SessionMemberJoinedEvent(Id, userId, role, alias));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            return;

        if (member.Role == SessionMemberRole.DM)
            throw new InvalidOperationException("Cannot remove the DM from the session");

        _members.Remove(member);
        Touch();
        RaiseDomainEvent(new SessionMemberLeftEvent(Id, userId));
    }

    public void AddCharacter(Character character)
    {
        if (!_members.Any(m => m.UserId == character.OwnerUserId))
            throw new InvalidOperationException("Character owner must be a session member");

        character.AssignToSession(Id);
        _characters.Add(character);
        Touch();
    }

    public void UpdateSettings(string settingsJson)
    {
        SettingsJson = settingsJson;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
        RaiseDomainEvent(new SessionDeactivatedEvent(Id));
    }
}

public class SessionMember
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public SessionMemberRole Role { get; set; }
    public string Alias { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }

    public Session Session { get; set; } = null!;
}

// Domain Events
public sealed record SessionCreatedEvent(Guid SessionId, SessionCode Code, Guid DMUserId) : DomainEvent;
public sealed record SessionMemberJoinedEvent(Guid SessionId, Guid UserId, SessionMemberRole Role, string Alias) : DomainEvent;
public sealed record SessionMemberLeftEvent(Guid SessionId, Guid UserId) : DomainEvent;
public sealed record SessionDeactivatedEvent(Guid SessionId) : DomainEvent;