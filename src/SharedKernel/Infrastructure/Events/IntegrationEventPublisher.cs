using System.Text.Json;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

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

    public async Task PublishAsync(IntegrationEvent message, CancellationToken ct = default)
    {
        var outbox = new OutboxMessage
        {
            Id = Ulid.NewUlid(),
            Type = message.GetType().FullName!,
            Content = JsonSerializer.Serialize(message),
            OccurredAt = message.OccurredAt,
            ActorId = message.ActorId,
            Partition = Random.Shared.Next(_config.PartitionStart, _config.PartitionEnd + 1)
        };

        _db.Set<OutboxMessage>().Add(outbox);

        await _db.SaveChangesAsync(ct);
    }
}