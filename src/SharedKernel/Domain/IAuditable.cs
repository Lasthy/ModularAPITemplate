
namespace ModularAPITemplate.SharedKernel.Domain.Components;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    UserIdType? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    UserIdType? UpdatedBy { get; set; }
}