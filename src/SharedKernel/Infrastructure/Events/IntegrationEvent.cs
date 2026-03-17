namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Base class for integration events that are sent across module boundaries.
/// </summary>
public abstract record IntegrationEvent : IEvent
{
    /// <summary>
    /// Unique identifier for the integration event.
    /// </summary>
    public Ulid EventId { get; } = Ulid.NewUlid();

    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public UserIdType? ActorId { get; init; }
}
