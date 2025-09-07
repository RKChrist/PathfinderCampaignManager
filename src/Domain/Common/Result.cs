using PathfinderCampaignManager.Domain.Errors;

namespace PathfinderCampaignManager.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, DomainError error)
    {
        if (isSuccess && error != DomainError.None)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == DomainError.None)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public DomainError Error { get; }

    public static Result Success() => new(true, DomainError.None);
    public static Result Failure(DomainError error) => new(false, error);
    
    public static Result<T> Success<T>(T value) => new(value, true, DomainError.None);
    public static Result<T> Failure<T>(DomainError error) => new(default, false, error);

    public static implicit operator Result(DomainError error) => Failure(error);

    public Result Match(Action onSuccess, Action<DomainError> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error);

        return this;
    }

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<DomainError, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error);
    }
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, DomainError error) : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<T>(T? value) => value is not null ? Success(value) : Failure<T>(GeneralErrors.NotFound);
    public static implicit operator Result<T>(DomainError error) => Failure<T>(error);

    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return IsSuccess ? Success(mapper(Value)) : Failure<TResult>(Error);
    }

    public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
    {
        return IsSuccess ? Success(await mapper(Value)) : Failure<TResult>(Error);
    }

    public Result<T> Match(Action<T> onSuccess, Action<DomainError> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value);
        else
            onFailure(Error);

        return this;
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainError, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(Value);

        return this;
    }

    public async Task<Result<T>> TapAsync(Func<T, Task> action)
    {
        if (IsSuccess)
            await action(Value);

        return this;
    }

    public Result<T> TapError(Action<DomainError> action)
    {
        if (IsFailure)
            action(Error);

        return this;
    }
}