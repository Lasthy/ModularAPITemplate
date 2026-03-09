using ModularAPITemplate.Modules.Produtos.Domain.Entities;

namespace ModularAPITemplate.Modules.Produtos.Application.DTOs;

/// <summary>
/// Métodos de mapeamento entre entidades e DTOs do módulo Produtos.
/// </summary>
public static class ProdutoMappings
{
    public static ProdutoResponse ToResponse(this Produto produto) =>
        new(
            produto.Id,
            produto.Nome,
            produto.Descricao,
            produto.Preco,
            produto.QuantidadeEmEstoque,
            produto.Ativo,
            produto.CriadoEm,
            produto.AtualizadoEm);
}
