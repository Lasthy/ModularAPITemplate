using MediatR;
using ModularAPITemplate.Modules.Produtos.Application.DTOs;
using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.Modules.Produtos.Application.UseCases.ListarProdutos;

public sealed record ListarProdutosQuery(int Page = 1, int PageSize = 10) : IRequest<Result<PagedResult<ProdutoResponse>>>;
