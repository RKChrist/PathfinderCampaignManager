using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Presentation.Shared.Models;

public enum CampaignChatMessageType
{
    Regular,
    System,
    DiceRoll,
    Whisper
}

public class CampaignChatDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public CampaignChatMessageType Type { get; set; }
    public bool IsPrivate { get; set; }
    public string? DiceResult { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static CampaignChatDto FromEntity(PathfinderCampaignManager.Domain.Entities.CampaignChat entity)
    {
        return new CampaignChatDto
        {
            Id = entity.Id,
            CampaignId = entity.CampaignId,
            AuthorId = entity.UserId,
            AuthorName = entity.User?.Username ?? "System",
            Content = entity.Message,
            Type = MapChatType(entity.Type),
            IsPrivate = entity.Type == PathfinderCampaignManager.Domain.Entities.CampaignChatType.DMOnly,
            DiceResult = ExtractDiceResultFromMetadata(entity.Metadata),
            CreatedAt = entity.Timestamp,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static CampaignChatMessageType MapChatType(PathfinderCampaignManager.Domain.Entities.CampaignChatType domainType)
    {
        return domainType switch
        {
            PathfinderCampaignManager.Domain.Entities.CampaignChatType.General => CampaignChatMessageType.Regular,
            PathfinderCampaignManager.Domain.Entities.CampaignChatType.System => CampaignChatMessageType.System,
            PathfinderCampaignManager.Domain.Entities.CampaignChatType.DiceRoll => CampaignChatMessageType.DiceRoll,
            PathfinderCampaignManager.Domain.Entities.CampaignChatType.DMOnly => CampaignChatMessageType.Whisper,
            _ => CampaignChatMessageType.Regular
        };
    }

    private static string? ExtractDiceResultFromMetadata(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return null;

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(metadata);
            if (document.RootElement.TryGetProperty("rollResult", out var result))
            {
                return result.GetString();
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return null;
    }
}