using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.Modules.NomeModulo.Infrastructure.Persistence;

/// <summary>
/// DbContext do módulo NomeModulo.
/// Cada módulo possui seu próprio contexto isolado.
/// </summary>
public sealed class NomeModuloDbContext(
    DbContextOptions<NomeModuloDbContext> options) : DbContext(options), IBaseDbContext
{
    // TODO: Adicionar DbSets
    // public DbSet<MinhaEntidade> MinhasEntidades => Set<MinhaEntidade>();
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplySoftDeleteQueryFilter();
        modelBuilder.ConfigureOutboxMessage();
        modelBuilder.HasDefaultSchema("nomemodulo_schema");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NomeModuloDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.ConfigureSharedKernelConventions();
    }
}
