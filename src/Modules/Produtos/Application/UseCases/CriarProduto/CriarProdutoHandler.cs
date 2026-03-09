using MediatR;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.Modules.Produtos.Application.DTOs;
using ModularAPITemplate.Modules.Produtos.Domain.Entities;
using ModularAPITemplate.Modules.Produtos.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.Modules.Produtos.Application.UseCases.CriarProduto;

/// <summary>
/// Caso de uso: criação de um novo produto.
/// </summary>
public sealed class CriarProdutoHandler(
    ProdutosDbContext context,
    ILogger<CriarProdutoHandler> logger) : IRequestHandler<CriarProdutoCommand, Result<ProdutoResponse>>
{
    public async Task<Result<ProdutoResponse>> Handle(CriarProdutoCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product: {Nome}", request.Nome);

        var produto = Produto.Criar(
            request.Nome,
            request.Descricao,
            request.Preco,
            request.QuantidadeEmEstoque);

        context.Produtos.Add(produto);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product created successfully: {ProdutoId}", produto.Id);

        return Result.Success(produto.ToResponse());
    }
}
