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
/// Cleans up old processed outbox messages from the database.
/// </summary>
public class OutboxCleanupWorker<TModule, TContext> : BaseWorker
    where TContext : IBaseDbContext
    where TModule : IModule
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxCleanupWorker<TModule, TContext>> _logger;
    private readonly OutboxConfiguration<TModule> _configuration;

    public OutboxCleanupWorker(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxCleanupWorker<TModule, TContext>> logger, OutboxConfiguration<TModule> configuration)
        : base(serviceScopeFactory, logger, TimeSpan.FromSeconds(1))
    {
        _logger = logger;
        _scopeFactory = serviceScopeFactory;
        _configuration = configuration;

        Interval = TimeSpan.FromSeconds(_configuration.RecoveryThresholdSeconds);
    }

    /// <summary>
    /// Background job that removes processed outbox messages older than the configured cleanup threshold.
    /// </summary>
    protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<TContext>();

        var now = DateTime.UtcNow;

        var partitions = _configuration.GetPartitions();

        var messages = await db.OutboxMessages
                    .Where(x =>
                        partitions.Contains(x.Partition) &&
                        x.ProcessedAt != null &&
                        x.ProcessedAt < now.AddDays(-_configuration.CleanupThresholdDays))
                    .OrderBy(x => x.OccurredAt)
                    .ToArrayAsync();
        
        if(messages.Length == 0)
            return;

        db.OutboxMessages.RemoveRange(messages);

        await db.SaveChangesAsync();
    }
}