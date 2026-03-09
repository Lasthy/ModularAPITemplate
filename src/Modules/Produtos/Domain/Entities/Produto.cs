using ModularAPITemplate.SharedKernel.Domain;

namespace ModularAPITemplate.Modules.Produtos.Domain.Entities;

/// <summary>
/// Entidade Produto — raiz de agregado do módulo de Produtos.
/// </summary>
public sealed class Produto : AggregateRoot
{
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int QuantidadeEmEstoque { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    private Produto() { } // EF Core

    public static Produto Criar(string nome, string descricao, decimal preco, int quantidadeEmEstoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do produto é obrigatório.", nameof(nome));
        if (preco < 0)
            throw new ArgumentException("Preço não pode ser negativo.", nameof(preco));
        if (quantidadeEmEstoque < 0)
            throw new ArgumentException("Quantidade em estoque não pode ser negativa.", nameof(quantidadeEmEstoque));

        var produto = new Produto
        {
            Nome = nome,
            Descricao = descricao,
            Preco = preco,
            QuantidadeEmEstoque = quantidadeEmEstoque,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        produto.RaiseDomainEvent(new Events.ProdutoCriadoEvent(produto.Id, produto.Nome));

        return produto;
    }

    public void Atualizar(string nome, string descricao, decimal preco, int quantidadeEmEstoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do produto é obrigatório.", nameof(nome));
        if (preco < 0)
            throw new ArgumentException("Preço não pode ser negativo.", nameof(preco));

        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        QuantidadeEmEstoque = quantidadeEmEstoque;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }
}
