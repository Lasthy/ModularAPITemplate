using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Workers;

public class OutboxRecoveryWorker<TModule, TContext> : BaseWorker
    where TContext : IBaseDbContext
    where TModule : IModule
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxRecoveryWorker<TModule, TContext>> _logger;
    private readonly OutboxConfiguration<TModule> _configuration;

    public OutboxRecoveryWorker(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxRecoveryWorker<TModule, TContext>> logger, OutboxConfiguration<TModule> configuration)
        : base(serviceScopeFactory, logger, TimeSpan.FromSeconds(1))
    {
        _logger = logger;
        _scopeFactory = serviceScopeFactory;
        _configuration = configuration;

        Interval = TimeSpan.FromSeconds(_configuration.RecoveryThresholdSeconds);
    }

    protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<TContext>();

        var now = DateTime.UtcNow;

        var partitions = _configuration.GetPartitions();

        var messages = await db.OutboxMessages
                    .Where(x =>
                        partitions.Contains(x.Partition) &&
                        x.ProcessingAt < now.AddSeconds(-_configuration.RecoveryThresholdSeconds))
                    .OrderBy(x => x.OccurredAt)
                    .ToArrayAsync();
        
        if(messages.Length == 0)
            return;
        
        foreach (var msg in messages)
        {
            _logger.LogInformation("Recovering outbox message with id {MessageId} that was being processed since {ProcessingAt}", msg.Id, msg.ProcessingAt);

            msg.ProcessingAt = null;
            msg.NextRetryAt = now.AddMilliseconds(_configuration.IntervalMilliseconds);
        }

        await db.SaveChangesAsync();
    }
}