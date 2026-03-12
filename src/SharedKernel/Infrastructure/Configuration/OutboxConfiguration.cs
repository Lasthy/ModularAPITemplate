using Microsoft.Extensions.Configuration;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Configuration;

public class OutboxConfiguration<TModule> where TModule : IModule
{
    private readonly IConfiguration _configuration;

    public OutboxConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool Enabled => _configuration.GetValue<bool>($"Modules:{TModule.ModuleName}:Outbox:Enabled", true);
    public int IntervalMilliseconds => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:IntervalMilliseconds", 1000);
    public int BatchSize => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:BatchSize", 50);
    public int PartitionCount => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:PartitionCount", 64);
    public int PartitionStart => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:PartitionStart", 0);
    public int PartitionEnd => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:PartitionEnd", 63);
    public int RecoveryThresholdSeconds => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:RecoveryThresholdSeconds", 60);
    public int CleanupThresholdDays => _configuration.GetValue<int>($"Modules:{TModule.ModuleName}:Outbox:CleanupThresholdDays", 7);

    public int[] GetPartitions()
    {
        return Enumerable.Range(PartitionStart, PartitionEnd - PartitionStart + 1).ToArray();
    }
}