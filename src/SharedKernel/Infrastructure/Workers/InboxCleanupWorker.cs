using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Workers;

/// <summary>
/// Cleans up old processed inbox messages from the database.
/// </summary>
public class InboxCleanupWorker<TModule, TContext> : BaseWorker
    where TContext : IBaseDbContext
    where TModule : IModule
{
    private readonly InboxConfiguration<TModule> _configuration;

    public InboxCleanupWorker(IServiceScopeFactory serviceScopeFactory, ILogger<InboxCleanupWorker<TModule, TContext>> logger, InboxConfiguration<TModule> configuration)
        : base(serviceScopeFactory, logger, TimeSpan.FromSeconds(1))
    {
        _configuration = configuration;

        Interval = TimeSpan.FromDays(_configuration.CleanupThresholdDays);
    }

    /// <summary>
    /// Background job that removes processed inbox messages older than the configured cleanup threshold.
    /// </summary>
    protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<TContext>();

        var now = DateTime.UtcNow;

        var partitions = _configuration.GetPartitions();

        var messages = await db.InboxMessages
            .Where(x =>
                partitions.Contains(x.Partition) &&
                x.ProcessedAt != null &&
                x.ProcessedAt < now.AddDays(-_configuration.CleanupThresholdDays))
            .OrderBy(x => x.OccurredAt)
            .ToArrayAsync();

        if (messages.Length == 0)
            return;

        db.InboxMessages.RemoveRange(messages);

        await db.SaveChangesAsync(cancellationToken);
    }
}
