using ModularAPITemplate.SharedKernel.Infrastructure.Events;

namespace ModularAPITemplate.SharedKernel.Domain.Components;

public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}