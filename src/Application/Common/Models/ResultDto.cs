namespace PathfinderCampaignManager.Application.Common.Models;

public class ResultDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }

    public static ResultDto Success() => new() { IsSuccess = true };
    
    public static ResultDto Failure(string errorCode, string errorMessage, string? errorDetails = null) =>
        new() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage, ErrorDetails = errorDetails };

    public static ResultDto<T> Success<T>(T data) => new() { IsSuccess = true, Data = data };
    
    public static ResultDto<T> Failure<T>(string errorCode, string errorMessage, string? errorDetails = null) =>
        new() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage, ErrorDetails = errorDetails };
}

public class ResultDto<T> : ResultDto
{
    public T? Data { get; set; }
}