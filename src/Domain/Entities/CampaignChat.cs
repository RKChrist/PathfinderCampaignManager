namespace PathfinderCampaignManager.Domain.Entities;

public class CampaignChat : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public Guid UserId { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public CampaignChatType Type { get; private set; } = CampaignChatType.General;
    public string? Metadata { get; private set; } // JSON for additional data like dice rolls, character actions, etc.
    public bool IsSystemMessage { get; private set; }
    public DateTime Timestamp { get; private set; }

    // Navigation
    public Campaign Campaign { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private CampaignChat() { } // For EF Core

    public static CampaignChat Create(
        Guid campaignId,
        Guid userId,
        string message,
        CampaignChatType type = CampaignChatType.General,
        string? metadata = null,
        bool isSystemMessage = false)
    {
        var chat = new CampaignChat
        {
            CampaignId = campaignId,
            UserId = userId,
            Message = message,
            Type = type,
            Metadata = metadata,
            IsSystemMessage = isSystemMessage,
            Timestamp = DateTime.UtcNow
        };

        chat.RaiseDomainEvent(new CampaignChatCreatedEvent(
            campaignId, 
            userId, 
            message, 
            type, 
            chat.Id
        ));

        return chat;
    }

    public static CampaignChat CreateSystemMessage(
        Guid campaignId,
        string message,
        CampaignChatType type = CampaignChatType.System,
        string? metadata = null)
    {
        return Create(
            campaignId,
            Guid.Empty, // System messages use empty GUID
            message,
            type,
            metadata,
            isSystemMessage: true
        );
    }

    public static CampaignChat CreateDiceRoll(
        Guid campaignId,
        Guid userId,
        string rollDescription,
        string rollResult,
        bool isPrivate = false)
    {
        var metadata = System.Text.Json.JsonSerializer.Serialize(new
        {
            rollResult,
            isPrivate
        });

        return Create(
            campaignId,
            userId,
            rollDescription,
            CampaignChatType.DiceRoll,
            metadata
        );
    }
}

public enum CampaignChatType
{
    General = 1,
    System = 2,
    DiceRoll = 3,
    CharacterAction = 4,
    Combat = 5,
    OutOfCharacter = 6,
    DMOnly = 7
}

// Domain Events
public sealed record CampaignChatCreatedEvent(
    Guid CampaignId, 
    Guid UserId, 
    string Message, 
    CampaignChatType Type,
    Guid ChatId
) : DomainEvent;