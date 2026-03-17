
namespace ModularAPITemplate.SharedKernel.Domain.Components;

/// <summary>
/// Marks an entity as auditable with creation and update tracking.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    UserIdType? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    UserIdType? UpdatedBy { get; set; }
}