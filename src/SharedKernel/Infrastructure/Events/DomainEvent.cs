namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Base class for domain events emitted within the current bounded context.
/// </summary>
public abstract record DomainEvent : IEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public UserIdType? ActorId { get; init; }
}
