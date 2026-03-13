namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

public abstract record DomainEvent : IEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public UserIdType? ActorId { get; init; }
}
