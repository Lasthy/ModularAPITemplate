using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.Modules.Produtos.Application.UseCases.ObterProduto;
using ModularAPITemplate.Modules.Produtos.Domain.Entities;
using ModularAPITemplate.Modules.Produtos.Infrastructure.Persistence;
using NSubstitute;

namespace ModularAPITemplate.Modules.Produtos.Tests.Application.UseCases;

public class ObterProdutoHandlerTests : IDisposable
{
    private readonly ProdutosDbContext _context;
    private readonly ILogger<ObterProdutoHandler> _logger = Substitute.For<ILogger<ObterProdutoHandler>>();

    public ObterProdutoHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ProdutosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProdutosDbContext(options, Substitute.For<IPublisher>());
    }

    [Fact]
    public async Task Handle_ProdutoExistente_DeveRetornarSucesso()
    {
        // Arrange
        var produto = Produto.Criar("Monitor", "Monitor 27\"", 2000m, 5);
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        var handler = new ObterProdutoHandler(_context, _logger);

        // Act
        var result = await handler.Handle(new ObterProdutoQuery(produto.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Monitor", result.Value!.Nome);
    }

    [Fact]
    public async Task Handle_ProdutoInexistente_DeveRetornarFalha()
    {
        // Arrange
        var handler = new ObterProdutoHandler(_context, _logger);

        // Act
        var result = await handler.Handle(new ObterProdutoQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Produto não encontrado.", result.Error);
    }

    public void Dispose() => _context.Dispose();
}
