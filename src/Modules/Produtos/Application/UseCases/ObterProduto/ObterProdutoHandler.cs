using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.Modules.Produtos.Application.DTOs;
using ModularAPITemplate.Modules.Produtos.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.Modules.Produtos.Application.UseCases.ObterProduto;

/// <summary>
/// Caso de uso: obter um produto por Id.
/// </summary>
public sealed class ObterProdutoHandler(
    ProdutosDbContext context,
    ILogger<ObterProdutoHandler> logger) : IRequestHandler<ObterProdutoQuery, Result<ProdutoResponse>>
{
    public async Task<Result<ProdutoResponse>> Handle(ObterProdutoQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product: {ProdutoId}", request.Id);

        var produto = await context.Produtos
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (produto is null)
        {
            logger.LogWarning("Product not found: {ProdutoId}", request.Id);
            return Result.Failure<ProdutoResponse>("Produto não encontrado.");
        }

        return Result.Success(produto.ToResponse());
    }
}
