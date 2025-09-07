using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchetypeController : ControllerBase
{
    private readonly IArchetypeRepository _archetypeRepository;
    private readonly IArchetypeService _archetypeService;

    public ArchetypeController(IArchetypeRepository archetypeRepository, IArchetypeService archetypeService)
    {
        _archetypeRepository = archetypeRepository;
        _archetypeService = archetypeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PfArchetype>>> GetArchetypes()
    {
        var result = await _archetypeRepository.GetArchetypesAsync();
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("multiclass")]
    public async Task<ActionResult<IEnumerable<PfArchetype>>> GetMulticlassArchetypes()
    {
        var result = await _archetypeRepository.GetMulticlassArchetypesAsync();
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("general")]
    public async Task<ActionResult<IEnumerable<PfArchetype>>> GetGeneralArchetypes()
    {
        var result = await _archetypeRepository.GetGeneralArchetypesAsync();
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("class/{classId}")]
    public async Task<ActionResult<IEnumerable<PfArchetype>>> GetClassArchetypes(string classId)
    {
        var result = await _archetypeRepository.GetClassArchetypesAsync(classId);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PfArchetype>> GetArchetype(string id)
    {
        var result = await _archetypeRepository.GetArchetypeByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}/feats")]
    public async Task<ActionResult<IEnumerable<PfFeat>>> GetArchetypeFeats(string id)
    {
        var result = await _archetypeRepository.GetArchetypeFeatsAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateArchetypePrerequisites([FromBody] ArchetypeValidationRequest request)
    {
        var result = await _archetypeRepository.ValidatePrerequisitesAsync(request.ArchetypeId, request.Character);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("can-take-feat")]
    public async Task<ActionResult<bool>> CanTakeArchetypeFeat([FromBody] ArchetypeFeatValidationRequest request)
    {
        var result = await _archetypeService.CanTakeArchetypeFeatAsync(request.ArchetypeId, request.FeatId, request.Character);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("can-take-new")]
    public async Task<ActionResult<bool>> CanTakeNewArchetype([FromBody] CalculatedCharacter character)
    {
        var result = await _archetypeService.CanTakeNewArchetypeAsync(character);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}/spellcasting/{level}")]
    public async Task<ActionResult<PfMulticlassSpellcasting>> GetMulticlassSpellcasting(string id, int level)
    {
        var result = await _archetypeService.CalculateMulticlassSpellcastingAsync(id, level);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<PfArchetype>>> SearchArchetypes([FromQuery] string term)
    {
        var result = await _archetypeRepository.SearchArchetypesAsync(term);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}

public class ArchetypeValidationRequest
{
    public string ArchetypeId { get; set; } = string.Empty;
    public CalculatedCharacter Character { get; set; } = new();
}

public class ArchetypeFeatValidationRequest
{
    public string ArchetypeId { get; set; } = string.Empty;
    public string FeatId { get; set; } = string.Empty;
    public CalculatedCharacter Character { get; set; } = new();
}