using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.Modules.NomeModulo.Infrastructure.Persistence;

/// <summary>
/// DbContext do módulo NomeModulo.
/// Cada módulo possui seu próprio contexto isolado.
/// </summary>
public sealed class NomeModuloDbContext(
    DbContextOptions<NomeModuloDbContext> options,
    IPublisher publisher) : BaseDbContext(options, publisher)
{
    // TODO: Adicionar DbSets
    // public DbSet<MinhaEntidade> MinhasEntidades => Set<MinhaEntidade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("nomemodulo_schema");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NomeModuloDbContext).Assembly);
    }
}
