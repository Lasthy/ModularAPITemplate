using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModularAPITemplate.SharedKernel.Application.Context;
using ModularAPITemplate.SharedKernel.Domain;
using ModularAPITemplate.SharedKernel.Domain.Components;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IRequestContext _userContext;

    public AuditSaveChangesInterceptor(IRequestContext userContext)
    {
        _userContext = userContext;
    }

    private void ApplyAudit(DbContext context)
    {
        var now = DateTime.UtcNow;
        var user = _userContext.UserId;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                
                entry.Entity.DeletedAt = now;
                entry.Entity.DeletedBy = user;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = user;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = user;

                entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
            }
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
            ApplyAudit(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
            ApplyAudit(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}