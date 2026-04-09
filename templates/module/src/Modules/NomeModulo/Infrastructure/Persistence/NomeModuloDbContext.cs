using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.Modules.NomeModulo.Infrastructure.Persistence;

/// <summary>
/// DbContext for the NomeModulo module.
/// Each module owns its own isolated database context.
/// </summary>
public sealed class NomeModuloDbContext(
    DbContextOptions<NomeModuloDbContext> options) : DbContext(options), IBaseDbContext
{
    // TODO: Add DbSets for this module's entities.
    // public DbSet<MyEntity> MyEntities => Set<MyEntity>();

    /// <inheritdoc/>
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    /// <inheritdoc/>
    public DbSet<InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var isSqlite = string.Equals(
            Database.ProviderName,
            "Microsoft.EntityFrameworkCore.Sqlite",
            StringComparison.OrdinalIgnoreCase);

        // Apply shared kernel behavior (soft-delete filtering, outbox mapping, etc.)
        modelBuilder.ApplySoftDeleteQueryFilter();
        modelBuilder.ConfigureOutboxMessage(isSqlite);
        modelBuilder.ConfigureInboxMessage(isSqlite);

        // Ensure module schema is isolated.
        if (!isSqlite)
        {
            modelBuilder.HasDefaultSchema("nomemodulo_schema");
        }

        // Register entity configurations from this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NomeModuloDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.ConfigureSharedKernelConventions();
    }
}
