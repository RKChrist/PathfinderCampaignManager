using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Application.Encounters.Commands;
using PathfinderCampaignManager.Application.Encounters.Queries;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CombatController : ControllerBase
{
    private readonly IMediator _mediator;

    public CombatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("encounters")]
    public async Task<ActionResult<Guid>> CreateEncounter([FromBody] CreateEncounterRequest request)
    {
        var command = new CreateEncounterCommand(
            request.SessionId,
            request.Name,
            request.Description
        );

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetEncounter), new { id = result.Value }, result.Value);
        }

        return BadRequest(result.Error);
    }

    [HttpGet("encounters/{id:guid}")]
    public async Task<ActionResult<EncounterResponse>> GetEncounter(Guid id)
    {
        var query = new GetEncounterByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(EncounterResponse.FromEntity(result.Value));
        }

        return NotFound(result.Error);
    }

    [HttpPut("encounters/{id:guid}/start")]
    public async Task<ActionResult> StartEncounter(Guid id)
    {
        var command = new StartEncounterCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Error);
    }

    [HttpPut("encounters/{id:guid}/end")]
    public async Task<ActionResult> EndEncounter(Guid id)
    {
        var command = new EndEncounterCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Error);
    }

    [HttpPost("encounters/{encounterId:guid}/combatants")]
    public async Task<ActionResult> AddCombatant(Guid encounterId, [FromBody] AddCombatantRequest request)
    {
        var command = new AddCombatantCommand(
            encounterId,
            request.Name,
            request.Initiative,
            request.CharacterId,
            request.NpcMonsterId
        );

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Error);
    }

    [HttpDelete("encounters/{encounterId:guid}/combatants/{combatantId:guid}")]
    public async Task<ActionResult> RemoveCombatant(Guid encounterId, Guid combatantId)
    {
        var command = new RemoveCombatantCommand(encounterId, combatantId);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Error);
    }

    [HttpPut("encounters/{encounterId:guid}/next-turn")]
    public async Task<ActionResult> NextTurn(Guid encounterId)
    {
        var command = new NextTurnCommand(encounterId);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Error);
    }

    [HttpGet("encounters/active")]
    public async Task<ActionResult<IEnumerable<EncounterResponse>>> GetActiveEncounters()
    {
        var query = new GetActiveEncountersQuery();
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            var response = result.Value.Select(EncounterResponse.FromEntity);
            return Ok(response);
        }

        return BadRequest(result.Error);
    }

    [HttpGet("sessions/{sessionId:guid}/encounters")]
    public async Task<ActionResult<IEnumerable<EncounterResponse>>> GetEncountersBySession(Guid sessionId)
    {
        var query = new GetEncountersBySessionQuery(sessionId);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            var response = result.Value.Select(EncounterResponse.FromEntity);
            return Ok(response);
        }

        return BadRequest(result.Error);
    }
}

public record CreateEncounterRequest(
    string Name,
    Guid SessionId,
    string? Description = null
);

public record AddCombatantRequest(
    string Name,
    int Initiative,
    Guid? CharacterId = null,
    Guid? NpcMonsterId = null
);

public record EncounterResponse(
    Guid Id,
    string Name,
    Guid SessionId,
    string? Description,
    bool IsActive,
    int CurrentRound,
    int CurrentTurn,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<CombatantResponse> Combatants
)
{
    public static EncounterResponse FromEntity(PathfinderCampaignManager.Domain.Entities.Encounter encounter) =>
        new(
            encounter.Id,
            encounter.Name,
            encounter.SessionId,
            encounter.Description,
            encounter.IsActive,
            encounter.CurrentRound,
            encounter.CurrentTurn,
            encounter.CreatedAt,
            encounter.UpdatedAt,
            encounter.Combatants.Select(CombatantResponse.FromEntity)
        );
}

public record CombatantResponse(
    Guid Id,
    string Name,
    int Initiative,
    Guid? CharacterId,
    Guid? NpcMonsterId,
    int TurnOrder
)
{
    public static CombatantResponse FromEntity(PathfinderCampaignManager.Domain.Entities.Combatant combatant) =>
        new(
            combatant.Id,
            combatant.Name,
            combatant.Initiative,
            combatant.CharacterId,
            combatant.NpcMonsterId,
            combatant.TurnOrder
        );
}