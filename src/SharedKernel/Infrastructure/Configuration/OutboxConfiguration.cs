using Microsoft.Extensions.Configuration;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Configuration;

public class OutboxConfiguration
{
    private readonly IConfiguration _configuration;

    public OutboxConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool Enabled => _configuration.GetValue<bool>("Outbox:Enabled", true);
    public int IntervalMilliseconds => _configuration.GetValue<int>("Outbox:IntervalMilliseconds", 1000);
    public int BatchSize => _configuration.GetValue<int>("Outbox:BatchSize", 50);
    public int PartitionCount => _configuration.GetValue<int>("Outbox:PartitionCount", 64);
    public int PartitionStart => _configuration.GetValue<int>("Outbox:PartitionStart", 0);
    public int PartitionEnd => _configuration.GetValue<int>("Outbox:PartitionEnd", 63);
}