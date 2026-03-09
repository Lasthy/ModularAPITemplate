using ModularAPITemplate.Modules.Produtos.Domain.Entities;

namespace ModularAPITemplate.Modules.Produtos.Tests.Domain;

public class ProdutoTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarProduto()
    {
        // Arrange & Act
        var produto = Produto.Criar("Notebook", "Notebook Dell 15\"", 3500.00m, 10);

        // Assert
        Assert.NotEqual(Guid.Empty, produto.Id);
        Assert.Equal("Notebook", produto.Nome);
        Assert.Equal("Notebook Dell 15\"", produto.Descricao);
        Assert.Equal(3500.00m, produto.Preco);
        Assert.Equal(10, produto.QuantidadeEmEstoque);
        Assert.True(produto.Ativo);
        Assert.True(produto.DomainEvents.Count > 0);
    }

    [Fact]
    public void Criar_ComNomeVazio_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Produto.Criar("", "Descrição", 100m, 5));
    }

    [Fact]
    public void Criar_ComPrecoNegativo_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Produto.Criar("Produto", "Descrição", -1m, 5));
    }

    [Fact]
    public void Criar_ComEstoqueNegativo_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Produto.Criar("Produto", "Descrição", 100m, -1));
    }

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarProduto()
    {
        // Arrange
        var produto = Produto.Criar("Notebook", "Dell", 3500m, 10);

        // Act
        produto.Atualizar("Notebook Atualizado", "Dell Inspiron", 4000m, 15);

        // Assert
        Assert.Equal("Notebook Atualizado", produto.Nome);
        Assert.Equal("Dell Inspiron", produto.Descricao);
        Assert.Equal(4000m, produto.Preco);
        Assert.Equal(15, produto.QuantidadeEmEstoque);
        Assert.NotNull(produto.AtualizadoEm);
    }

    [Fact]
    public void Desativar_DeveDesativarProduto()
    {
        // Arrange
        var produto = Produto.Criar("Notebook", "Dell", 3500m, 10);

        // Act
        produto.Desativar();

        // Assert
        Assert.False(produto.Ativo);
        Assert.NotNull(produto.AtualizadoEm);
    }

    [Fact]
    public void Ativar_DeveAtivarProduto()
    {
        // Arrange
        var produto = Produto.Criar("Notebook", "Dell", 3500m, 10);
        produto.Desativar();

        // Act
        produto.Ativar();

        // Assert
        Assert.True(produto.Ativo);
    }
}
