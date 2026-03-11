using MediatR;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Contrato para eventos de integração entre módulos.
/// Eventos de integração são publicados após a persistência e servem
/// para comunicação assíncrona entre módulos/serviços.
/// </summary>
public interface IIntegrationEvent : INotification
{
    Ulid EventId { get; }
    DateTime OccurredAt { get; }
}
