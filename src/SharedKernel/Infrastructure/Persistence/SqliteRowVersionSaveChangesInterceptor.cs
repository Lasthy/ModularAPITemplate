using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// EF Core interceptor that assigns application-managed concurrency tokens for SQLite.
/// </summary>
public class SqliteRowVersionSaveChangesInterceptor : SaveChangesInterceptor
{
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

    private static void ApplySqliteRowVersion(DbContext context)
    {
        if (!string.Equals(context.Database.ProviderName, SqliteProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<OutboxMessage>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.RowVersion = CreateRowVersionToken();
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<InboxMessage>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.RowVersion = CreateRowVersionToken();
            }
        }
    }

    private static byte[] CreateRowVersionToken()
    {
        var bytes = new byte[8];
        Random.Shared.NextBytes(bytes);
        return bytes;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            ApplySqliteRowVersion(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            ApplySqliteRowVersion(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
