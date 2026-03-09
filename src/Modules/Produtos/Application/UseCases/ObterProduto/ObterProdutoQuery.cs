using MediatR;
using ModularAPITemplate.Modules.Produtos.Application.DTOs;
using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.Modules.Produtos.Application.UseCases.ObterProduto;

public sealed record ObterProdutoQuery(Guid Id) : IRequest<Result<ProdutoResponse>>;
