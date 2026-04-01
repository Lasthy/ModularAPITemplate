using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Workers;

/// <summary>
/// Processes pending outbox messages by deserializing and dispatching them to event handlers.
/// </summary>
public class OutboxProcessingWorker<TModule, TContext> : BaseWorker
    where TContext : IBaseDbContext
    where TModule : IModule
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessingWorker<TModule, TContext>> _logger;
    private readonly OutboxConfiguration<TModule> _configuration;

    public OutboxProcessingWorker(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxProcessingWorker<TModule, TContext>> logger, OutboxConfiguration<TModule> configuration)
        : base(serviceScopeFactory, logger, TimeSpan.FromSeconds(1))
    {
        _logger = logger;
        _scopeFactory = serviceScopeFactory;
        _configuration = configuration;

        Interval = TimeSpan.FromMilliseconds(_configuration.IntervalMilliseconds);
    }

    /// <summary>
    /// Background job that periodically claims and processes pending outbox messages.
    /// </summary>
    protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var enabled = _configuration.Enabled;

        if (!enabled)
        {
            _logger.LogInformation("OutboxProcessingWorker is disabled. Stopping execution.");

            RequestCancellation();
            return;
        }

        var context = services.GetRequiredService<TContext>();

        var partitions = _configuration.GetPartitions();

        var messages = await ClaimMessages(context, partitions, _configuration.BatchSize);

        if(messages.Length == 0)
            return;

        await Parallel.ForEachAsync(
            messages,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount > 1 ? Environment.ProcessorCount - 1 : 1
            },
            async (message, ct) =>
            {
                await using var services = _scopeFactory.CreateAsyncScope();

                await ProcessMessage(message, ct, services.ServiceProvider);
            }
        );
    }

    private async Task<OutboxMessage[]> ClaimMessages(IBaseDbContext db, int[] partitions, int batchSize)
    {
        var messages = Array.Empty<OutboxMessage>();

        var attempts = 0;

        while(attempts++ < _configuration.MaxRetryAttempts && messages.Length == 0)
        {
            try
            {
                var now = DateTime.UtcNow;

                messages = await db.OutboxMessages
                    .Where(x =>
                        partitions.Contains(x.Partition) &&
                        x.ProcessedAt == null &&
                        x.ProcessingAt == null &&
                        (x.NextRetryAt == null || x.NextRetryAt <= now))
                    .OrderBy(x => x.OccurredAt)
                    .Take(batchSize)
                    .ToArrayAsync();
                
                foreach (var msg in messages)
                    msg.ProcessingAt = now;

                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrency conflict while claiming outbox messages. Retrying...");

                await Task.Delay(Random.Shared.Next(10, 150));
            }
        }

        return messages;
    }

    private async Task ProcessMessage(OutboxMessage message, CancellationToken ct, IServiceProvider services)
    {
        try
        {
            var type = EventTypeRegistry.Resolve(message.Type);

            var @event = JsonSerializer.Deserialize(
                message.Content,
                type
            ) as IEvent;

            var handlerType = typeof(IEventHandler<>).MakeGenericType(type);

            var handlers = services.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync");

                if (method == null)
                    continue;

                var task = (Task)method.Invoke(handler, new object[] { @event!, ct })!;

                await task;
            }

            await MarkProcessed(message.Id);
        }
        catch (Exception ex)
        {
            await HandleFailure(message, ex);
        }
    }

    private async Task MarkProcessed(Ulid id)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        var msg = await db.OutboxMessages.FindAsync(id);

        msg!.ProcessedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    async Task HandleFailure(OutboxMessage message, Exception ex)
    {
        _logger.LogError(ex, "Failed to process outbox message with id {MessageId}", message.Id);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        var msg = await db.OutboxMessages.FindAsync(message.Id);

        msg!.RetryCount++;

        var delayMilliseconds = Math.Pow(2, Math.Max(msg.RetryCount, 5)) * _configuration.IntervalMilliseconds + 50; // just a tiny bit to ensure the next retry is after the base interval

        msg.NextRetryAt = DateTime.UtcNow.AddMilliseconds(delayMilliseconds);
        msg.Error = ex.ToString();

        await db.SaveChangesAsync();
    }
}