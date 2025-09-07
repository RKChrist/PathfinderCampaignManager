using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Domain.Entities.Rules;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RulesController : ControllerBase
{
    private readonly IRulesRepository _rulesRepository;

    public RulesController(IRulesRepository rulesRepository)
    {
        _rulesRepository = rulesRepository;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<RuleCategory>>> GetCategories()
    {
        try
        {
            var categoryCountsResult = await _rulesRepository.GetRuleCategoryCountsAsync();
            return categoryCountsResult.Match<ActionResult<List<RuleCategory>>>(
                categoryCounts => Ok(categoryCounts.Keys.ToList()),
                error => BadRequest(error.Message)
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("categories/{categoryName}")]
    public async Task<ActionResult<List<RuleRecord>>> GetCategoryRules(string categoryName)
    {
        try
        {
            if (Enum.TryParse<RuleCategory>(categoryName, true, out var category))
            {
                var result = await _rulesRepository.GetRulesByCategoryAsync(category);
                return result.Match<ActionResult<List<RuleRecord>>>(
                    rules => Ok(rules.ToList()),
                    error => NotFound(error.Message)
                );
            }
            return BadRequest($"Invalid category name: {categoryName}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("categories/{categoryName}/rules/{ruleId}")]
    public async Task<ActionResult<RuleRecord>> GetRule(string categoryName, string ruleId)
    {
        try
        {
            var result = await _rulesRepository.GetRuleByIdAsync(new RuleId(ruleId));
            return result.Match<ActionResult<RuleRecord>>(
                rule => Ok(rule),
                error => NotFound(error.Message)
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<RuleRecord>>> SearchRules([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search query cannot be empty");
            }

            var result = await _rulesRepository.SearchRulesAsync(q);
            return result.Match<ActionResult<List<RuleRecord>>>(
                rules => Ok(rules.ToList()),
                error => BadRequest(error.Message)
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}