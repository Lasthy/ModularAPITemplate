namespace ModularAPITemplate.SharedKernel.Domain.Components;

/// <summary>
/// Marks an entity as soft-deletable (logical delete instead of physical delete).
/// </summary>
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
    UserIdType? DeletedBy { get; set; }
}