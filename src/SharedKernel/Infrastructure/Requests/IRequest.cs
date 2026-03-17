namespace ModularAPITemplate.SharedKernel.Infrastructure.Requests;

/// <summary>
/// Marker interface for request types returning a response.
/// </summary>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IRequest<TResponse>
{
}

/// <summary>
/// Marker interface for request types that do not return a value.
/// </summary>
public interface IRequest
{
}