using MediatR;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Contrato para eventos de integração entre módulos.
/// Eventos de integração são publicados após a persistência e servem
/// para comunicação assíncrona entre módulos/serviços.
/// </summary>
public record IntegrationEvent : IEvent
{
    public Ulid EventId { get; } = Ulid.NewUlid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public UserIdType? ActorId { get; init; }
}
