using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Entities;

public class User : BaseEntity
{
    private readonly List<SessionMember> _sessionMemberships = new();
    private readonly List<Character> _characters = new();

    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public string? DiscordId { get; private set; }
    public UserRole Role { get; private set; } = UserRole.Player;
    public bool IsActive { get; private set; } = true;
    public bool EmailVerified { get; private set; } = false;
    public DateTime? LastLoginAt { get; private set; }

    public IReadOnlyCollection<SessionMember> SessionMemberships => _sessionMemberships.AsReadOnly();
    public IReadOnlyCollection<Character> Characters => _characters.AsReadOnly();

    private User() { } // For EF Core

    public static User Create(string email, string username, string displayName)
    {
        var user = new User
        {
            Email = email,
            Username = username,
            DisplayName = displayName
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, email, username));
        return user;
    }

    public static User CreateWithPassword(string email, string username, string displayName, string passwordHash)
    {
        var user = new User
        {
            Email = email,
            Username = username,
            DisplayName = displayName,
            PasswordHash = passwordHash
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, email, username));
        return user;
    }

    public static User CreateWithDiscord(string email, string username, string displayName, string discordId)
    {
        var user = new User
        {
            Email = email,
            Username = username,
            DisplayName = displayName,
            DiscordId = discordId,
            EmailVerified = true // Discord users are pre-verified
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, email, username));
        return user;
    }

    public void UpdateProfile(string displayName)
    {
        DisplayName = displayName;
        Touch();
        RaiseDomainEvent(new UserProfileUpdatedEvent(Id, displayName));
    }

    public void UpdateRole(UserRole role, Guid updatedBy)
    {
        if (Role == role)
            return;

        var previousRole = Role;
        Role = role;
        Touch();
        RaiseDomainEvent(new UserRoleChangedEvent(Id, previousRole, role, updatedBy));
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        Touch();
    }

    public void Deactivate(Guid deactivatedBy)
    {
        IsActive = false;
        Touch();
        RaiseDomainEvent(new UserDeactivatedEvent(Id, deactivatedBy));
    }

    public void Activate(Guid activatedBy)
    {
        IsActive = true;
        Touch();
        RaiseDomainEvent(new UserActivatedEvent(Id, activatedBy));
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        Touch();
        RaiseDomainEvent(new UserPasswordChangedEvent(Id));
    }

    public void VerifyEmail()
    {
        if (EmailVerified)
            return;

        EmailVerified = true;
        Touch();
        RaiseDomainEvent(new UserEmailVerifiedEvent(Id, Email));
    }

    public bool HasPassword() => !string.IsNullOrEmpty(PasswordHash);
    public bool HasDiscordAuth() => !string.IsNullOrEmpty(DiscordId);
}

// Domain Events
public sealed record UserRegisteredEvent(Guid UserId, string Email, string Username) : DomainEvent;
public sealed record UserProfileUpdatedEvent(Guid UserId, string DisplayName) : DomainEvent;
public sealed record UserRoleChangedEvent(Guid UserId, UserRole PreviousRole, UserRole NewRole, Guid UpdatedBy) : DomainEvent;
public sealed record UserDeactivatedEvent(Guid UserId, Guid DeactivatedBy) : DomainEvent;
public sealed record UserActivatedEvent(Guid UserId, Guid ActivatedBy) : DomainEvent;
public sealed record UserPasswordChangedEvent(Guid UserId) : DomainEvent;
public sealed record UserEmailVerifiedEvent(Guid UserId, string Email) : DomainEvent;