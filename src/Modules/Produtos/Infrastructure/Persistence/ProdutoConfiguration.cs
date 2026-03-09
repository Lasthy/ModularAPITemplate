using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularAPITemplate.Modules.Produtos.Domain.Entities;

namespace ModularAPITemplate.Modules.Produtos.Infrastructure.Persistence;

/// <summary>
/// Configuração EF Core para a entidade Produto.
/// </summary>
public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Descricao)
            .HasMaxLength(1000);

        builder.Property(p => p.Preco)
            .HasPrecision(18, 2);

        builder.Property(p => p.QuantidadeEmEstoque);

        builder.Property(p => p.Ativo)
            .HasDefaultValue(true);

        builder.Property(p => p.CriadoEm);

        builder.Property(p => p.AtualizadoEm);

        // Ignora eventos de domínio na persistência
        builder.Ignore(p => p.DomainEvents);
    }
}
