using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.Modules.Produtos.Application.UseCases.CriarProduto;
using ModularAPITemplate.Modules.Produtos.Infrastructure.Persistence;
using NSubstitute;

namespace ModularAPITemplate.Modules.Produtos.Tests.Application.UseCases;

public class CriarProdutoHandlerTests : IDisposable
{
    private readonly ProdutosDbContext _context;
    private readonly ILogger<CriarProdutoHandler> _logger = Substitute.For<ILogger<CriarProdutoHandler>>();

    public CriarProdutoHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ProdutosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProdutosDbContext(options, Substitute.For<IPublisher>());
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var command = new CriarProdutoCommand("Teclado", "Teclado mecânico", 350m, 20);
        var handler = new CriarProdutoHandler(_context, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Teclado", result.Value!.Nome);
        Assert.Equal(350m, result.Value.Preco);

        var produtoNoBanco = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == result.Value.Id);
        Assert.NotNull(produtoNoBanco);
    }

    public void Dispose() => _context.Dispose();
}
