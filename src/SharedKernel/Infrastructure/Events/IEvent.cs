namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

public interface IEvent
{
    public DateTime OccurredAt { get; }
    public UserIdType? ActorId { get; init; }
}