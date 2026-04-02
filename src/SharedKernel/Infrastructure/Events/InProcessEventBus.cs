using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Default in-process implementation of <see cref="IEventBus"/>.
/// Resolves and executes all registered <see cref="IEventHandler{T}"/> instances for each event.
/// Can be replaced with a broker-based implementation (RabbitMQ, Kafka, etc.).
/// </summary>
public sealed class InProcessEventBus
(
    IServiceProvider serviceProvider,
    ILogger<InProcessEventBus> logger) : IEventBus
{
    /// <summary>
    /// Publishes an event instance to all subscribed handlers.
    /// If the event is an <see cref="IntegrationEvent"/>, it is forwarded to an <see cref="IIntegrationEventPublisher"/>.
    /// </summary>
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : IEvent
    {
        if (@event is IntegrationEvent integrationEvent)
        {
            var integrationEventPublisher = serviceProvider.GetRequiredService<IIntegrationEventPublisher>();

            logger.LogInformation(
                "Publishing integration event {EventType} (Id: {EventId})",
                @integrationEvent.GetType().Name,
                @integrationEvent.EventId);

            await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

            return;
        }

        // Resolve all handlers registered for this event type.
        var handlers = serviceProvider.GetServices<IEventHandler<T>>();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }
}
