namespace ModularAPITemplate.SharedKernel.Domain.Components;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}