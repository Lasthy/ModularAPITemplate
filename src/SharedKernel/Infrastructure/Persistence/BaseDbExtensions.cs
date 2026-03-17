using System.Linq.Expressions;
using Cysharp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularAPITemplate.SharedKernel.Domain.Components;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Extensions for configuring EF Core conventions and shared kernel entities.
/// </summary>
public static class BaseDbExtensions
{
    /// <summary>
    /// Configures shared kernel conventions (e.g. Ulid conversion) for the EF Core model builder.
    /// </summary>
    public static void ConfigureSharedKernelConventions(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToBytesConverter>();
    }

    /// <summary>
    /// Configures the OutboxMessage entity (indexes, concurrency token, etc.).
    /// </summary>
    public static void ConfigureOutboxMessage(this ModelBuilder builder)
    {
        builder.Entity<OutboxMessage>(entity =>
        {
            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.HasIndex(x => new
            {
                x.Partition,
                x.ProcessedAt,
                x.ProcessingAt,
                x.NextRetryAt,
                x.OccurredAt
            })
            .HasDatabaseName("IX_Outbox_Partition_Dispatch");
        });
    }

    /// <summary>
    /// Applies a global query filter to exclude soft-deleted entities.
    /// </summary>
    public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "entity");

            var propertyMethod = typeof(EF)
                .GetMethod(nameof(EF.Property))!
                .MakeGenericMethod(typeof(DateTime?));

            var deletedAtProperty = Expression.Call(
                propertyMethod,
                parameter,
                Expression.Constant(nameof(ISoftDeletable.DeletedAt)));

            var condition = Expression.Equal(
                deletedAtProperty,
                Expression.Constant(null));

            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder
                .Entity(entityType.ClrType)
                .HasQueryFilter(lambda);
        }
    }
}