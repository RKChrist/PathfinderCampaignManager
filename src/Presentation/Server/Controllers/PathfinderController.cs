using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PathfinderController : ControllerBase
{
    private readonly IPathfinderDataRepository _pathfinderRepository;

    public PathfinderController(IPathfinderDataRepository pathfinderRepository)
    {
        _pathfinderRepository = pathfinderRepository;
    }

    [HttpGet("classes")]
    public async Task<ActionResult<List<PfClass>>> GetClasses()
    {
        var result = await _pathfinderRepository.GetAllClassesAsync();
        return result.Match<ActionResult<List<PfClass>>>(
            classes => Ok(classes.ToList()),
            error => BadRequest(error.Message)
        );
    }

    [HttpGet("feats")]
    public async Task<ActionResult<List<PfFeat>>> GetFeats()
    {
        var result = await _pathfinderRepository.GetFeatsAsync();
        return result.Match<ActionResult<List<PfFeat>>>(
            feats => Ok(feats.ToList()),
            error => BadRequest(error.Message)
        );
    }

    [HttpGet("feats/{featId}")]
    public async Task<ActionResult<PfFeat>> GetFeat(string featId)
    {
        var result = await _pathfinderRepository.GetFeatAsync(featId);
        return result.Match<ActionResult<PfFeat>>(
            feat => Ok(feat),
            error => NotFound(error.Message)
        );
    }

    [HttpGet("ancestries")]
    public async Task<ActionResult<List<PfAncestry>>> GetAncestries()
    {
        var result = await _pathfinderRepository.GetAllAncestriesAsync();
        return result.Match<ActionResult<List<PfAncestry>>>(
            ancestries => Ok(ancestries.ToList()),
            error => BadRequest(error.Message)
        );
    }

    [HttpGet("backgrounds")]
    public async Task<ActionResult<List<PfBackground>>> GetBackgrounds()
    {
        var result = await _pathfinderRepository.GetAllBackgroundsAsync();
        return result.Match<ActionResult<List<PfBackground>>>(
            backgrounds => Ok(backgrounds.ToList()),
            error => BadRequest(error.Message)
        );
    }

    [HttpGet("spells")]
    public async Task<ActionResult<List<PfSpell>>> GetSpells()
    {
        var result = await _pathfinderRepository.GetSpellsAsync();
        return result.Match<ActionResult<List<PfSpell>>>(
            spells => Ok(spells.ToList()),
            error => BadRequest(error.Message)
        );
    }

    [HttpGet("spells/{spellId}")]
    public async Task<ActionResult<PfSpell>> GetSpell(string spellId)
    {
        var result = await _pathfinderRepository.GetSpellAsync(spellId);
        return result.Match<ActionResult<PfSpell>>(
            spell => Ok(spell),
            error => NotFound(error.Message)
        );
    }
}