using MediatR;
using PathfinderCampaignManager.Domain.Common;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Behaviors;

public class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only handle commands (not queries)
        if (!IsCommand(request))
            return await next();

        var response = await next();

        if (response.IsSuccess)
        {
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        else
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
        }

        return response;
    }

    private static bool IsCommand(TRequest request)
    {
        // Convention: Commands implement IRequest<Result> or IRequest<Result<T>>
        // Queries implement IRequest<Result<T>> where T is a data type
        var requestType = request.GetType();
        var requestName = requestType.Name;
        
        return requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase) ||
               requestType.GetInterfaces().Any(i => 
                   i.IsGenericType && 
                   i.GetGenericTypeDefinition() == typeof(IRequest<>) &&
                   i.GetGenericArguments().First() == typeof(Result));
    }
}