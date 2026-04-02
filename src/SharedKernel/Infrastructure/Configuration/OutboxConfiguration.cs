using Microsoft.Extensions.Configuration;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Configuration;

/// <summary>
/// Reads outbox-related configuration values for a given module.
/// </summary>
/// <typeparam name="TModule">Module type.</typeparam>
public class OutboxConfiguration<TModule> where TModule : IModule
{
    private readonly IConfiguration _configuration;

    public OutboxConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Whether outbox processing is enabled for the module.
    /// </summary>
    public bool Enabled => _configuration.GetValue<bool>($"Modules:{TModule.ModuleName}:Outbox:Enabled", true);

    /// <summary>
    /// Interval between outbox worker executions in milliseconds.
    /// </summary>
    public int IntervalMilliseconds => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:IntervalMilliseconds", 1000);

    /// <summary>
    /// Number of messages to claim per outbox processing batch.
    /// </summary>
    public int BatchSize => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:BatchSize", 50);

    /// <summary>
    /// Total number of partitions used for outbox sharding.
    /// </summary>
    public int PartitionCount => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:PartitionCount", 64);

    /// <summary>
    /// The starting partition index (inclusive).
    /// </summary>
    public int PartitionStart => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:PartitionStart", 0);

    /// <summary>
    /// The ending partition index (inclusive).
    /// </summary>
    public int PartitionEnd => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:PartitionEnd", 63);

    /// <summary>
    /// Number of seconds after which a claimed message is considered stuck and eligible for recovery.
    /// </summary>
    public int RecoveryThresholdSeconds => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:RecoveryThresholdSeconds", 60);

    /// <summary>
    /// Number of days to keep processed outbox items before cleanup.
    /// </summary>
    public int CleanupThresholdDays => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:CleanupThresholdDays", 7);

    /// <summary>
    /// Maximum number of retry attempts for processing a batch of messages before giving up.
    /// </summary>
    public int MaxRetryAttempts => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:MaxRetryAttempts", 3);

    /// <summary>
    /// Pending outbox message count that triggers a warning log when reached or exceeded.
    /// Set to 0 or less to disable backlog warnings.
    /// </summary>
    public int BacklogWarningThreshold => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:BacklogWarningThreshold", 1000);

    /// <summary>
    /// Minimum number of seconds between backlog warning logs while the backlog remains above the threshold.
    /// Set to 0 or less to log every cycle when above threshold.
    /// </summary>
    public int BacklogWarningCooldownSeconds => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:BacklogWarningCooldownSeconds", 300);

    /// <summary>
    /// Returns all supported partitions for the configured range.
    /// </summary>
    public int[] GetPartitions()
    {
        return Enumerable.Range(PartitionStart, PartitionEnd - PartitionStart + 1).ToArray();
    }
}