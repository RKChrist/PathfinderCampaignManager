using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Application.RulesSync.Models;

public class SyncResult
{
    public string SyncId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int ItemsProcessed { get; set; }
    public int ItemsSkipped { get; set; }
    public int ItemsFailed { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? TimeSpan.Zero;
    public List<SyncError> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SyncProgress
{
    public string SyncId { get; set; } = string.Empty;
    public SyncStatus Status { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int FailedItems { get; set; }
    public string CurrentOperation { get; set; } = string.Empty;
    public decimal PercentageComplete => TotalItems > 0 ? (decimal)ProcessedItems / TotalItems * 100 : 0;
    public DateTime StartTime { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public List<string> RecentOperations { get; set; } = new();
}

public class SyncHistory
{
    public string SyncId { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public SyncStatus Status { get; set; }
    public int ItemsProcessed { get; set; }
    public int ItemsFailed { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorSummary { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
}

public class SyncError
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public SyncErrorSeverity Severity { get; set; } = SyncErrorSeverity.Error;
}

public class SrdContent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ContentType Type { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
    public Dictionary<string, object> ParsedData { get; set; } = new();
    public string Version { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class SrdIndex
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ContentType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> Traits { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string Checksum { get; set; } = string.Empty;
}

public class SrdValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public ContentType DetectedType { get; set; }
    public string? SuggestedName { get; set; }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ActualValue { get; set; }
}

public class ValidationWarning
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
}

public class ProcessingResult
{
    public bool IsSuccess { get; set; }
    public int ItemsCreated { get; set; }
    public int ItemsUpdated { get; set; }
    public int ItemsSkipped { get; set; }
    public int ItemsFailed { get; set; }
    public List<ProcessingError> Errors { get; set; } = new();
    public Dictionary<string, object> Statistics { get; set; } = new();
}

public class ProcessingError
{
    public string ItemName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? Field { get; set; }
    public ProcessingErrorType Type { get; set; }
}

public enum ContentType
{
    All = 0,
    Classes = 1,
    Spells = 2,
    Feats = 3,
    Equipment = 4,
    Weapons = 5,
    Armor = 6,
    Ancestry = 7,
    Backgrounds = 8,
    Traits = 9,
    Conditions = 10,
    Actions = 11,
    Rules = 12
}

public enum SyncStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    PartiallyCompleted = 6
}

public enum SyncErrorSeverity
{
    Warning = 1,
    Error = 2,
    Critical = 3
}

public enum ProcessingErrorType
{
    ValidationError = 1,
    DataConflict = 2,
    DatabaseError = 3,
    ParseError = 4,
    NetworkError = 5
}