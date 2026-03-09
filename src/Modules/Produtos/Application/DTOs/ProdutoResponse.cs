namespace ModularAPITemplate.Modules.Produtos.Application.DTOs;

/// <summary>
/// DTO de resposta para representação de um produto.
/// </summary>
public sealed record ProdutoResponse(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int QuantidadeEmEstoque,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);
