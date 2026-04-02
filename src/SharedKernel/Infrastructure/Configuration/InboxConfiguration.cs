using Microsoft.Extensions.Configuration;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Configuration;

/// <summary>
/// Reads inbox-related configuration values for a given module.
/// </summary>
/// <typeparam name="TModule">Module type.</typeparam>
public class InboxConfiguration<TModule> where TModule : IModule
{
    private readonly IConfiguration _configuration;

    public InboxConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Whether inbox processing is enabled for the module.
    /// </summary>
    public bool Enabled => _configuration.GetValue<bool>($"Modules:{TModule.ModuleName}:Inbox:Enabled", true);

    /// <summary>
    /// Interval between inbox worker executions in milliseconds.
    /// </summary>
    public int IntervalMilliseconds => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Inbox:IntervalMilliseconds", 1000);

    /// <summary>
    /// Number of messages to claim per inbox processing batch.
    /// </summary>
    public int BatchSize => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Inbox:BatchSize", 50);

    /// <summary>
    /// Total number of partitions used for inbox sharding.
    /// </summary>
    public int PartitionCount => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Inbox:PartitionCount", 64);

    /// <summary>
    /// The starting partition index (inclusive).
    /// </summary>
    public int PartitionStart => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Inbox:PartitionStart", 0);

    /// <summary>
    /// The ending partition index (inclusive).
    /// </summary>
    public int PartitionEnd => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Inbox:PartitionEnd", 63);

    /// <summary>
    /// Maximum number of retry attempts for processing a batch of messages before giving up.
    /// </summary>
    public int MaxRetryAttempts => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Inbox:MaxRetryAttempts", 3);

    /// <summary>
    /// Number of seconds after which a claimed message is considered stuck and eligible for recovery.
    /// </summary>
    public int RecoveryThresholdSeconds => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Inbox:RecoveryThresholdSeconds", 60);

    /// <summary>
    /// Number of days to keep processed inbox items before cleanup.
    /// </summary>
    public int CleanupThresholdDays => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Inbox:CleanupThresholdDays", 7);

    /// <summary>
    /// Returns all supported partitions for the configured range.
    /// </summary>
    public int[] GetPartitions()
    {
        return Enumerable.Range(PartitionStart, PartitionEnd - PartitionStart + 1).ToArray();
    }
}
