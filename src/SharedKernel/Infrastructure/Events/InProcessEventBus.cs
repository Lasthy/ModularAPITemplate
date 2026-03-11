using MediatR;
using Microsoft.Extensions.Logging;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Implementação in-process do barramento de eventos.
/// Usa MediatR para despachar eventos dentro do mesmo processo.
/// Pode ser substituída por uma implementação externa (RabbitMQ, Kafka, etc).
/// </summary>
public sealed class InProcessEventBus(
    IPublisher publisher,
    ILogger<InProcessEventBus> logger) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent
    {
        logger.LogInformation(
            "Publishing integration event {EventType} (Id: {EventId})",
            integrationEvent.GetType().Name,
            integrationEvent.EventId);

        await publisher.Publish(integrationEvent, cancellationToken);
    }
}
