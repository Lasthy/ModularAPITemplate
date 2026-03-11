using System.Linq.Expressions;
using Cysharp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Domain.Components;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

public static class BaseDbExtensions
{
    public static void ConfigureSharedKernelConventions(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToBytesConverter>();
    }

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