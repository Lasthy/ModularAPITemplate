using MediatR;
using ModularAPITemplate.Modules.Produtos.Application.DTOs;
using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.Modules.Produtos.Application.UseCases.CriarProduto;

public sealed record CriarProdutoCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    int QuantidadeEmEstoque) : IRequest<Result<ProdutoResponse>>;
