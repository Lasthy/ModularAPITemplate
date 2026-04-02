using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Health;

/// <summary>
/// Generic health check for a module that validates DB connectivity and inbox/outbox backlog pressure.
/// </summary>
/// <typeparam name="TContext">Module DbContext type.</typeparam>
/// <typeparam name="TModule">Module marker type.</typeparam>
public sealed class ModuleHealthCheck<TContext, TModule> : IHealthCheck
    where TContext : DbContext, IBaseDbContext
    where TModule : IModule
{
    private readonly TContext _dbContext;
    private readonly InboxConfiguration<TModule> _inboxConfiguration;
    private readonly OutboxConfiguration<TModule> _outboxConfiguration;
    private readonly ILogger<ModuleHealthCheck<TContext, TModule>> _logger;

    public ModuleHealthCheck(
        TContext dbContext,
        InboxConfiguration<TModule> inboxConfiguration,
        OutboxConfiguration<TModule> outboxConfiguration,
        ILogger<ModuleHealthCheck<TContext, TModule>> logger)
    {
        _dbContext = dbContext;
        _inboxConfiguration = inboxConfiguration;
        _outboxConfiguration = outboxConfiguration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                var data = CreateDataDictionary(inboxBacklog: null, outboxBacklog: null, databaseReachable: false);
                return HealthCheckResult.Unhealthy($"Module '{TModule.ModuleName}' database is unreachable.", data: data);
            }

            var inboxPartitions = _inboxConfiguration.GetPartitions();
            var outboxPartitions = _outboxConfiguration.GetPartitions();

            var inboxBacklog = await _dbContext.InboxMessages
                .AsNoTracking()
                .Where(message => message.ProcessedAt == null
                                  && message.ProcessingAt == null
                                  && inboxPartitions.Contains(message.Partition))
                .CountAsync(cancellationToken);

            var outboxBacklog = await _dbContext.OutboxMessages
                .AsNoTracking()
                .Where(message => message.ProcessedAt == null
                                  && message.ProcessingAt == null
                                  && outboxPartitions.Contains(message.Partition))
                .CountAsync(cancellationToken);

            var inboxThresholdExceeded = _inboxConfiguration.BacklogWarningThreshold > 0
                                         && inboxBacklog >= _inboxConfiguration.BacklogWarningThreshold;

            var outboxThresholdExceeded = _outboxConfiguration.BacklogWarningThreshold > 0
                                          && outboxBacklog >= _outboxConfiguration.BacklogWarningThreshold;

            var dataPayload = CreateDataDictionary(inboxBacklog, outboxBacklog, databaseReachable: true);

            if (inboxThresholdExceeded || outboxThresholdExceeded)
            {
                _logger.LogWarning(
                    "Module health degraded for {ModuleName}. Inbox backlog: {InboxBacklog}, Outbox backlog: {OutboxBacklog}",
                    TModule.ModuleName,
                    inboxBacklog,
                    outboxBacklog);

                return HealthCheckResult.Degraded(
                    $"Module '{TModule.ModuleName}' backlog exceeded configured threshold.",
                    data: dataPayload);
            }

            return HealthCheckResult.Healthy($"Module '{TModule.ModuleName}' is healthy.", data: dataPayload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while executing module health check for {ModuleName}", TModule.ModuleName);

            var data = CreateDataDictionary(inboxBacklog: null, outboxBacklog: null, databaseReachable: false);
            return HealthCheckResult.Unhealthy($"Module '{TModule.ModuleName}' health check failed.", ex, data);
        }
    }

    private Dictionary<string, object> CreateDataDictionary(int? inboxBacklog, int? outboxBacklog, bool databaseReachable)
    {
        return new Dictionary<string, object>
        {
            ["module"] = TModule.ModuleName,
            ["dbContext"] = typeof(TContext).Name,
            ["databaseReachable"] = databaseReachable,
            ["inboxBacklog"] = inboxBacklog ?? -1,
            ["outboxBacklog"] = outboxBacklog ?? -1,
            ["inboxBacklogWarningThreshold"] = _inboxConfiguration.BacklogWarningThreshold,
            ["outboxBacklogWarningThreshold"] = _outboxConfiguration.BacklogWarningThreshold,
            ["checkedAtUtc"] = DateTime.UtcNow,
        };
    }
}