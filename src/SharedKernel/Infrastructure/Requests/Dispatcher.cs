using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Requests;

public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));

        var handler = (IRequestHandler<TRequest, TResponse>)_serviceProvider.GetService(handlerType)!;

        if (handler == null)
        {
            return Result.Failure<TResponse>($"No handler found for request type {typeof(TRequest).FullName}.");
        }

        return await handler.HandleAsync(request, cancellationToken);
    }

    public async Task<Result> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(typeof(TRequest));

        var handler = (IRequestHandler<TRequest>)_serviceProvider.GetService(handlerType)!;

        if (handler == null)
        {
            return Result.Failure($"No handler found for request type {typeof(TRequest).FullName}.");
        }

        return await handler.HandleAsync(request, cancellationToken);
    }
}