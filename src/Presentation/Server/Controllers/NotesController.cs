using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Application.Notes.Commands;
using PathfinderCampaignManager.Application.Notes.Queries;
using PathfinderCampaignManager.Domain.Enums;
using System.Security.Claims;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all notes for a character
    /// </summary>
    [HttpGet("character/{characterId:guid}")]
    public async Task<ActionResult<List<CharacterNoteDto>>> GetCharacterNotes(
        Guid characterId, 
        [FromQuery] NoteVisibility? visibility = null)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user");
        }

        var query = new GetCharacterNotesQuery(characterId, userId, visibility);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Get a specific note by ID
    /// </summary>
    [HttpGet("{noteId:guid}")]
    public async Task<ActionResult<CharacterNoteDto>> GetNoteById(Guid noteId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user");
        }

        var query = new GetCharacterNoteByIdQuery(noteId, userId);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Contains("not found") ? NotFound(result.Error) : BadRequest(result.Error);
    }

    /// <summary>
    /// Create a new character note
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CharacterNoteDto>> CreateNote([FromBody] CreateCharacterNoteRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user");
        }

        var command = new CreateCharacterNoteCommand(
            request.CharacterId,
            userId,
            request.Title,
            request.Content,
            request.Visibility,
            request.Color,
            request.Tags
        );

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetNoteById), new { noteId = result.Value.Id }, result.Value);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Update an existing note
    /// </summary>
    [HttpPut("{noteId:guid}")]
    public async Task<ActionResult<CharacterNoteDto>> UpdateNote(Guid noteId, [FromBody] UpdateCharacterNoteRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user");
        }

        var command = new UpdateCharacterNoteCommand(
            noteId,
            userId,
            request.Title,
            request.Content,
            request.Tags
        );

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Contains("not found") ? NotFound(result.Error) : BadRequest(result.Error);
    }

    /// <summary>
    /// Delete a note
    /// </summary>
    [HttpDelete("{noteId:guid}")]
    public async Task<ActionResult> DeleteNote(Guid noteId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user");
        }

        var command = new DeleteCharacterNoteCommand(noteId, userId);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Error.Contains("not found") ? NotFound(result.Error) : BadRequest(result.Error);
    }

    /// <summary>
    /// Change note visibility
    /// </summary>
    [HttpPut("{noteId:guid}/visibility")]
    public async Task<ActionResult> ChangeNoteVisibility(Guid noteId, [FromBody] ChangeVisibilityRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user");
        }

        var command = new ChangeCharacterNoteVisibilityCommand(noteId, request.Visibility, userId);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Error.Contains("not found") ? NotFound(result.Error) : BadRequest(result.Error);
    }

    /// <summary>
    /// Update note appearance (color, pin status, sort order)
    /// </summary>
    [HttpPut("{noteId:guid}/appearance")]
    public async Task<ActionResult> UpdateNoteAppearance(Guid noteId, [FromBody] UpdateAppearanceRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user");
        }

        var command = new UpdateCharacterNoteAppearanceCommand(
            noteId, 
            request.Color, 
            request.IsPinned, 
            request.SortOrder, 
            userId
        );

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Error.Contains("not found") ? NotFound(result.Error) : BadRequest(result.Error);
    }

    /// <summary>
    /// Toggle pin status of a note
    /// </summary>
    [HttpPut("{noteId:guid}/pin")]
    public async Task<ActionResult> TogglePin(Guid noteId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user");
        }

        // First get the current note to determine the new pin state
        var getNoteQuery = new GetCharacterNoteByIdQuery(noteId, userId);
        var getNoteResult = await _mediator.Send(getNoteQuery);

        if (!getNoteResult.IsSuccess)
        {
            return getNoteResult.Error.Contains("not found") ? NotFound(getNoteResult.Error) : BadRequest(getNoteResult.Error);
        }

        var currentNote = getNoteResult.Value;
        var command = new UpdateCharacterNoteAppearanceCommand(
            noteId, 
            null, 
            !currentNote.IsPinned, 
            null, 
            userId
        );

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Error);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

// Request DTOs
public record CreateCharacterNoteRequest(
    Guid CharacterId,
    string Title,
    string Content,
    NoteVisibility Visibility = NoteVisibility.Private,
    string? Color = null,
    List<string>? Tags = null
);

public record UpdateCharacterNoteRequest(
    string Title,
    string Content,
    List<string>? Tags = null
);

public record ChangeVisibilityRequest(
    NoteVisibility Visibility
);

public record UpdateAppearanceRequest(
    string? Color = null,
    bool? IsPinned = null,
    int? SortOrder = null
);