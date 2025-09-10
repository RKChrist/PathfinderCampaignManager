using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Domain.Entities;
using System.Security.Claims;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId}/chat")]
[Authorize]
public class CampaignChatController : ControllerBase
{
    // In a real implementation, you would inject campaign and chat repositories
    private static readonly List<Campaign> _campaigns = CampaignController._campaigns;
    private static readonly List<CampaignChat> _chatMessages = new();

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignChatDto>>> GetChatHistory(
        Guid campaignId, 
        [FromQuery] int limit = 50,
        [FromQuery] DateTime? since = null)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == campaignId);

        if (campaign == null)
            return NotFound();

        if (!campaign.IsUserMember(userId) && campaign.DMUserId != userId)
            return Forbid();

        var query = _chatMessages
            .Where(c => c.CampaignId == campaignId);

        if (since.HasValue)
        {
            query = query.Where(c => c.Timestamp > since.Value);
        }

        // Filter out DM-only messages if user is not DM
        if (campaign.DMUserId != userId)
        {
            query = query.Where(c => c.Type != CampaignChatType.DMOnly);
        }

        var messages = query
            .OrderByDescending(c => c.Timestamp)
            .Take(limit)
            .OrderBy(c => c.Timestamp)
            .Select(MapToChatDto)
            .ToList();

        return Ok(messages);
    }

    [HttpPost]
    public async Task<ActionResult<CampaignChatDto>> SendMessage(
        Guid campaignId, 
        [FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == campaignId);

        if (campaign == null)
            return NotFound();

        if (!campaign.IsUserMember(userId) && campaign.DMUserId != userId)
            return Forbid();

        // Validate message type permissions
        if (request.Type == CampaignChatType.DMOnly && campaign.DMUserId != userId)
            return Forbid("Only DMs can send DM-only messages");

        var chatMessage = CampaignChat.Create(
            campaignId,
            userId,
            request.Message,
            request.Type,
            request.Metadata
        );

        _chatMessages.Add(chatMessage);

        // Update campaign activity
        campaign.UpdateActivity();

        var dto = MapToChatDto(chatMessage);
        
        // In a real implementation, you would broadcast this via SignalR
        // await _hubContext.Clients.Group($"Campaign-{campaignId}").SendAsync("NewChatMessage", dto);

        return Ok(dto);
    }

    [HttpPost("dice-roll")]
    public async Task<ActionResult<CampaignChatDto>> SendDiceRoll(
        Guid campaignId,
        [FromBody] DiceRollRequest request)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == campaignId);

        if (campaign == null)
            return NotFound();

        if (!campaign.IsUserMember(userId) && campaign.DMUserId != userId)
            return Forbid();

        // Simple dice rolling logic (in a real implementation, use a proper dice rolling library)
        var result = RollDice(request.DiceExpression);
        
        var chatMessage = CampaignChat.CreateDiceRoll(
            campaignId,
            userId,
            $"Rolling {request.DiceExpression}: {result.Description}",
            result.Total.ToString(),
            request.IsPrivate && campaign.DMUserId == userId
        );

        _chatMessages.Add(chatMessage);
        campaign.UpdateActivity();

        var dto = MapToChatDto(chatMessage);
        
        return Ok(dto);
    }

    [HttpPost("system-message")]
    public async Task<ActionResult<CampaignChatDto>> SendSystemMessage(
        Guid campaignId,
        [FromBody] SystemMessageRequest request)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == campaignId);

        if (campaign == null)
            return NotFound();

        if (campaign.DMUserId != userId)
            return Forbid("Only DMs can send system messages");

        var chatMessage = CampaignChat.CreateSystemMessage(
            campaignId,
            request.Message,
            request.Type
        );

        _chatMessages.Add(chatMessage);
        campaign.UpdateActivity();

        var dto = MapToChatDto(chatMessage);
        
        return Ok(dto);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new InvalidOperationException("User ID not found in token"));
    }

    private static CampaignChatDto MapToChatDto(CampaignChat chat)
    {
        return new CampaignChatDto
        {
            Id = chat.Id,
            CampaignId = chat.CampaignId,
            UserId = chat.UserId,
            UserName = chat.IsSystemMessage ? "System" : $"User-{chat.UserId.ToString()[..8]}",
            Message = chat.Message,
            Type = chat.Type,
            Metadata = chat.Metadata,
            IsSystemMessage = chat.IsSystemMessage,
            Timestamp = chat.Timestamp
        };
    }

    private static DiceRollResult RollDice(string expression)
    {
        // Simple dice rolling - in real implementation use proper parser
        var random = new Random();
        
        // Parse simple expressions like "1d20", "2d6+3", etc.
        var parts = expression.ToLower().Replace(" ", "").Split('+');
        var total = 0;
        var description = "";

        foreach (var part in parts)
        {
            if (part.Contains('d'))
            {
                var diceParts = part.Split('d');
                if (diceParts.Length == 2 && 
                    int.TryParse(diceParts[0], out var count) && 
                    int.TryParse(diceParts[1], out var sides))
                {
                    var rolls = new List<int>();
                    for (int i = 0; i < count; i++)
                    {
                        var roll = random.Next(1, sides + 1);
                        rolls.Add(roll);
                        total += roll;
                    }
                    description += $"{count}d{sides}({string.Join(",", rolls)})";
                }
            }
            else if (int.TryParse(part, out var modifier))
            {
                total += modifier;
                if (modifier > 0) description += $"+{modifier}";
                else description += modifier.ToString();
            }
        }

        return new DiceRollResult
        {
            Total = total,
            Description = $"{description} = {total}"
        };
    }
}

public class SendMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public CampaignChatType Type { get; set; } = CampaignChatType.General;
    public string? Metadata { get; set; }
}

public class DiceRollRequest
{
    public string DiceExpression { get; set; } = "1d20";
    public bool IsPrivate { get; set; } = false;
    public string? Description { get; set; }
}

public class SystemMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public CampaignChatType Type { get; set; } = CampaignChatType.System;
}

public class CampaignChatDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public CampaignChatType Type { get; set; }
    public string? Metadata { get; set; }
    public bool IsSystemMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DiceRollResult
{
    public int Total { get; set; }
    public string Description { get; set; } = string.Empty;
}