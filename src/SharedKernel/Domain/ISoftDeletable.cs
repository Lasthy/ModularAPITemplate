namespace ModularAPITemplate.SharedKernel.Domain.Components;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
    UserIdType? DeletedBy { get; set; }
}