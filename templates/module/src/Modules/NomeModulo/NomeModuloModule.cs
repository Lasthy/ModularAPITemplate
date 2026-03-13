using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularAPITemplate.Modules.NomeModulo.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Infrastructure.Workers;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.Modules.NomeModulo;

/// <summary>
/// Módulo NomeModulo — ponto de entrada para registro de serviços e endpoints.
/// </summary>
public sealed class NomeModuloModule : IModule
{
    public static string ModuleName => "NomeModulo";

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<NomeModuloDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration[$"ConnectionString"],
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "nomemodulo_schema"));
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        // Outbox workers
        services.AddHostedService<OutboxProcessingWorker<NomeModuloModule, NomeModuloDbContext>>();
        services.AddHostedService<OutboxRecoveryWorker<NomeModuloModule, NomeModuloDbContext>>();
        services.AddHostedService<OutboxCleanupWorker<NomeModuloModule, NomeModuloDbContext>>();

        // Services
        services.AddSingleton<IEventBus, InProcessEventBus<NomeModuloDbContext>>();

        services.AddScoped<IIntegrationEventPublisher<NomeModuloDbContext>, IntegrationEventPublisher<NomeModuloModule, NomeModuloDbContext>>();

        EventTypeRegistry.RegisterFromAssembly(typeof(NomeModuloModule).Assembly);

        // MediatR handlers do módulo
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(NomeModuloModule).Assembly));
    }

    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // TODO: Mapear endpoints
        // ExemploEndpoints.Map(endpoints);
    }
}
