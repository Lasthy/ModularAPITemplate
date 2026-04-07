using ModularAPITemplate.SharedKernel.Infrastructure.Events;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Publishes integration events into the outbox for later dispatch.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes the integration event by persisting it to the outbox table.
    /// </summary>
    Task PublishAsync(IntegrationEvent message, CancellationToken ct = default);
    bool CanPublish(IntegrationEvent message);
}

/// <summary>
/// Context-specific publisher contract kept for modules that need typed registrations.
/// </summary>
/// <typeparam name="TContext">The database context type.</typeparam>
public interface IIntegrationEventPublisher<TContext> : IIntegrationEventPublisher
     where TContext : IBaseDbContext
{
}