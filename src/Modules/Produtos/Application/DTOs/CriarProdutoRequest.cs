using System.ComponentModel.DataAnnotations;

namespace ModularAPITemplate.Modules.Produtos.Application.DTOs;

/// <summary>
/// DTO de requisição para criação de um novo produto.
/// </summary>
public sealed record CriarProdutoRequest(
    [Required] string Nome,
    string Descricao,
    [Range(0, double.MaxValue)] decimal Preco,
    [Range(0, int.MaxValue)] int QuantidadeEmEstoque);
