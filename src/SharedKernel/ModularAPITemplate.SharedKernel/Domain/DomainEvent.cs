namespace ModularAPITemplate.SharedKernel.Domain;

/// <summary>
/// Registro base para eventos de domínio com timestamp de ocorrência.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
