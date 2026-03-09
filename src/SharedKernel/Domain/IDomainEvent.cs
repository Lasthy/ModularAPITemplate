using MediatR;

namespace ModularAPITemplate.SharedKernel.Domain;

/// <summary>
/// Contrato para eventos de domínio.
/// Eventos de domínio representam algo que aconteceu dentro de um agregado.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}
