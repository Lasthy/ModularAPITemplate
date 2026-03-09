namespace ModularAPITemplate.SharedKernel.Domain;

/// <summary>
/// Marcador para raízes de agregado.
/// Agregados são o limite de consistência transacional.
/// </summary>
public abstract class AggregateRoot : Entity;
