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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply shared kernel behavior (soft-delete filtering, outbox mapping, etc.)
        modelBuilder.ApplySoftDeleteQueryFilter();
        modelBuilder.ConfigureOutboxMessage();

        // Ensure module schema is isolated.
        modelBuilder.HasDefaultSchema("nomemodulo_schema");

        // Register entity configurations from this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NomeModuloDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.ConfigureSharedKernelConventions();
    }
}
