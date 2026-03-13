using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Implementação in-process do barramento de eventos.
/// Resolve e executa todos os handlers registrados para cada evento.
/// Pode ser substituída por uma implementação externa (RabbitMQ, Kafka, etc).
/// </summary>
public sealed class InProcessEventBus<TContext>
(
    IServiceProvider serviceProvider,
    ILogger<InProcessEventBus<TContext>> logger) : IEventBus
where TContext : IBaseDbContext
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : IEvent
    {
        if(@event is IntegrationEvent integrationEvent)
        {
            var integrationEventPublisher = serviceProvider.GetRequiredService<IIntegrationEventPublisher<TContext>>();

            logger.LogInformation(
            "Publishing integration event {EventType} (Id: {EventId})",
                @integrationEvent.GetType().Name,
                @integrationEvent.EventId);

            await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

            return;
        }

        var handlers = serviceProvider.GetServices<IEventHandler<T>>();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }
}
