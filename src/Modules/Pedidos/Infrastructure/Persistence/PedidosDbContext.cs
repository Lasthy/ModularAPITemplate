using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.Modules.Pedidos.Infrastructure.Persistence;

/// <summary>
/// DbContext do módulo Pedidos.
/// Cada módulo possui seu próprio contexto isolado.
/// </summary>
public sealed class PedidosDbContext(
    DbContextOptions<PedidosDbContext> options,
    IPublisher publisher) : BaseDbContext(options, publisher)
{
    // TODO: Adicionar DbSets
    // public DbSet<MinhaEntidade> MinhasEntidades => Set<MinhaEntidade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("pedidos");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidosDbContext).Assembly);
    }
}
