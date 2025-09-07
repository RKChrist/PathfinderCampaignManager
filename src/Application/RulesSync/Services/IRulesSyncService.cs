using PathfinderCampaignManager.Application.RulesSync.Models;

namespace PathfinderCampaignManager.Application.RulesSync.Services;

public interface IRulesSyncService
{
    Task<SyncResult> SyncAllContentAsync(CancellationToken cancellationToken = default);
    Task<SyncResult> SyncContentTypeAsync(ContentType contentType, CancellationToken cancellationToken = default);
    Task<SyncResult> SyncFromUrlAsync(string url, ContentType contentType, CancellationToken cancellationToken = default);
    Task<SyncProgress> GetSyncProgressAsync(string syncId, CancellationToken cancellationToken = default);
    Task<List<SyncHistory>> GetSyncHistoryAsync(int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> CancelSyncAsync(string syncId, CancellationToken cancellationToken = default);
}

public interface ISrdDownloader
{
    Task<SrdContent> DownloadContentAsync(string url, CancellationToken cancellationToken = default);
    Task<List<SrdIndex>> GetContentIndexAsync(CancellationToken cancellationToken = default);
    Task<SrdValidationResult> ValidateContentAsync(SrdContent content, CancellationToken cancellationToken = default);
}

public interface IRulesContentProcessor
{
    Task<ProcessingResult> ProcessClassesAsync(List<SrdContent> content, CancellationToken cancellationToken = default);
    Task<ProcessingResult> ProcessSpellsAsync(List<SrdContent> content, CancellationToken cancellationToken = default);
    Task<ProcessingResult> ProcessFeatsAsync(List<SrdContent> content, CancellationToken cancellationToken = default);
    Task<ProcessingResult> ProcessEquipmentAsync(List<SrdContent> content, CancellationToken cancellationToken = default);
    Task<ProcessingResult> ProcessAncestryAsync(List<SrdContent> content, CancellationToken cancellationToken = default);
    Task<ProcessingResult> ProcessBackgroundsAsync(List<SrdContent> content, CancellationToken cancellationToken = default);
}