using ModularAPITemplate.SharedKernel.Infrastructure.Events;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

public interface IIntegrationEventPublisher<TContext>
     where TContext : IBaseDbContext
{
    Task PublishAsync(IntegrationEvent message, CancellationToken ct = default);
}