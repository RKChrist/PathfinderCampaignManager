using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PathfinderCampaignManager.Domain.Entities.Combat;
using PathfinderCampaignManager.Presentation.Server.Hubs;
using System.Security.Claims;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CombatController : ControllerBase
{
    private readonly IHubContext<CombatHub> _combatHub;
    private readonly ILogger<CombatController> _logger;

    // In-memory storage for demo purposes - made public so CombatHub can access it
    public static readonly Dictionary<Guid, CombatSession> _combatSessions = new();

    public CombatController(
        IHubContext<CombatHub> combatHub,
        ILogger<CombatController> logger)
    {
        _combatHub = combatHub;
        _logger = logger;
    }

    [HttpGet("campaigns/{campaignId:guid}")]
    public async Task<ActionResult<PathfinderCampaignManager.Presentation.Shared.Models.CombatSession>> GetCampaignCombatSession(Guid campaignId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Get or create combat session
            var combatSession = await GetOrCreateCombatSessionInternal(campaignId);

            var sharedCombatSession = new PathfinderCampaignManager.Presentation.Shared.Models.CombatSession
            {
                Id = combatSession.Id,
                Name = combatSession.Name,
                IsActive = combatSession.IsActive,
                Round = combatSession.Round,
                CurrentTurn = combatSession.CurrentTurn,
                Participants = combatSession.Participants.Select(MapToSharedParticipant).ToList()
            };

            return Ok(sharedCombatSession);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combat session for campaign {CampaignId}", campaignId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("campaigns/{campaignId:guid}/start")]
    public async Task<ActionResult> StartCampaignCombat(Guid campaignId)
    {
        try
        {
            var combatSession = await GetOrCreateCombatSessionInternal(campaignId);
            
            if (!combatSession.Participants.Any())
                return BadRequest("Cannot start combat without participants");

            combatSession.IsActive = true;
            combatSession.Round = 1;
            combatSession.CurrentTurn = 0;

            // Sort participants by initiative (descending)
            combatSession.Participants = combatSession.Participants
                .OrderByDescending(p => p.Initiative)
                .ThenByDescending(p => p.InitiativeModifier)
                .ToList();

            _combatSessions[campaignId] = combatSession;

            // Notify all connected clients
            await _combatHub.Clients.Group($"Combat-{campaignId}").SendAsync("CombatUpdated", campaignId.ToString());

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting combat for campaign {CampaignId}", campaignId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("add-participants")]
    public async Task<ActionResult> AddParticipantsToCombat([FromBody] AddParticipantsRequest request)
    {
        try
        {
            var combatSession = await GetOrCreateCombatSessionInternal(request.CampaignId);

            foreach (var participantData in request.Participants)
            {
                if (participantData.TryGetValue("Id", out var idValue) && Guid.TryParse(idValue?.ToString(), out var id))
                {
                    // Check if participant already exists
                    if (combatSession.Participants.Any(p => p.Id == id || p.CharacterId == id))
                        continue;

                    var participant = new CombatParticipant
                    {
                        Id = Guid.NewGuid(),
                        Name = participantData.GetValueOrDefault("Name", "Unknown").ToString() ?? "Unknown",
                        Type = ParseParticipantType(participantData.GetValueOrDefault("Type", "PlayerCharacter").ToString()),
                        Initiative = Convert.ToInt32(participantData.GetValueOrDefault("Initiative", 0)),
                        InitiativeModifier = 0, // Could be calculated from stats
                        HitPoints = Convert.ToInt32(participantData.GetValueOrDefault("HitPoints", 1)),
                        CurrentHitPoints = Convert.ToInt32(participantData.GetValueOrDefault("HitPoints", 1)),
                        ArmorClass = Convert.ToInt32(participantData.GetValueOrDefault("ArmorClass", 10)),
                        FortitudeSave = 0, // Would be calculated from character data
                        ReflexSave = 0,
                        WillSave = 0,
                        IsPlayerCharacter = Convert.ToBoolean(participantData.GetValueOrDefault("IsPlayerCharacter", false)),
                        IsHidden = false,
                        CharacterId = Convert.ToBoolean(participantData.GetValueOrDefault("IsPlayerCharacter", false)) ? id : null,
                        PlayerId = Convert.ToBoolean(participantData.GetValueOrDefault("IsPlayerCharacter", false)) ? GetCurrentUserId() : null,
                        Notes = string.Empty,
                        Conditions = new List<string>(),
                        Level = 1, // Would come from character data
                        Class = "Unknown",
                        Ancestry = "Unknown"
                    };

                    combatSession.Participants.Add(participant);
                }
            }

            _combatSessions[request.CampaignId] = combatSession;

            // Notify all connected clients via SignalR
            await _combatHub.Clients.Group($"Combat-{request.CampaignId}").SendAsync("ParticipantsAdded", 
                combatSession.Participants.Select(MapToSharedParticipant).ToList());

            _logger.LogInformation("Added {Count} participants to combat session for campaign {CampaignId}", 
                request.Participants.Count, request.CampaignId);

            return Ok(new { Message = "Participants added successfully", Count = request.Participants.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participants to combat for campaign {CampaignId}", request.CampaignId);
            return StatusCode(500, "Internal server error");
        }
    }

    private static CombatParticipantType ParseParticipantType(string? typeString)
    {
        return typeString?.ToLower() switch
        {
            "playercharacter" => CombatParticipantType.PlayerCharacter,
            "nonplayercharacter" => CombatParticipantType.NonPlayerCharacter,
            "monster" => CombatParticipantType.Monster,
            "hazard" => CombatParticipantType.Hazard,
            _ => CombatParticipantType.PlayerCharacter
        };
    }

    private async Task<CombatSession> GetOrCreateCombatSessionInternal(Guid campaignId)
    {
        if (_combatSessions.TryGetValue(campaignId, out var existingSession))
        {
            return existingSession;
        }

        // Create new combat session with mock data
        var participants = new List<CombatParticipant>
        {
            new CombatParticipant
            {
                Id = Guid.NewGuid(),
                Name = "Test Fighter",
                Type = CombatParticipantType.PlayerCharacter,
                Initiative = 15,
                InitiativeModifier = 2,
                HitPoints = 25,
                CurrentHitPoints = 25,
                ArmorClass = 18,
                FortitudeSave = 5,
                ReflexSave = 2,
                WillSave = 1,
                IsPlayerCharacter = true,
                IsHidden = false,
                CharacterId = Guid.NewGuid(),
                Notes = string.Empty,
                Conditions = new List<string>(),
                Level = 1,
                Class = "Fighter",
                Ancestry = "Human"
            }
        };

        var combatSession = new CombatSession
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = $"Combat Session for Campaign {campaignId}",
            IsActive = false,
            Round = 0,
            CurrentTurn = 0,
            Participants = participants,
            // CreatedAt and CreatedBy not available in CombatSession
            // These would typically be in BaseEntity but CombatSession may not inherit from it
        };

        _combatSessions[campaignId] = combatSession;
        return combatSession;
    }

    private CombatParticipantDto MapToParticipantDto(CombatParticipant participant)
    {
        return new CombatParticipantDto
        {
            Id = participant.Id,
            Name = participant.Name,
            Type = participant.Type,
            Initiative = participant.Initiative,
            InitiativeModifier = participant.InitiativeModifier,
            HitPoints = participant.HitPoints,
            CurrentHitPoints = participant.CurrentHitPoints,
            ArmorClass = participant.ArmorClass,
            FortitudeSave = participant.FortitudeSave,
            ReflexSave = participant.ReflexSave,
            WillSave = participant.WillSave,
            IsPlayerCharacter = participant.IsPlayerCharacter,
            IsHidden = participant.IsHidden,
            CharacterId = participant.CharacterId,
            Notes = participant.Notes,
            Conditions = participant.Conditions?.ToList() ?? new List<string>(),
            Level = participant.Level,
            Class = participant.Class,
            Ancestry = participant.Ancestry
        };
    }

    private PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant MapToSharedParticipant(CombatParticipant participant)
    {
        return new PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant
        {
            Id = participant.Id,
            Name = participant.Name,
            Type = participant.Type.ToString(),
            Initiative = participant.Initiative,
            InitiativeModifier = participant.InitiativeModifier,
            HitPoints = participant.HitPoints,
            CurrentHitPoints = participant.CurrentHitPoints,
            ArmorClass = participant.ArmorClass,
            FortitudeSave = participant.FortitudeSave,
            ReflexSave = participant.ReflexSave,
            WillSave = participant.WillSave,
            IsPlayerCharacter = participant.IsPlayerCharacter,
            IsHidden = participant.IsHidden,
            CharacterId = participant.CharacterId?.ToString(),
            PlayerId = participant.PlayerId?.ToString(),
            Notes = participant.Notes,
            Conditions = participant.Conditions?.ToList() ?? new List<string>(),
            Level = participant.Level,
            Class = participant.Class,
            Ancestry = participant.Ancestry,
            CreatureType = participant.Class ?? "Unknown",
            PassivePerception = 10 + participant.Level // Simple calculation
        };
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new InvalidOperationException("User ID not found in token"));
    }
}

// DTOs
public class CombatSessionDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Round { get; set; }
    public int CurrentTurn { get; set; }
    public List<CombatParticipantDto> Participants { get; set; } = new();
}

public class CombatParticipantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CombatParticipantType Type { get; set; }
    public int Initiative { get; set; }
    public int InitiativeModifier { get; set; }
    public int HitPoints { get; set; }
    public int CurrentHitPoints { get; set; }
    public int ArmorClass { get; set; }
    public int FortitudeSave { get; set; }
    public int ReflexSave { get; set; }
    public int WillSave { get; set; }
    public bool IsPlayerCharacter { get; set; }
    public bool IsHidden { get; set; }
    public Guid? CharacterId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<string> Conditions { get; set; } = new();
    public int Level { get; set; }
    public string? Class { get; set; }
    public string? Ancestry { get; set; }
}

public class AddParticipantsRequest
{
    public Guid CampaignId { get; set; }
    public List<Dictionary<string, object>> Participants { get; set; } = new();
}