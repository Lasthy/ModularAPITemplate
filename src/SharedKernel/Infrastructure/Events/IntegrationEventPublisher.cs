using System.Text.Json;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Publishes integration events by persisting them into the outbox table.
/// </summary>
/// <typeparam name="TModule">Module type whose configuration is used.</typeparam>
/// <typeparam name="TContext">Database context type.</typeparam>
public class IntegrationEventPublisher<TModule, TContext> : IIntegrationEventPublisher<TContext>
     where TContext : IBaseDbContext
     where TModule : IModule
{
    private readonly TContext _db;
    private readonly OutboxConfiguration<TModule> _config;

    public IntegrationEventPublisher(TContext db, OutboxConfiguration<TModule> config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>
    /// Creates a new outbox entry for the event and saves it to the database.
    /// </summary>
    public async Task PublishAsync(IntegrationEvent message, CancellationToken ct = default)
    {
        var outbox = new OutboxMessage
        {
            Id = Ulid.NewUlid(),
            Type = message.GetType().FullName!,
            Content = JsonSerializer.Serialize<object>(message),
            OccurredAt = message.OccurredAt,
            ActorId = message.ActorId,
            Partition = Random.Shared.Next(_config.PartitionStart, _config.PartitionEnd + 1)
        };

        _db.Set<OutboxMessage>().Add(outbox);

        await _db.SaveChangesAsync(ct);
    }

    public bool CanPublish(IntegrationEvent message)
    {
        // Module-local events continue to be routed by assembly.
        if (message.GetType().Assembly == typeof(TModule).Assembly)
            return true;

        // Shared contracts can be routed by namespace convention:
        // ExampleStore.SharedKernel.IntegrationEvents.<ModuleName>.*
        var @namespace = message.GetType().Namespace;

        if (string.IsNullOrWhiteSpace(@namespace))
            return false;

        return @namespace.Contains($".IntegrationEvents.{TModule.ModuleName}", StringComparison.OrdinalIgnoreCase);
    }
}