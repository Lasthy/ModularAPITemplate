using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Domain;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// DbContext base que despacha eventos de domínio automaticamente ao salvar.
/// Todos os módulos devem herdar deste DbContext.
/// </summary>
public abstract class BaseDbContext(
    DbContextOptions options,
    IPublisher publisher) : DbContext(options)
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
