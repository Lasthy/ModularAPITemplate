using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Requests;

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<TRequest>
    where TRequest : IRequest
{
    Task<Result> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}