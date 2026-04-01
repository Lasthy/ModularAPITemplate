using Cysharp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Domain;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Base DbContext that automatically dispatches domain events when saved.
/// All modules should derive from this DbContext.
/// </summary>
public interface IBaseDbContext
{
    /// <summary>
    /// Outbox messages used to reliably publish integration events during persistence.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    /// <summary>
    /// Inbox messages used to reliably consume integration events during persistence.
    /// </summary>
    public DbSet<InboxMessage> InboxMessages { get; set; }

    public DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
