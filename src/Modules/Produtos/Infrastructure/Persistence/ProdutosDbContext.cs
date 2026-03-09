using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.Modules.Produtos.Domain.Entities;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.Modules.Produtos.Infrastructure.Persistence;

/// <summary>
/// DbContext do módulo de Produtos.
/// Cada módulo possui seu próprio contexto isolado.
/// </summary>
public sealed class ProdutosDbContext(
    DbContextOptions<ProdutosDbContext> options,
    IPublisher publisher) : BaseDbContext(options, publisher)
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("produtos");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdutosDbContext).Assembly);
    }
}
