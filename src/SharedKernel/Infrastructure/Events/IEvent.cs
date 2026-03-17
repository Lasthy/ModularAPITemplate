namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Marker interface for domain and integration events.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Optional actor identifier related to the event.
    /// </summary>
    public UserIdType? ActorId { get; init; }
}