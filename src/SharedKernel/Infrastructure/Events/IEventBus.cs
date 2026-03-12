namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Barramento de eventos de integração.
/// Permite publicar e assinar eventos entre módulos.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IntegrationEvent;
}
