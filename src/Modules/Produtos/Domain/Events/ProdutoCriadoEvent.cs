using ModularAPITemplate.SharedKernel.Domain;

namespace ModularAPITemplate.Modules.Produtos.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um novo produto é criado.
/// </summary>
public sealed record ProdutoCriadoEvent(Guid ProdutoId, string Nome) : DomainEvent;
