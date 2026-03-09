namespace ModularAPITemplate.SharedKernel.Domain;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Fornece identidade e comparação por Id.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Eventos de domínio pendentes para despacho.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
        => left is not null ? left.Equals(right) : right is null;

    public static bool operator !=(Entity? left, Entity? right)
        => !(left == right);
}
