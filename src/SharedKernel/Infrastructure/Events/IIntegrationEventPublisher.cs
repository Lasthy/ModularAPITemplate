using ModularAPITemplate.SharedKernel.Infrastructure.Events;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Publishes integration events into the outbox for later dispatch.
/// </summary>
/// <typeparam name="TContext">The database context type.</typeparam>
public interface IIntegrationEventPublisher<TContext>
     where TContext : IBaseDbContext
{
    /// <summary>
    /// Publishes the integration event by persisting it to the outbox table.
    /// </summary>
    Task PublishAsync(IntegrationEvent message, CancellationToken ct = default);
}