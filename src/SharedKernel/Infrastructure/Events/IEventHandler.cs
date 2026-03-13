namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

public interface IEventHandler<TEvent> 
    where TEvent : IEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}