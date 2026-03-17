using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Requests;

/// <summary>
/// Dispatches request objects to their registered handlers.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends a request expecting a response.
    /// </summary>
    Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Sends a request that does not return a value.
    /// </summary>
    Task<Result> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;
}