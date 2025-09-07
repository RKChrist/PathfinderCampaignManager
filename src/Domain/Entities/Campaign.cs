using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Entities;

public class Campaign : BaseEntity
{
    private readonly List<Session> _sessions = new();
    private readonly List<CampaignMember> _members = new();

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid DMUserId { get; private set; }
    public Guid JoinToken { get; private set; }
    public string VariantRulesJson { get; private set; } = "{}";
    public string SettingsJson { get; private set; } = "{}";
    public bool IsActive { get; private set; } = true;
    public DateTime? LastActivityAt { get; private set; }

    // Navigation
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
    public IReadOnlyCollection<CampaignMember> Members => _members.AsReadOnly();
    public User DM { get; private set; } = null!;

    private Campaign() { } // For EF Core

    public static Campaign Create(string name, Guid dmUserId, string? description = null)
    {
        var campaign = new Campaign
        {
            Name = name,
            Description = description,
            DMUserId = dmUserId,
            JoinToken = Guid.NewGuid()
        };

        campaign.RaiseDomainEvent(new CampaignCreatedEvent(campaign.Id, name, dmUserId));
        return campaign;
    }

    public void UpdateDetails(string name, string? description = null)
    {
        Name = name;
        Description = description;
        Touch();
        RaiseDomainEvent(new CampaignUpdatedEvent(Id, "Details"));
    }

    public void UpdateVariantRules(Dictionary<VariantRuleType, bool> variantRules)
    {
        VariantRulesJson = System.Text.Json.JsonSerializer.Serialize(variantRules);
        Touch();
        RaiseDomainEvent(new CampaignVariantRulesUpdatedEvent(Id, variantRules.Keys.ToList()));
    }

    public Dictionary<VariantRuleType, bool> GetVariantRules()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<VariantRuleType, bool>>(VariantRulesJson) ?? new();
        }
        catch
        {
            return new Dictionary<VariantRuleType, bool>();
        }
    }

    public void AddMember(Guid userId, string alias, CampaignRole role = CampaignRole.Player)
    {
        if (_members.Any(m => m.UserId == userId))
            return;

        var member = new CampaignMember
        {
            CampaignId = Id,
            UserId = userId,
            Alias = alias,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        _members.Add(member);
        Touch();
        RaiseDomainEvent(new CampaignMemberAddedEvent(Id, userId, alias, role));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null) return;

        _members.Remove(member);
        Touch();
        RaiseDomainEvent(new CampaignMemberRemovedEvent(Id, userId));
    }

    public void RegenerateJoinToken()
    {
        JoinToken = Guid.NewGuid();
        Touch();
        RaiseDomainEvent(new CampaignJoinTokenRegeneratedEvent(Id, JoinToken));
    }

    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        Touch();
    }

    public bool CanUserJoin(Guid userId) => !_members.Any(m => m.UserId == userId);
    public bool IsUserDM(Guid userId) => DMUserId == userId;
    public bool IsUserMember(Guid userId) => _members.Any(m => m.UserId == userId) || DMUserId == userId;
}

public class CampaignMember : BaseEntity
{
    public Guid CampaignId { get; set; }
    public Guid UserId { get; set; }
    public string Alias { get; set; } = string.Empty;
    public CampaignRole Role { get; set; } = CampaignRole.Player;
    public DateTime JoinedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    // Navigation
    public Campaign Campaign { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum CampaignRole
{
    Player = 1,
    DM = 2
}

// Domain Events
public sealed record CampaignCreatedEvent(Guid CampaignId, string Name, Guid DMUserId) : DomainEvent;
public sealed record CampaignUpdatedEvent(Guid CampaignId, string UpdateType) : DomainEvent;
public sealed record CampaignVariantRulesUpdatedEvent(Guid CampaignId, List<VariantRuleType> ChangedRules) : DomainEvent;
public sealed record CampaignMemberAddedEvent(Guid CampaignId, Guid UserId, string Alias, CampaignRole Role) : DomainEvent;
public sealed record CampaignMemberRemovedEvent(Guid CampaignId, Guid UserId) : DomainEvent;
public sealed record CampaignJoinTokenRegeneratedEvent(Guid CampaignId, Guid NewToken) : DomainEvent;