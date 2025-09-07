using Microsoft.Extensions.Logging;
using PathfinderCampaignManager.Application.RulesSync.Models;
using PathfinderCampaignManager.Application.RulesSync.Services;
using PathfinderCampaignManager.Domain.Interfaces;
using System.Collections.Concurrent;

namespace PathfinderCampaignManager.Infrastructure.RulesSync;

public class RulesSyncService : IRulesSyncService
{
    private readonly ISrdDownloader _srdDownloader;
    private readonly IRulesContentProcessor _contentProcessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RulesSyncService> _logger;
    private readonly ConcurrentDictionary<string, SyncProgress> _activeSyncs = new();
    private readonly List<SyncHistory> _syncHistory = new();

    public RulesSyncService(
        ISrdDownloader srdDownloader,
        IRulesContentProcessor contentProcessor,
        IUnitOfWork unitOfWork,
        ILogger<RulesSyncService> logger)
    {
        _srdDownloader = srdDownloader;
        _contentProcessor = contentProcessor;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SyncResult> SyncAllContentAsync(CancellationToken cancellationToken = default)
    {
        var syncId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting full content sync with ID: {SyncId}", syncId);

        var result = new SyncResult
        {
            SyncId = syncId,
            StartTime = DateTime.UtcNow
        };

        var progress = new SyncProgress
        {
            SyncId = syncId,
            Status = SyncStatus.InProgress,
            StartTime = DateTime.UtcNow,
            CurrentOperation = "Initializing full sync"
        };

        _activeSyncs[syncId] = progress;

        try
        {
            // Get the content index first
            progress.CurrentOperation = "Fetching content index";
            var contentIndex = await _srdDownloader.GetContentIndexAsync(cancellationToken);
            progress.TotalItems = contentIndex.Count;

            var allContent = new List<SrdContent>();
            var errors = new List<SyncError>();

            // Download all content
            foreach (var indexItem in contentIndex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Sync was cancelled";
                    progress.Status = SyncStatus.Cancelled;
                    break;
                }

                progress.CurrentOperation = $"Downloading {indexItem.Type}: {indexItem.Name}";
                
                try
                {
                    var content = await _srdDownloader.DownloadContentAsync(indexItem.Url, cancellationToken);
                    
                    // Validate content
                    var validation = await _srdDownloader.ValidateContentAsync(content, cancellationToken);
                    if (validation.IsValid)
                    {
                        allContent.Add(content);
                        result.ItemsProcessed++;
                    }
                    else
                    {
                        result.ItemsFailed++;
                        errors.Add(new SyncError
                        {
                            ItemId = indexItem.Url,
                            ItemName = indexItem.Name,
                            ContentType = indexItem.Type,
                            ErrorMessage = string.Join("; ", validation.Errors.Select(e => e.Message))
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to download content: {Name} from {Url}", indexItem.Name, indexItem.Url);
                    result.ItemsFailed++;
                    errors.Add(new SyncError
                    {
                        ItemId = indexItem.Url,
                        ItemName = indexItem.Name,
                        ContentType = indexItem.Type,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    });
                }

                progress.ProcessedItems = result.ItemsProcessed + result.ItemsFailed;
                progress.RecentOperations.Add($"Processed {indexItem.Name}");
                if (progress.RecentOperations.Count > 10)
                    progress.RecentOperations.RemoveAt(0);

                // Add small delay to be respectful to the server
                await Task.Delay(1000, cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                // Process content by type
                await ProcessContentByTypeAsync(allContent, result, progress, cancellationToken);
            }

            result.EndTime = DateTime.UtcNow;
            result.Errors = errors;
            result.IsSuccess = result.ItemsFailed == 0 && !cancellationToken.IsCancellationRequested;
            
            progress.Status = result.IsSuccess ? SyncStatus.Completed : 
                cancellationToken.IsCancellationRequested ? SyncStatus.Cancelled : SyncStatus.Failed;

            _logger.LogInformation("Completed full content sync. Success: {Success}, Processed: {Processed}, Failed: {Failed}", 
                result.IsSuccess, result.ItemsProcessed, result.ItemsFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full content sync failed with exception");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            progress.Status = SyncStatus.Failed;
        }
        finally
        {
            _activeSyncs.TryRemove(syncId, out _);
            AddToSyncHistory(result, ContentType.All);
        }

        return result;
    }

    public async Task<SyncResult> SyncContentTypeAsync(ContentType contentType, CancellationToken cancellationToken = default)
    {
        var syncId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting {ContentType} sync with ID: {SyncId}", contentType, syncId);

        var result = new SyncResult
        {
            SyncId = syncId,
            StartTime = DateTime.UtcNow
        };

        var progress = new SyncProgress
        {
            SyncId = syncId,
            Status = SyncStatus.InProgress,
            StartTime = DateTime.UtcNow,
            CurrentOperation = $"Initializing {contentType} sync"
        };

        _activeSyncs[syncId] = progress;

        try
        {
            // Get content index for specific type
            progress.CurrentOperation = $"Fetching {contentType} index";
            var contentIndex = await _srdDownloader.GetContentIndexAsync(cancellationToken);
            var typeContent = contentIndex.Where(c => c.Type == contentType).ToList();
            progress.TotalItems = typeContent.Count;

            var downloadedContent = new List<SrdContent>();
            var errors = new List<SyncError>();

            // Download content for this type
            foreach (var indexItem in typeContent)
            {
                if (cancellationToken.IsCancellationRequested) break;

                progress.CurrentOperation = $"Downloading: {indexItem.Name}";
                
                try
                {
                    var content = await _srdDownloader.DownloadContentAsync(indexItem.Url, cancellationToken);
                    var validation = await _srdDownloader.ValidateContentAsync(content, cancellationToken);
                    
                    if (validation.IsValid)
                    {
                        downloadedContent.Add(content);
                        result.ItemsProcessed++;
                    }
                    else
                    {
                        result.ItemsFailed++;
                        errors.Add(new SyncError
                        {
                            ItemId = indexItem.Url,
                            ItemName = indexItem.Name,
                            ContentType = contentType,
                            ErrorMessage = string.Join("; ", validation.Errors.Select(e => e.Message))
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to download {ContentType}: {Name}", contentType, indexItem.Name);
                    result.ItemsFailed++;
                    errors.Add(new SyncError
                    {
                        ItemId = indexItem.Url,
                        ItemName = indexItem.Name,
                        ContentType = contentType,
                        ErrorMessage = ex.Message
                    });
                }

                progress.ProcessedItems = result.ItemsProcessed + result.ItemsFailed;
                await Task.Delay(1000, cancellationToken); // Rate limiting
            }

            if (!cancellationToken.IsCancellationRequested && downloadedContent.Any())
            {
                // Process the downloaded content
                progress.CurrentOperation = $"Processing {contentType} data";
                var processingResult = await ProcessSpecificContentType(contentType, downloadedContent, cancellationToken);
                
                result.Metadata["created"] = processingResult.ItemsCreated;
                result.Metadata["updated"] = processingResult.ItemsUpdated;
                result.Metadata["skipped"] = processingResult.ItemsSkipped;
            }

            result.EndTime = DateTime.UtcNow;
            result.Errors = errors;
            result.IsSuccess = result.ItemsFailed == 0 && !cancellationToken.IsCancellationRequested;
            progress.Status = result.IsSuccess ? SyncStatus.Completed : SyncStatus.Failed;

            _logger.LogInformation("Completed {ContentType} sync. Success: {Success}, Processed: {Processed}, Failed: {Failed}", 
                contentType, result.IsSuccess, result.ItemsProcessed, result.ItemsFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ContentType} sync failed with exception", contentType);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            progress.Status = SyncStatus.Failed;
        }
        finally
        {
            _activeSyncs.TryRemove(syncId, out _);
            AddToSyncHistory(result, contentType);
        }

        return result;
    }

    public async Task<SyncResult> SyncFromUrlAsync(string url, ContentType contentType, CancellationToken cancellationToken = default)
    {
        var syncId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting single URL sync with ID: {SyncId} for {Url}", syncId, url);

        var result = new SyncResult
        {
            SyncId = syncId,
            StartTime = DateTime.UtcNow
        };

        try
        {
            var content = await _srdDownloader.DownloadContentAsync(url, cancellationToken);
            var validation = await _srdDownloader.ValidateContentAsync(content, cancellationToken);

            if (validation.IsValid)
            {
                var processingResult = await ProcessSpecificContentType(contentType, new[] { content }, cancellationToken);
                result.ItemsProcessed = 1;
                result.IsSuccess = processingResult.IsSuccess;
                result.Metadata = processingResult.Statistics;
            }
            else
            {
                result.ItemsFailed = 1;
                result.IsSuccess = false;
                result.ErrorMessage = string.Join("; ", validation.Errors.Select(e => e.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Single URL sync failed for {Url}", url);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ItemsFailed = 1;
        }

        result.EndTime = DateTime.UtcNow;
        AddToSyncHistory(result, contentType);
        return result;
    }

    public Task<SyncProgress> GetSyncProgressAsync(string syncId, CancellationToken cancellationToken = default)
    {
        _activeSyncs.TryGetValue(syncId, out var progress);
        return Task.FromResult(progress ?? new SyncProgress { SyncId = syncId, Status = SyncStatus.Completed });
    }

    public Task<List<SyncHistory>> GetSyncHistoryAsync(int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var history = _syncHistory
            .OrderByDescending(h => h.StartTime)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(history);
    }

    public Task<bool> CancelSyncAsync(string syncId, CancellationToken cancellationToken = default)
    {
        if (_activeSyncs.TryGetValue(syncId, out var progress))
        {
            progress.Status = SyncStatus.Cancelled;
            progress.CurrentOperation = "Cancellation requested";
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    private async Task ProcessContentByTypeAsync(List<SrdContent> allContent, SyncResult result, SyncProgress progress, CancellationToken cancellationToken)
    {
        var contentByType = allContent.GroupBy(c => c.Type).ToList();

        foreach (var group in contentByType)
        {
            if (cancellationToken.IsCancellationRequested) break;

            progress.CurrentOperation = $"Processing {group.Key} data";
            
            try
            {
                var processingResult = await ProcessSpecificContentType(group.Key, group.ToList(), cancellationToken);
                
                result.Metadata[$"{group.Key}_created"] = processingResult.ItemsCreated;
                result.Metadata[$"{group.Key}_updated"] = processingResult.ItemsUpdated;
                result.Metadata[$"{group.Key}_skipped"] = processingResult.ItemsSkipped;
                result.Metadata[$"{group.Key}_failed"] = processingResult.ItemsFailed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process {ContentType} data", group.Key);
                result.Errors.Add(new SyncError
                {
                    ItemId = group.Key.ToString(),
                    ItemName = $"{group.Key} Processing",
                    ContentType = group.Key,
                    ErrorMessage = ex.Message
                });
            }
        }
    }

    private async Task<ProcessingResult> ProcessSpecificContentType(ContentType contentType, IEnumerable<SrdContent> content, CancellationToken cancellationToken)
    {
        var contentList = content.ToList();
        
        return contentType switch
        {
            ContentType.Classes => await _contentProcessor.ProcessClassesAsync(contentList, cancellationToken),
            ContentType.Spells => await _contentProcessor.ProcessSpellsAsync(contentList, cancellationToken),
            ContentType.Feats => await _contentProcessor.ProcessFeatsAsync(contentList, cancellationToken),
            ContentType.Equipment => await _contentProcessor.ProcessEquipmentAsync(contentList, cancellationToken),
            ContentType.Ancestry => await _contentProcessor.ProcessAncestryAsync(contentList, cancellationToken),
            ContentType.Backgrounds => await _contentProcessor.ProcessBackgroundsAsync(contentList, cancellationToken),
            _ => new ProcessingResult { IsSuccess = true } // Skip unknown types
        };
    }

    private void AddToSyncHistory(SyncResult result, ContentType contentType)
    {
        var history = new SyncHistory
        {
            SyncId = result.SyncId,
            ContentType = contentType,
            Status = result.IsSuccess ? SyncStatus.Completed : SyncStatus.Failed,
            ItemsProcessed = result.ItemsProcessed,
            ItemsFailed = result.ItemsFailed,
            StartTime = result.StartTime,
            EndTime = result.EndTime,
            Duration = result.Duration,
            ErrorSummary = result.ErrorMessage,
            InitiatedBy = "System" // Would be replaced with actual user info
        };

        _syncHistory.Add(history);
        
        // Keep only last 100 entries
        if (_syncHistory.Count > 100)
        {
            _syncHistory.RemoveAt(0);
        }
    }
}