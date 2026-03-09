using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.Modules.Produtos.Application.DTOs;
using ModularAPITemplate.Modules.Produtos.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.Modules.Produtos.Application.UseCases.ListarProdutos;

/// <summary>
/// Caso de uso: listar produtos com paginação.
/// </summary>
public sealed class ListarProdutosHandler(
    ProdutosDbContext context,
    ILogger<ListarProdutosHandler> logger) : IRequestHandler<ListarProdutosQuery, Result<PagedResult<ProdutoResponse>>>
{
    public async Task<Result<PagedResult<ProdutoResponse>>> Handle(ListarProdutosQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Listing products - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        var totalCount = await context.Produtos.CountAsync(cancellationToken);

        var produtos = await context.Produtos
            .OrderByDescending(p => p.CriadoEm)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => p.ToResponse())
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<ProdutoResponse>(produtos, totalCount, request.Page, request.PageSize);

        return Result.Success(pagedResult);
    }
}
