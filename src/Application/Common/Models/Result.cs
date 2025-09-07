namespace PathfinderCampaignManager.Application.Common.Models;

public class Result
{
    protected Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public string? Error { get; }
    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

public class Result<T> : Result
{
    protected internal Result(T value, bool isSuccess, string? error = null) 
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T Value { get; }
}