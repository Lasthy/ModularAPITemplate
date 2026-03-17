using ModularAPITemplate.SharedKernel.Infrastructure.Events;

namespace ModularAPITemplate.SharedKernel.Domain.Components;

/// <summary>
/// Indicates that an entity produces domain events that should be dispatched.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Domain events produced by the entity.
    /// </summary>
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears domain events after they have been dispatched.
    /// </summary>
    void ClearDomainEvents();
}