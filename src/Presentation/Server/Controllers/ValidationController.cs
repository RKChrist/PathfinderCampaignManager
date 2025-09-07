using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Validation;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    private readonly IValidationService _validationService;
    private readonly ICharacterRepository _characterRepository;

    public ValidationController(IValidationService validationService, ICharacterRepository characterRepository)
    {
        _validationService = validationService;
        _characterRepository = characterRepository;
    }

    [HttpPost("character/{characterId}")]
    public async Task<ActionResult<ValidationReport>> ValidateCharacter(Guid characterId)
    {
        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null)
        {
            return NotFound($"Character with ID {characterId} not found");
        }

        // Convert Character to PfCharacter for validation
        var pfCharacter = new PfCharacter 
        { 
            Id = character.Id, 
            Name = character.Name, 
            Level = character.Level 
        };
        
        var validationResult = await _validationService.ValidateCharacterAsync(pfCharacter);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpPost("calculated-character")]
    public async Task<ActionResult<ValidationReport>> ValidateCalculatedCharacter([FromBody] ICalculatedCharacter character)
    {
        if (character == null)
        {
            return BadRequest("Character data is required");
        }

        var validationResult = await _validationService.ValidateCalculatedCharacterAsync(character);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpPost("campaign/{campaignId}")]
    public async Task<ActionResult<ValidationReport>> ValidateCampaign(string campaignId)
    {
        if (string.IsNullOrWhiteSpace(campaignId))
        {
            return BadRequest("Campaign ID is required");
        }

        var validationResult = await _validationService.ValidateCampaignAsync(campaignId);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpPost("archetype/{characterId}/{archetypeId}")]
    public async Task<ActionResult<ValidationReport>> ValidateArchetypeProgression(string characterId, string archetypeId)
    {
        if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(archetypeId))
        {
            return BadRequest("Character ID and Archetype ID are required");
        }

        var validationResult = await _validationService.ValidateArchetypeProgressionAsync(characterId, archetypeId);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpPost("prerequisites")]
    public async Task<ActionResult<ValidationReport>> ValidatePrerequisites([FromBody] PrerequisiteValidationRequest request)
    {
        if (request == null || request.Character == null)
        {
            return BadRequest("Character and prerequisites data are required");
        }

        var validationResult = await _validationService.ValidatePrerequisitesAsync(request.Prerequisites, request.Character);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpPost("spellcasting")]
    public async Task<ActionResult<ValidationReport>> ValidateSpellcasting([FromBody] ICalculatedCharacter character)
    {
        if (character == null)
        {
            return BadRequest("Character data is required");
        }

        var validationResult = await _validationService.ValidateSpellcastingAsync(character);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpPost("feat/{featId}")]
    public async Task<ActionResult<ValidationReport>> ValidateFeatSelection(string featId, [FromBody] ICalculatedCharacter character)
    {
        if (string.IsNullOrWhiteSpace(featId) || character == null)
        {
            return BadRequest("Feat ID and character data are required");
        }

        var validationResult = await _validationService.ValidateFeatSelectionAsync(character, featId);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpPost("ability-scores")]
    public async Task<ActionResult<ValidationReport>> ValidateAbilityScores([FromBody] AbilityScoreValidationRequest request)
    {
        if (request == null || request.AbilityScores == null)
        {
            return BadRequest("Ability scores data is required");
        }

        var validationResult = await _validationService.ValidateAbilityScoreArrayAsync(request.AbilityScores, request.CharacterLevel);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpPost("equipment")]
    public async Task<ActionResult<ValidationReport>> ValidateEquipmentLoad([FromBody] ICalculatedCharacter character)
    {
        if (character == null)
        {
            return BadRequest("Character data is required");
        }

        var validationResult = await _validationService.ValidateEquipmentLoadAsync(character);
        if (validationResult.IsFailure)
        {
            return BadRequest(validationResult.Error.Message);
        }

        return Ok(validationResult.Value);
    }

    [HttpGet("health")]
    public async Task<ActionResult<string>> GetValidationHealth()
    {
        return Ok("Validation service is running");
    }
}

public class PrerequisiteValidationRequest
{
    public IEnumerable<PfPrerequisite> Prerequisites { get; set; } = new List<PfPrerequisite>();
    public ICalculatedCharacter Character { get; set; } = null!;
}

public class AbilityScoreValidationRequest
{
    public Dictionary<string, int> AbilityScores { get; set; } = new Dictionary<string, int>();
    public int CharacterLevel { get; set; } = 1;
}