using FluentValidation;
using MediatR;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Errors;

namespace PathfinderCampaignManager.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(result => result.Errors.Count != 0)
            .SelectMany(result => result.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var error = new DomainError("VALIDATION.FAILED", errorMessage);
            
            return CreateValidationResult<TResponse>(error);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(DomainError error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)(object)Result.Failure(error);
        }

        object validationResult = typeof(Result<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(Result.Failure))!
            .Invoke(null, [error])!;

        return (TResult)validationResult;
    }
}