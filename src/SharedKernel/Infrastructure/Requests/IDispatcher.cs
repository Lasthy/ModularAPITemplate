using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Requests;

public interface IDispatcher
{
    Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;
    Task<Result> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;
}