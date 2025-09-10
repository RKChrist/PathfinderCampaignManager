using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using System.Security.Claims;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CampaignController : ControllerBase
{
    // In a real implementation, you would inject your campaign repository/service
    public static readonly List<Campaign> _campaigns = new();

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignDto>>> GetCampaigns()
    {
        var userId = GetCurrentUserId();
        
        // Get campaigns where user is DM or member
        var userCampaigns = _campaigns
            .Where(c => c.DMUserId == userId || c.IsUserMember(userId))
            .Select(MapToCampaignDto)
            .ToList();

        return Ok(userCampaigns);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CampaignDto>> GetCampaign(Guid id)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == id);

        if (campaign == null)
            return NotFound();

        if (!campaign.IsUserMember(userId) && campaign.DMUserId != userId)
            return Forbid();

        return Ok(MapToCampaignDto(campaign));
    }

    [HttpPost]
    public async Task<ActionResult<CampaignDto>> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        var userId = GetCurrentUserId();
        
        var campaign = Campaign.Create(
            name: request.Name,
            dmUserId: userId,
            description: request.Description
        );

        // Set up variant rules if any are enabled
        var variantRules = new Dictionary<VariantRuleType, bool>();
        if (request.UseFreeArchetype) variantRules[VariantRuleType.FreeArchetype] = true;
        if (request.UseDualClass) variantRules[VariantRuleType.DualClass] = true;
        if (request.UseProficiencyWithoutLevel) variantRules[VariantRuleType.ProficiencyWithoutLevel] = true;
        if (request.UseAutomaticBonusProgression) variantRules[VariantRuleType.AutomaticBonusProgression] = true;
        if (request.UseGradualAbilityBoosts) variantRules[VariantRuleType.GradualAbilityBoosts] = true;
        
        if (variantRules.Any())
        {
            campaign.UpdateVariantRules(variantRules);
        }

        _campaigns.Add(campaign);

        return CreatedAtAction(
            nameof(GetCampaign),
            new { id = campaign.Id },
            MapToCampaignDto(campaign)
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CampaignDto>> UpdateCampaign(Guid id, [FromBody] UpdateCampaignRequest request)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == id);

        if (campaign == null)
            return NotFound();

        if (campaign.DMUserId != userId)
            return Forbid("Only the DM can update campaign details");

        campaign.UpdateDetails(request.Name, request.Description);

        return Ok(MapToCampaignDto(campaign));
    }

    [HttpPost("{id}/join")]
    public async Task<ActionResult> JoinCampaign(Guid id, [FromBody] JoinCampaignRequest request)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == id || c.JoinToken == id);

        if (campaign == null)
            return NotFound("Campaign not found");

        if (!campaign.CanUserJoin(userId))
            return BadRequest("User is already a member of this campaign");

        campaign.AddMember(userId, request.Alias);

        return Ok(new { Message = "Successfully joined campaign" });
    }

    [HttpPost("{id}/leave")]
    public async Task<ActionResult> LeaveCampaign(Guid id)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == id);

        if (campaign == null)
            return NotFound();

        if (campaign.DMUserId == userId)
            return BadRequest("DM cannot leave their own campaign");

        campaign.RemoveMember(userId);

        return Ok(new { Message = "Successfully left campaign" });
    }

    [HttpPost("{id}/regenerate-token")]
    public async Task<ActionResult<JoinTokenResponse>> RegenerateJoinToken(Guid id)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == id);

        if (campaign == null)
            return NotFound();

        if (campaign.DMUserId != userId)
            return Forbid("Only the DM can regenerate the join token");

        campaign.RegenerateJoinToken();

        return Ok(new JoinTokenResponse { JoinToken = campaign.JoinToken });
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<CampaignMemberDto>>> GetCampaignMembers(Guid id)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == id);

        if (campaign == null)
            return NotFound();

        if (!campaign.IsUserMember(userId) && campaign.DMUserId != userId)
            return Forbid();

        var members = campaign.Members.Select(m => new CampaignMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            Alias = m.Alias,
            Role = m.Role,
            JoinedAt = m.JoinedAt,
            LastSeenAt = m.LastSeenAt,
            UserName = $"User-{m.UserId.ToString()[..8]}" // In real implementation, get from User entity
        }).ToList();

        return Ok(members);
    }

    [HttpDelete("{id}/members/{memberId}")]
    public async Task<ActionResult> RemoveCampaignMember(Guid id, Guid memberId)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == id);

        if (campaign == null)
            return NotFound();

        if (campaign.DMUserId != userId)
            return Forbid("Only the DM can remove members");

        var member = campaign.Members.FirstOrDefault(m => m.Id == memberId);
        if (member == null)
            return NotFound("Member not found");

        if (member.UserId == campaign.DMUserId)
            return BadRequest("Cannot remove the DM from the campaign");

        campaign.RemoveMember(member.UserId);

        return Ok(new { Message = "Member removed successfully" });
    }

    [HttpPost("{id}/invite")]
    public async Task<ActionResult<InviteResponse>> CreateInviteLink(Guid id, [FromBody] CreateInviteRequest request)
    {
        var userId = GetCurrentUserId();
        var campaign = _campaigns.FirstOrDefault(c => c.Id == id);

        if (campaign == null)
            return NotFound();

        if (campaign.DMUserId != userId)
            return Forbid("Only the DM can create invite links");

        // Generate a unique invite link
        var inviteCode = Guid.NewGuid().ToString("N")[..12].ToUpper();
        var inviteLink = $"{Request.Scheme}://{Request.Host}/join/{inviteCode}";

        // In a real implementation, you would store this invite with expiration
        // For now, we'll use the campaign's join token
        return Ok(new InviteResponse
        {
            InviteCode = campaign.JoinToken.ToString(),
            InviteLink = $"{Request.Scheme}://{Request.Host}/join/{campaign.JoinToken}",
            ExpiresAt = DateTime.UtcNow.AddDays(7) // 7 days expiration
        });
    }

    [HttpGet("join/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<CampaignJoinInfo>> GetCampaignByJoinToken(Guid token)
    {
        var campaign = _campaigns.FirstOrDefault(c => c.JoinToken == token);

        if (campaign == null)
            return NotFound("Invalid join token");

        return Ok(new CampaignJoinInfo
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            DMName = "DM", // In real implementation, you'd get the DM's display name
            MemberCount = campaign.Members.Count
        });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new InvalidOperationException("User ID not found in token"));
    }

    private static CampaignDto MapToCampaignDto(Campaign campaign)
    {
        return new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            DMUserId = campaign.DMUserId,
            JoinToken = campaign.JoinToken,
            IsActive = campaign.IsActive,
            LastActivityAt = campaign.LastActivityAt,
            MemberCount = campaign.Members.Count,
            SessionCount = campaign.Sessions.Count,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
    }
}

public class CreateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Variant Rules
    public bool UseFreeArchetype { get; set; } = false;
    public bool UseDualClass { get; set; } = false;
    public bool UseProficiencyWithoutLevel { get; set; } = false;
    public bool UseAutomaticBonusProgression { get; set; } = false;
    public bool UseGradualAbilityBoosts { get; set; } = false;
    public bool UseStaminaVariant { get; set; } = false;
}

public class UpdateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class JoinCampaignRequest
{
    public string Alias { get; set; } = string.Empty;
}

public class CampaignDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DMUserId { get; set; }
    public Guid JoinToken { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public int MemberCount { get; set; }
    public int SessionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CampaignJoinInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DMName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
}

public class JoinTokenResponse
{
    public Guid JoinToken { get; set; }
}

public class CampaignMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public CampaignRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

public class CreateInviteRequest
{
    public string? CustomMessage { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class InviteResponse
{
    public string InviteCode { get; set; } = string.Empty;
    public string InviteLink { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}