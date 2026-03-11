using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Workers;

public class OutboxWorker<TContext> : BaseWorker
    where TContext : IBaseDbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxWorker<TContext>> _logger;
    private readonly IPublisher _publisher;
    private readonly OutboxConfiguration _configuration;

    public OutboxWorker(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxWorker<TContext>> logger, IPublisher publisher, OutboxConfiguration configuration)
        : base(serviceScopeFactory, logger, TimeSpan.FromSeconds(1))
    {
        _logger = logger;
        _publisher = publisher;
        _scopeFactory = serviceScopeFactory;
        _configuration = configuration;

        Interval = TimeSpan.FromMilliseconds(_configuration.IntervalMilliseconds);
    }

    protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var enabled = _configuration.Enabled;

        if (!enabled)
        {
            _logger.LogInformation("OutboxWorker is disabled. Stopping execution.");

            RequestCancellation();
            return;
        }

        var context = services.GetRequiredService<TContext>();

        var partitionStart = _configuration.PartitionStart;
        var partitionEnd = _configuration.PartitionEnd;
        var batchSize = _configuration.BatchSize;

        var partitions = Enumerable.Range(partitionStart, partitionEnd - partitionStart + 1).ToArray();

        var messages = await ClaimMessages(context, partitions, batchSize);

        if(messages.Count == 0)
            return;

        await Parallel.ForEachAsync(
            messages,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            },
            async (message, ct) =>
            {
                await ProcessMessage(message, ct);
            }
        );
    }

    private async Task<List<OutboxMessage>> ClaimMessages(IBaseDbContext db, int[] partitions, int batchSize)
    {
        var now = DateTime.UtcNow;

        var messages = await db.OutboxMessages
            .Where(x =>
                partitions.Contains(x.Partition) &&
                x.ProcessedAt == null &&
                x.ProcessingAt == null &&
                (x.NextRetryAt == null || x.NextRetryAt <= now))
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync();

        foreach (var msg in messages)
            msg.ProcessingAt = now;

        await db.SaveChangesAsync();

        return messages;
    }

    private async Task ProcessMessage(OutboxMessage message, CancellationToken ct)
    {
        try
        {
            var type = EventTypeRegistry.Resolve(message.Type);

            var notification = JsonSerializer.Deserialize(
                message.Content,
                type
            ) as INotification;

            await _publisher.Publish(notification!, ct);

            await MarkProcessed(message.Id);
        }
        catch (Exception ex)
        {
            await HandleFailure(message, ex);
        }
    }

    private async Task MarkProcessed(Ulid id)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<IBaseDbContext>();

        var msg = await db.OutboxMessages.FindAsync(id);

        msg!.ProcessedAt = DateTime.UtcNow;
        msg.ProcessingAt = null;

        await db.SaveChangesAsync();
    }

    async Task HandleFailure(OutboxMessage message, Exception ex)
    {
        _logger.LogError(ex, "Failed to process outbox message with id {MessageId}", message.Id);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IBaseDbContext>();

        var msg = await db.OutboxMessages.FindAsync(message.Id);

        msg!.RetryCount++;

        var delaySeconds = Math.Pow(2, msg.RetryCount);

        msg.NextRetryAt = DateTime.UtcNow.AddSeconds(delaySeconds);
        msg.ProcessingAt = null;
        msg.Error = ex.ToString();

        await db.SaveChangesAsync();
    }
}