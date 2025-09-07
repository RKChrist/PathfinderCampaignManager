using Microsoft.AspNetCore.Mvc;
using PathfinderCampaignManager.Application.RulesSync.Models;
using PathfinderCampaignManager.Application.RulesSync.Services;

namespace PathfinderCampaignManager.Presentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RulesSyncController : ControllerBase
{
    private readonly IRulesSyncService _syncService;
    private readonly ILogger<RulesSyncController> _logger;

    public RulesSyncController(IRulesSyncService syncService, ILogger<RulesSyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// Start a full content sync from the Pathfinder 2e SRD
    /// </summary>
    [HttpPost("sync/full")]
    public async Task<ActionResult<SyncResult>> StartFullSync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full content sync via API");
        
        try
        {
            var result = await _syncService.SyncAllContentAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start full sync");
            return StatusCode(500, new { error = "Failed to start sync", message = ex.Message });
        }
    }

    /// <summary>
    /// Start a sync for a specific content type
    /// </summary>
    [HttpPost("sync/{contentType}")]
    public async Task<ActionResult<SyncResult>> StartContentTypeSync(
        ContentType contentType, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting {ContentType} sync via API", contentType);
        
        try
        {
            var result = await _syncService.SyncContentTypeAsync(contentType, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start {ContentType} sync", contentType);
            return StatusCode(500, new { error = $"Failed to start {contentType} sync", message = ex.Message });
        }
    }

    /// <summary>
    /// Sync a single item from a specific URL
    /// </summary>
    [HttpPost("sync/url")]
    public async Task<ActionResult<SyncResult>> SyncFromUrl(
        [FromBody] SyncFromUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "URL is required" });
        }

        _logger.LogInformation("Starting URL sync for {Url} as {ContentType}", request.Url, request.ContentType);
        
        try
        {
            var result = await _syncService.SyncFromUrlAsync(request.Url, request.ContentType, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync from URL: {Url}", request.Url);
            return StatusCode(500, new { error = "Failed to sync from URL", message = ex.Message });
        }
    }

    /// <summary>
    /// Get the progress of an active sync operation
    /// </summary>
    [HttpGet("progress/{syncId}")]
    public async Task<ActionResult<SyncProgress>> GetSyncProgress(string syncId)
    {
        try
        {
            var progress = await _syncService.GetSyncProgressAsync(syncId);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync progress for ID: {SyncId}", syncId);
            return StatusCode(500, new { error = "Failed to get sync progress", message = ex.Message });
        }
    }

    /// <summary>
    /// Get sync history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<SyncHistory>>> GetSyncHistory(
        [FromQuery] int pageSize = 20)
    {
        if (pageSize <= 0 || pageSize > 100)
        {
            return BadRequest(new { error = "Page size must be between 1 and 100" });
        }

        try
        {
            var history = await _syncService.GetSyncHistoryAsync(pageSize);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync history");
            return StatusCode(500, new { error = "Failed to get sync history", message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an active sync operation
    /// </summary>
    [HttpPost("cancel/{syncId}")]
    public async Task<ActionResult<bool>> CancelSync(string syncId)
    {
        try
        {
            var cancelled = await _syncService.CancelSyncAsync(syncId);
            return Ok(new { cancelled, syncId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel sync: {SyncId}", syncId);
            return StatusCode(500, new { error = "Failed to cancel sync", message = ex.Message });
        }
    }

    /// <summary>
    /// Get available content types for sync
    /// </summary>
    [HttpGet("content-types")]
    public ActionResult<ContentTypeInfo[]> GetContentTypes()
    {
        var contentTypes = Enum.GetValues<ContentType>()
            .Where(ct => ct != ContentType.All)
            .Select(ct => new ContentTypeInfo
            {
                Type = ct,
                Name = ct.ToString(),
                Description = GetContentTypeDescription(ct)
            })
            .ToArray();

        return Ok(contentTypes);
    }

    private static string GetContentTypeDescription(ContentType contentType) => contentType switch
    {
        ContentType.Classes => "Character classes like Fighter, Wizard, Rogue, etc.",
        ContentType.Spells => "All spells from cantrips to 10th level",
        ContentType.Feats => "General, skill, ancestry, and class feats",
        ContentType.Equipment => "Weapons, armor, and general equipment",
        ContentType.Weapons => "Weapons and weapon-specific data",
        ContentType.Armor => "Armor and armor-specific data",
        ContentType.Ancestry => "Player character ancestries like Human, Elf, Dwarf, etc.",
        ContentType.Backgrounds => "Character backgrounds that provide ability boosts and skills",
        ContentType.Traits => "Universal traits and keywords",
        ContentType.Conditions => "Status conditions that affect characters",
        ContentType.Actions => "Basic and special actions available to characters",
        ContentType.Rules => "General game rules and mechanics",
        _ => "Unknown content type"
    };
}

public class SyncFromUrlRequest
{
    public string Url { get; set; } = string.Empty;
    public ContentType ContentType { get; set; } = ContentType.Rules;
}

public class ContentTypeInfo
{
    public ContentType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}