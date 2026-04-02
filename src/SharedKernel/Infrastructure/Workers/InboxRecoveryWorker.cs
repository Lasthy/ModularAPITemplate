using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Workers;

/// <summary>
/// Periodically scans for inbox messages that were claimed but not processed and reclaims them.
/// </summary>
public class InboxRecoveryWorker<TModule, TContext> : BaseWorker
    where TContext : IBaseDbContext
    where TModule : IModule
{
    private readonly ILogger<InboxRecoveryWorker<TModule, TContext>> _logger;
    private readonly InboxConfiguration<TModule> _configuration;

    public InboxRecoveryWorker(IServiceScopeFactory serviceScopeFactory, ILogger<InboxRecoveryWorker<TModule, TContext>> logger, InboxConfiguration<TModule> configuration)
        : base(serviceScopeFactory, logger, TimeSpan.FromSeconds(1))
    {
        _logger = logger;
        _configuration = configuration;

        Interval = TimeSpan.FromSeconds(_configuration.RecoveryThresholdSeconds);
    }

    /// <summary>
    /// Background job that reclaims inbox messages that have been stuck in processing for longer than the recovery threshold.
    /// </summary>
    protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<TContext>();

        var now = DateTime.UtcNow;

        var partitions = _configuration.GetPartitions();

        var messages = await db.InboxMessages
            .Where(x =>
                partitions.Contains(x.Partition) &&
                x.ProcessingAt < now.AddSeconds(-_configuration.RecoveryThresholdSeconds))
            .OrderBy(x => x.OccurredAt)
            .ToArrayAsync();

        if (messages.Length == 0)
            return;

        foreach (var msg in messages)
        {
            _logger.LogInformation("Recovering inbox message with id {MessageId} that was being processed since {ProcessingAt}", msg.Id, msg.ProcessingAt);

            msg.ProcessingAt = null;
            msg.NextRetryAt = now.AddMilliseconds(_configuration.IntervalMilliseconds);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
