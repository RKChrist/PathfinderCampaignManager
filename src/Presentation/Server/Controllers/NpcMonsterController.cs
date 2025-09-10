using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NpcMonsterController : ControllerBase
{
    // In-memory storage for demo purposes
    private static readonly List<MonsterData> _monsters = InitializeDefaultMonsters();
    private static readonly List<CombatEncounter> _encounters = new();
    private static readonly List<PremadeEncounter> _premadeEncounters = InitializePremadeEncounters();

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MonsterResponse>>> GetMonsters([FromQuery] Guid? sessionId = null, [FromQuery] Guid? campaignId = null)
    {
        var userId = GetCurrentUserId();
        
        // Include both user-owned monsters and template monsters
        var userMonsters = _monsters.Where(m => m.OwnerUserId == userId || m.IsTemplate);

        if (sessionId.HasValue)
        {
            userMonsters = userMonsters.Where(m => m.SessionId == sessionId || m.SessionId == null);
        }
        else if (campaignId.HasValue)
        {
            // For campaign-based queries, include monsters from any session in that campaign
            // Since we don't have campaign-session relationship in current model, 
            // we'll use campaignId as sessionId for this demo
            userMonsters = userMonsters.Where(m => m.SessionId == campaignId || m.SessionId == null);
        }

        var monsterResponses = userMonsters.Select(MapToResponse).ToList();
        return Ok(monsterResponses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MonsterResponse>> GetMonster(Guid id)
    {
        var userId = GetCurrentUserId();
        var monster = _monsters.FirstOrDefault(m => m.Id == id && m.OwnerUserId == userId);

        if (monster == null)
            return NotFound();

        return Ok(MapToResponse(monster));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateMonster([FromBody] CreateMonsterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
            Console.WriteLine($"Request data: Name='{request?.Name}', Type='{request?.Type}', Level={request?.Level}, AC={request?.ArmorClass}, HP={request?.HitPoints}");
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();

        // Parse the Type string to enum
        if (!Enum.TryParse<NpcMonsterType>(request.Type, true, out var monsterType))
        {
            ModelState.AddModelError(nameof(request.Type), "Invalid monster type specified");
            return BadRequest(ModelState);
        }

        var monster = new MonsterData
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = monsterType,
            Level = request.Level,
            Description = request.Description ?? string.Empty,
            ArmorClass = request.ArmorClass,
            HitPoints = request.HitPoints,
            MaxHitPoints = request.HitPoints,
            Speed = request.Speed ?? "30 feet",
            OwnerUserId = userId,
            SessionId = request.SessionId,
            IsTemplate = request.IsTemplate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _monsters.Add(monster);
        Console.WriteLine($"Successfully created monster: {monster.Name} (ID: {monster.Id})");

        return CreatedAtAction(nameof(GetMonster), new { id = monster.Id }, monster.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MonsterResponse>> UpdateMonster(Guid id, [FromBody] UpdateMonsterRequest request)
    {
        var userId = GetCurrentUserId();
        var monster = _monsters.FirstOrDefault(m => m.Id == id && m.OwnerUserId == userId);

        if (monster == null)
            return NotFound();

        // Parse the Type string to enum
        if (!Enum.TryParse<NpcMonsterType>(request.Type, true, out var updateMonsterType))
        {
            ModelState.AddModelError(nameof(request.Type), "Invalid monster type specified");
            return BadRequest(ModelState);
        }

        monster.Name = request.Name;
        monster.Type = updateMonsterType;
        monster.Level = request.Level;
        monster.Description = request.Description ?? string.Empty;
        monster.ArmorClass = request.ArmorClass;
        monster.HitPoints = request.HitPoints;
        monster.MaxHitPoints = request.MaxHitPoints;
        monster.Speed = request.Speed ?? "30 feet";
        monster.IsTemplate = request.IsTemplate;
        monster.UpdatedAt = DateTime.UtcNow;

        return Ok(MapToResponse(monster));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteMonster(Guid id)
    {
        var userId = GetCurrentUserId();
        var monster = _monsters.FirstOrDefault(m => m.Id == id && m.OwnerUserId == userId);

        if (monster == null)
            return NotFound();

        _monsters.Remove(monster);
        return NoContent();
    }

    [HttpPost("{id:guid}/damage")]
    public async Task<ActionResult<MonsterResponse>> DamageMonster(Guid id, [FromBody] DamageRequest request)
    {
        var userId = GetCurrentUserId();
        var monster = _monsters.FirstOrDefault(m => m.Id == id);

        if (monster == null)
            return NotFound();

        monster.HitPoints = Math.Max(0, monster.HitPoints - request.Amount);
        monster.UpdatedAt = DateTime.UtcNow;

        return Ok(MapToResponse(monster));
    }

    [HttpPost("{id:guid}/heal")]
    public async Task<ActionResult<MonsterResponse>> HealMonster(Guid id, [FromBody] HealRequest request)
    {
        var userId = GetCurrentUserId();
        var monster = _monsters.FirstOrDefault(m => m.Id == id);

        if (monster == null)
            return NotFound();

        monster.HitPoints = Math.Min(monster.MaxHitPoints, monster.HitPoints + request.Amount);
        monster.UpdatedAt = DateTime.UtcNow;

        return Ok(MapToResponse(monster));
    }

    // Combat Encounters
    [HttpGet("encounters")]
    public async Task<ActionResult<IEnumerable<MonsterEncounterResponse>>> GetEncounters([FromQuery] Guid? campaignId = null)
    {
        var userId = GetCurrentUserId();
        var userEncounters = _encounters.Where(e => e.CreatedByUserId == userId);

        if (campaignId.HasValue)
        {
            userEncounters = userEncounters.Where(e => e.CampaignId == campaignId);
        }

        var encounterResponses = userEncounters.Select(MapToMonsterEncounterResponse).ToList();
        return Ok(encounterResponses);
    }

    [HttpPost("encounters")]
    public async Task<ActionResult<Guid>> CreateEncounter([FromBody] CreateMonsterEncounterRequest request)
    {
        var userId = GetCurrentUserId();

        var encounter = new CombatEncounter
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            CampaignId = request.CampaignId,
            CreatedByUserId = userId,
            Monsters = request.MonsterIds?.Select(id => _monsters.FirstOrDefault(m => m.Id == id)).Where(m => m != null).Cast<MonsterData>().ToList() ?? new(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _encounters.Add(encounter);

        return CreatedAtAction(nameof(GetEncounter), new { id = encounter.Id }, encounter.Id);
    }

    [HttpGet("encounters/{id:guid}")]
    public async Task<ActionResult<MonsterEncounterResponse>> GetEncounter(Guid id)
    {
        var encounter = _encounters.FirstOrDefault(e => e.Id == id);

        if (encounter == null)
            return NotFound();

        return Ok(MapToMonsterEncounterResponse(encounter));
    }

    // Premade Encounters
    [HttpGet("premade-encounters")]
    public async Task<ActionResult<IEnumerable<PremadeEncounterResponse>>> GetPremadeEncounters()
    {
        var response = _premadeEncounters.Select(MapToPremadeEncounterResponse).ToList();
        return Ok(response);
    }

    [HttpPost("premade-encounters/{id:guid}/start/{campaignId:guid}")]
    public async Task<ActionResult<Guid>> StartPremadeEncounter(Guid id, Guid campaignId)
    {
        var userId = GetCurrentUserId();
        var premadeEncounter = _premadeEncounters.FirstOrDefault(pe => pe.Id == id);

        if (premadeEncounter == null)
            return NotFound("Premade encounter not found");

        // Create actual monsters from the premade encounter
        var monsters = new List<MonsterData>();
        foreach (var monsterTemplate in premadeEncounter.MonsterTemplates)
        {
            for (int i = 0; i < monsterTemplate.Count; i++)
            {
                // Parse the template type to enum
                if (!Enum.TryParse<NpcMonsterType>(monsterTemplate.Type, true, out var templateType))
                {
                    templateType = NpcMonsterType.Humanoid; // Default fallback
                }

                var monster = new MonsterData
                {
                    Id = Guid.NewGuid(),
                    Name = monsterTemplate.Count > 1 ? $"{monsterTemplate.Name} #{i + 1}" : monsterTemplate.Name,
                    Type = templateType,
                    Level = monsterTemplate.Level,
                    Description = monsterTemplate.Description,
                    ArmorClass = monsterTemplate.ArmorClass,
                    HitPoints = monsterTemplate.HitPoints,
                    MaxHitPoints = monsterTemplate.HitPoints,
                    Speed = monsterTemplate.Speed,
                    OwnerUserId = userId,
                    IsTemplate = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _monsters.Add(monster);
                monsters.Add(monster);
            }
        }

        // Create the encounter
        var encounter = new CombatEncounter
        {
            Id = Guid.NewGuid(),
            Name = premadeEncounter.Name,
            Description = premadeEncounter.Description,
            CampaignId = campaignId,
            CreatedByUserId = userId,
            Monsters = monsters,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _encounters.Add(encounter);

        return CreatedAtAction(nameof(GetEncounter), new { id = encounter.Id }, encounter.Id);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new InvalidOperationException("User ID not found in token"));
    }

    private static MonsterResponse MapToResponse(MonsterData monster)
    {
        return new MonsterResponse
        {
            Id = monster.Id,
            Name = monster.Name,
            Type = monster.Type,
            Level = monster.Level,
            Description = monster.Description,
            ArmorClass = monster.ArmorClass,
            HitPoints = monster.HitPoints,
            Speed = ParseSpeedToInteger(monster.Speed),
            IsTemplate = monster.IsTemplate,
            OwnerUserId = monster.OwnerUserId,
            SessionId = monster.SessionId,
            CreatedAt = monster.CreatedAt,
            UpdatedAt = monster.UpdatedAt
        };
    }

    private static int ParseSpeedToInteger(string speed)
    {
        if (string.IsNullOrWhiteSpace(speed))
            return 25;

        // Extract number from strings like "30 feet", "25", etc.
        var match = System.Text.RegularExpressions.Regex.Match(speed, @"\d+");
        if (match.Success && int.TryParse(match.Value, out var result))
            return result;
        
        return 25; // Default fallback
    }

    private static MonsterEncounterResponse MapToMonsterEncounterResponse(CombatEncounter encounter)
    {
        return new MonsterEncounterResponse
        {
            Id = encounter.Id,
            Name = encounter.Name,
            Description = encounter.Description,
            CampaignId = encounter.CampaignId,
            Monsters = encounter.Monsters.Select(MapToResponse).ToList(),
            CreatedAt = encounter.CreatedAt,
            UpdatedAt = encounter.UpdatedAt
        };
    }

    private static PremadeEncounterResponse MapToPremadeEncounterResponse(PremadeEncounter encounter)
    {
        return new PremadeEncounterResponse
        {
            Id = encounter.Id,
            Name = encounter.Name,
            Description = encounter.Description,
            PartyLevel = encounter.PartyLevel,
            Difficulty = encounter.Difficulty,
            MonsterTemplates = encounter.MonsterTemplates
        };
    }

    private static List<MonsterData> InitializeDefaultMonsters()
    {
        // Create a default user ID for demo purposes
        var defaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        return new List<MonsterData>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Goblin Warrior",
                Type = NpcMonsterType.Humanoid,
                Level = 1,
                Description = "A small but fierce goblin warrior armed with crude weapons.",
                ArmorClass = 16,
                HitPoints = 15,
                MaxHitPoints = 15,
                Speed = "25 feet",
                OwnerUserId = defaultUserId,
                SessionId = null, // Available to all campaigns
                IsTemplate = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Orc Brute",
                Type = NpcMonsterType.Humanoid,
                Level = 2,
                Description = "A savage orc warrior with great strength and brutality.",
                ArmorClass = 13,
                HitPoints = 30,
                MaxHitPoints = 30,
                Speed = "25 feet",
                OwnerUserId = defaultUserId,
                SessionId = null,
                IsTemplate = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Skeleton Guard",
                Type = NpcMonsterType.Undead,
                Level = 2,
                Description = "An animated skeleton wearing tattered armor.",
                ArmorClass = 16,
                HitPoints = 20,
                MaxHitPoints = 20,
                Speed = "25 feet",
                OwnerUserId = defaultUserId,
                SessionId = null,
                IsTemplate = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Town Guard",
                Type = NpcMonsterType.Humanoid,
                Level = 1,
                Description = "A local guard who keeps the peace in town.",
                ArmorClass = 18,
                HitPoints = 22,
                MaxHitPoints = 22,
                Speed = "25 feet",
                OwnerUserId = defaultUserId,
                SessionId = null,
                IsTemplate = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<PremadeEncounter> InitializePremadeEncounters()
    {
        return new List<PremadeEncounter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Goblin Ambush",
                Description = "A group of goblins attacks from the bushes along a forest path.",
                PartyLevel = 1,
                Difficulty = "Low",
                MonsterTemplates = new List<MonsterTemplate>
                {
                    new() { Name = "Goblin Warrior", Type = "Humanoid", Level = -1, ArmorClass = 16, HitPoints = 6, Speed = "25 feet", Count = 3, Description = "A small but fierce goblin warrior armed with a dogslicer." },
                    new() { Name = "Goblin Commando", Type = "Humanoid", Level = 1, ArmorClass = 17, HitPoints = 18, Speed = "25 feet", Count = 1, Description = "The leader of this goblin war party, wielding a horsechopper." }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Skeletal Guards",
                Description = "Ancient skeletal guardians animate to defend a forgotten tomb.",
                PartyLevel = 2,
                Difficulty = "Moderate",
                MonsterTemplates = new List<MonsterTemplate>
                {
                    new() { Name = "Skeleton Guard", Type = "Undead", Level = 2, ArmorClass = 16, HitPoints = 20, Speed = "25 feet", Count = 2, Description = "Animated skeleton wearing tattered armor and wielding rusty weapons." },
                    new() { Name = "Skeletal Champion", Type = "Undead", Level = 2, ArmorClass = 18, HitPoints = 32, Speed = "25 feet", Count = 1, Description = "A more powerful undead warrior retaining some combat skills from life." }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Orc Raiding Party",
                Description = "A violent orc warband seeks to pillage and plunder.",
                PartyLevel = 3,
                Difficulty = "Moderate",
                MonsterTemplates = new List<MonsterTemplate>
                {
                    new() { Name = "Orc Brute", Type = "Humanoid", Level = 2, ArmorClass = 18, HitPoints = 32, Speed = "25 feet", Count = 2, Description = "A hulking orc warrior with a massive falchion." },
                    new() { Name = "Orc Warchief", Type = "Humanoid", Level = 4, ArmorClass = 21, HitPoints = 70, Speed = "25 feet", Count = 1, Description = "The brutal leader of the orc warband, experienced in battle." }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Owlbear Encounter",
                Description = "A territorial owlbear defends its forest domain from intruders.",
                PartyLevel = 4,
                Difficulty = "Severe",
                MonsterTemplates = new List<MonsterTemplate>
                {
                    new() { Name = "Owlbear", Type = "Beast", Level = 4, ArmorClass = 22, HitPoints = 70, Speed = "25 feet", Count = 1, Description = "A ferocious beast with the body of a bear and the head of an owl, known for its deadly embrace." }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Troll Bridge",
                Description = "A hungry troll demands payment from those who wish to cross its bridge.",
                PartyLevel = 5,
                Difficulty = "Severe",
                MonsterTemplates = new List<MonsterTemplate>
                {
                    new() { Name = "Troll", Type = "Giant", Level = 5, ArmorClass = 22, HitPoints = 90, Speed = "30 feet", Count = 1, Description = "A regenerating giant with incredible strength and an insatiable hunger." }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Dragon's Lair - Young Red Dragon",
                Description = "A young red dragon guards its growing hoard in a volcanic lair.",
                PartyLevel = 8,
                Difficulty = "Extreme",
                MonsterTemplates = new List<MonsterTemplate>
                {
                    new() { Name = "Young Red Dragon", Type = "Dragon", Level = 10, ArmorClass = 29, HitPoints = 210, Speed = "40 feet, fly 120 feet", Count = 1, Description = "A fierce young red dragon with devastating fire breath and cunning intelligence." }
                }
            }
        };
    }
}

// Data models
public class MonsterData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NpcMonsterType Type { get; set; }
    public int Level { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ArmorClass { get; set; }
    public int HitPoints { get; set; }
    public int MaxHitPoints { get; set; }
    public string Speed { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public Guid? SessionId { get; set; }
    public bool IsTemplate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CombatEncounter
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? CampaignId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public List<MonsterData> Monsters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Request models
public class CreateMonsterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty;  // Keep as string for client compatibility, convert to enum internally
    
    [Range(-1, 25)]
    public int Level { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Range(10, 50)]
    public int ArmorClass { get; set; }
    
    [Range(1, 1000)]
    public int HitPoints { get; set; }
    
    [StringLength(100)]
    public string? Speed { get; set; }
    
    public Guid? SessionId { get; set; }
    public bool IsTemplate { get; set; }
}

public class UpdateMonsterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;  // Keep as string for client compatibility
    public int Level { get; set; }
    public string? Description { get; set; }
    public int ArmorClass { get; set; }
    public int HitPoints { get; set; }
    public int MaxHitPoints { get; set; }
    public string? Speed { get; set; }
    public bool IsTemplate { get; set; }
}

public class CreateMonsterEncounterRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CampaignId { get; set; }
    public List<Guid>? MonsterIds { get; set; }
}

public class DamageRequest
{
    public int Amount { get; set; }
}

public class HealRequest
{
    public int Amount { get; set; }
}

// Response models
public class MonsterResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NpcMonsterType Type { get; set; }
    public int Level { get; set; }
    public string? Description { get; set; }
    public int? ArmorClass { get; set; }
    public int? HitPoints { get; set; }
    public int? Speed { get; set; }
    public bool IsTemplate { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MonsterEncounterResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? CampaignId { get; set; }
    public List<MonsterResponse> Monsters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Premade Encounter Models
public class PremadeEncounter
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PartyLevel { get; set; }
    public string Difficulty { get; set; } = string.Empty; // Trivial, Low, Moderate, Severe, Extreme
    public List<MonsterTemplate> MonsterTemplates { get; set; } = new();
}

public class MonsterTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ArmorClass { get; set; }
    public int HitPoints { get; set; }
    public string Speed { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
}

public class PremadeEncounterResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PartyLevel { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public List<MonsterTemplate> MonsterTemplates { get; set; } = new();
}