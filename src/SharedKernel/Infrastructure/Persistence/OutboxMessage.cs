using Cysharp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

public class OutboxMessage
{
    public Ulid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ProcessingAt { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public int Partition { get; set; }
    public string? Error { get; set; }
    public UserIdType? ActorId { get; set; }
    public byte[] RowVersion { get; set; } = default!;
}