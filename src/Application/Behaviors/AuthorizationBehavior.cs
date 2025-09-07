using MediatR;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Errors;

namespace PathfinderCampaignManager.Application.Behaviors;

public interface IAuthorizedRequest
{
    Task<Result> AuthorizeAsync(ICurrentUserService currentUser, IAuthorizationService authService, CancellationToken cancellationToken);
}

public class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserService currentUser,
    IAuthorizationService authorizationService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IAuthorizedRequest authorizedRequest)
            return await next();

        if (!_currentUser.IsAuthenticated)
        {
            return CreateUnauthorizedResult<TResponse>();
        }

        var authResult = await authorizedRequest.AuthorizeAsync(_currentUser, _authorizationService, cancellationToken);

        if (authResult.IsFailure)
        {
            return CreateAuthorizationFailureResult<TResponse>(authResult.Error);
        }

        return await next();
    }

    private static TResult CreateUnauthorizedResult<TResult>() where TResult : Result
    {
        var error = AuthorizationErrors.Unauthorized;

        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)(object)Result.Failure(error);
        }

        object result = typeof(Result<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(Result.Failure))!
            .Invoke(null, [error])!;

        return (TResult)result;
    }

    private static TResult CreateAuthorizationFailureResult<TResult>(DomainError error) where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)(object)Result.Failure(error);
        }

        object result = typeof(Result<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(Result.Failure))!
            .Invoke(null, [error])!;

        return (TResult)result;
    }
}