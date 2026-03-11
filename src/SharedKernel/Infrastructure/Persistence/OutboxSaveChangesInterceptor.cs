using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using ModularAPITemplate.SharedKernel.Domain.Components;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

public class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly OutboxConfiguration _configuration;

    public OutboxSaveChangesInterceptor(OutboxConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var domainEvents = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            var message = new OutboxMessage
            {
                Id = Ulid.NewUlid(),
                Type = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent),
                OccurredAt = domainEvent.OccurredAt,
            };

            message.Partition = message.Id.GetHashCode() % _configuration.PartitionCount;

            context.Set<OutboxMessage>().Add(message);
        }

        foreach (var entity in context.ChangeTracker.Entries<IHasDomainEvents>())
        {
            entity.Entity.ClearDomainEvents();
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}