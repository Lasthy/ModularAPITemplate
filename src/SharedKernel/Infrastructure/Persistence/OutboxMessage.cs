using Cysharp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Represents an outbox message persisted to the database.
/// </summary>
public class OutboxMessage
{
    public Ulid Id { get; set; }

    /// <summary>
    /// CLR type full name of the event.
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Serialized event payload.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// When the message was successfully processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// When the message was claimed for processing.
    /// </summary>
    public DateTime? ProcessingAt { get; set; }

    /// <summary>
    /// Number of times processing has been attempted.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// When the message should be retried.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Partition number for sharding concurrent processing.
    /// </summary>
    public int Partition { get; set; }

    public string? Error { get; set; }

    public UserIdType? ActorId { get; set; }

    public byte[] RowVersion { get; set; } = default!;
}