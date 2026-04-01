using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Requests;

/// <summary>
/// Default dispatcher implementation that resolves request handlers from the DI container.
/// </summary>
public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Sends a request expecting a response.
    /// </summary>
    public async Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));

        // Prefer GetService to avoid throwing during resolution.
        var handler = (IRequestHandler<TRequest, TResponse>?)_serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            return Result.Failure<TResponse>($"No handler found for request type {typeof(TRequest).FullName}.");
        }

        return await handler.HandleAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends a request that does not return a value.
    /// </summary>
    public async Task<Result> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(typeof(TRequest));

        var handler = (IRequestHandler<TRequest>?)_serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            return Result.Failure($"No handler found for request type {typeof(TRequest).FullName}.");
        }

        return await handler.HandleAsync(request, cancellationToken);
    }
}