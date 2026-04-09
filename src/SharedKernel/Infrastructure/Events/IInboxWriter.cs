using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Application;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Infrastructure.Json;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Writes integration events to the inbox for reliable processing within a module.
/// Deduplicates by event ID to prevent double-processing.
/// </summary>
public interface IInboxWriter<TContext> where TContext : IBaseDbContext
{
    /// <summary>
    /// Persists an integration event to the inbox if it hasn't been received before.
    /// </summary>
    /// <returns>True if the message was written, false if it was a duplicate.</returns>
    Task<Result> WriteAsync(IntegrationEvent @event, CancellationToken ct = default);
}

public class InboxWriter<TModule, TContext> : IInboxWriter<TContext>
    where TModule : IModule
    where TContext : IBaseDbContext
{
    private readonly TContext _db;
    private readonly InboxConfiguration<TModule> _config;
    private readonly IJsonSerializer _jsonSerializer;

    public InboxWriter(TContext db, InboxConfiguration<TModule> config, IJsonSerializer jsonSerializer)
    {
        _db = db;
        _config = config;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<Result> WriteAsync(IntegrationEvent @event, CancellationToken ct = default)
    {
        var message = await _db.InboxMessages.FirstOrDefaultAsync(m => m.Id == @event.EventId, ct);

        if (message is not null)
            return Result.Success("Event has already been added to inbox.");

        message = new InboxMessage
        {
            Id = @event.EventId,
            Type = @event.GetType().FullName!,
            Content = _jsonSerializer.Serialize(@event),
            OccurredAt = @event.OccurredAt,
            Partition = Random.Shared.Next(_config.PartitionStart, _config.PartitionEnd + 1)
        };

        _db.InboxMessages.Add(message);

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}