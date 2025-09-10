using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Presentation.Shared.Models;
using System.Security.Claims;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CharactersController : ControllerBase
{
    // In a real implementation, you would inject character repository/service
    private static readonly List<CharacterData> _characters = new();
    private static readonly Dictionary<Guid, List<Guid>> _campaignCharacters = new();

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterDto>>> GetCharacters([FromQuery] Guid? campaignId = null)
    {
        var userId = GetCurrentUserId();

        IEnumerable<CharacterData> userCharacters = _characters.Where(c => c.UserId == userId);

        // Filter by campaign if specified
        if (campaignId.HasValue && _campaignCharacters.TryGetValue(campaignId.Value, out var campaignCharacterIds))
        {
            userCharacters = userCharacters.Where(c => campaignCharacterIds.Contains(c.Id));
        }

        var characterDtos = userCharacters.Select(MapToCharacterDto).ToList();
        return Ok(characterDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CharacterDto>> GetCharacter(Guid id)
    {
        var userId = GetCurrentUserId();
        var character = _characters.FirstOrDefault(c => c.Id == id && c.UserId == userId);

        if (character == null)
            return NotFound();

        return Ok(MapToCharacterDto(character));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCharacter([FromBody] CreateCharacterRequest request)
    {
        var userId = GetCurrentUserId();

        var character = new CharacterData
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Level = request.Level,
            Ancestry = request.Ancestry,
            Heritage = request.Heritage,
            Background = request.Background,
            Class = request.Class,
            AbilityScores = request.AbilityScores ?? new Dictionary<string, int>(),
            Skills = request.Skills ?? new Dictionary<string, string>(),
            Feats = request.Feats ?? new List<string>(),
            Equipment = request.Equipment ?? new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _characters.Add(character);

        // Associate with campaign if provided
        if (request.CampaignId.HasValue)
        {
            if (!_campaignCharacters.ContainsKey(request.CampaignId.Value))
            {
                _campaignCharacters[request.CampaignId.Value] = new List<Guid>();
            }
            _campaignCharacters[request.CampaignId.Value].Add(character.Id);
        }

        return CreatedAtAction(
            nameof(GetCharacter),
            new { id = character.Id },
            character.Id
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CharacterDto>> UpdateCharacter(Guid id, [FromBody] UpdateCharacterRequest request)
    {
        var userId = GetCurrentUserId();
        var character = _characters.FirstOrDefault(c => c.Id == id && c.UserId == userId);

        if (character == null)
            return NotFound();

        character.Name = request.Name;
        character.Level = request.Level;
        character.AbilityScores = request.AbilityScores ?? character.AbilityScores;
        character.Skills = request.Skills ?? character.Skills;
        character.Feats = request.Feats ?? character.Feats;
        character.Equipment = request.Equipment ?? character.Equipment;
        character.UpdatedAt = DateTime.UtcNow;

        return Ok(MapToCharacterDto(character));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCharacter(Guid id)
    {
        var userId = GetCurrentUserId();
        var character = _characters.FirstOrDefault(c => c.Id == id && c.UserId == userId);

        if (character == null)
            return NotFound();

        _characters.Remove(character);

        // Remove from all campaigns
        foreach (var campaignChars in _campaignCharacters.Values)
        {
            campaignChars.Remove(id);
        }

        return NoContent();
    }

    [HttpPost("{characterId}/join-campaign/{campaignId}")]
    public async Task<ActionResult> JoinCampaignWithCharacter(Guid characterId, Guid campaignId)
    {
        var userId = GetCurrentUserId();
        var character = _characters.FirstOrDefault(c => c.Id == characterId && c.UserId == userId);

        if (character == null)
            return NotFound("Character not found");

        if (!_campaignCharacters.ContainsKey(campaignId))
        {
            _campaignCharacters[campaignId] = new List<Guid>();
        }

        if (!_campaignCharacters[campaignId].Contains(characterId))
        {
            _campaignCharacters[campaignId].Add(characterId);
        }

        return Ok(new { Message = "Character successfully joined campaign" });
    }

    [HttpDelete("{characterId}/leave-campaign/{campaignId}")]
    public async Task<ActionResult> LeaveCampaignWithCharacter(Guid characterId, Guid campaignId)
    {
        var userId = GetCurrentUserId();
        var character = _characters.FirstOrDefault(c => c.Id == characterId && c.UserId == userId);

        if (character == null)
            return NotFound("Character not found");

        if (_campaignCharacters.TryGetValue(campaignId, out var campaignChars))
        {
            campaignChars.Remove(characterId);
        }

        return Ok(new { Message = "Character successfully left campaign" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new InvalidOperationException("User ID not found in token"));
    }

    private static CharacterDto MapToCharacterDto(CharacterData character)
    {
        return new CharacterDto
        {
            Id = character.Id,
            Name = character.Name,
            Level = character.Level,
            Ancestry = character.Ancestry,
            Heritage = character.Heritage,
            Background = character.Background,
            Class = character.Class,
            AbilityScores = character.AbilityScores,
            Skills = character.Skills,
            Feats = character.Feats,
            Equipment = character.Equipment,
            CreatedAt = character.CreatedAt,
            UpdatedAt = character.UpdatedAt
        };
    }
}

public class CharacterData
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public string Ancestry { get; set; } = string.Empty;
    public string Heritage { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public Dictionary<string, int> AbilityScores { get; set; } = new();
    public Dictionary<string, string> Skills { get; set; } = new();
    public List<string> Feats { get; set; } = new();
    public List<string> Equipment { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}