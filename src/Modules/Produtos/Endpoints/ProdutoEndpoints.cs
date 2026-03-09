using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ModularAPITemplate.Modules.Produtos.Application.DTOs;
using ModularAPITemplate.Modules.Produtos.Application.UseCases.CriarProduto;
using ModularAPITemplate.Modules.Produtos.Application.UseCases.ListarProdutos;
using ModularAPITemplate.Modules.Produtos.Application.UseCases.ObterProduto;
using ModularAPITemplate.SharedKernel.Application;

namespace ModularAPITemplate.Modules.Produtos.Endpoints;

/// <summary>
/// Endpoints do módulo de Produtos.
/// </summary>
public static class ProdutoEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/produtos")
            .WithGroupName(ProdutosModule.ModuleName)
            .WithTags("Produtos");

        group.MapGet("/", ListarProdutos)
            .WithName("ListarProdutos")
            .WithSummary("Lista todos os produtos com paginação")
            .WithDescription("Retorna uma lista paginada de produtos cadastrados.")
            .Produces<PagedResult<ProdutoResponse>>()
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", ObterProduto)
            .WithName("ObterProduto")
            .WithSummary("Obtém um produto pelo ID")
            .WithDescription("Retorna os dados de um produto específico.")
            .Produces<ProdutoResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/", CriarProduto)
            .WithName("CriarProduto")
            .WithSummary("Cria um novo produto")
            .WithDescription("Cadastra um novo produto no sistema.")
            .Produces<ProdutoResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> ListarProdutos(
        ISender sender,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListarProdutosQuery(page, pageSize), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error);
    }

    private static async Task<IResult> ObterProduto(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ObterProdutoQuery(id), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(new { error = result.Error });
    }

    private static async Task<IResult> CriarProduto(
        CriarProdutoRequest request,
        ISender sender,
        CancellationToken cancellationToken = default)
    {
        var command = new CriarProdutoCommand(
            request.Nome,
            request.Descricao,
            request.Preco,
            request.QuantidadeEmEstoque);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/produtos/{result.Value!.Id}", result.Value)
            : Results.BadRequest(new { error = result.Error });
    }
}
