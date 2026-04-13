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
/// NomeModulo module — entry point for registering services and endpoints.
/// </summary>
public sealed class NomeModuloModule : IModule
{
    public static string ModuleName => "NomeModulo";

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        DefaultServicesRegistration(services, configuration);

        // Register module-specific services here.
    }

    public static void RegisterControllers(IMvcBuilder mvcBuilder)
    {
        // Optional: configure filters/conventions for module controllers.
    }

    public static void DefaultServicesRegistration(IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider");
        var connectionString = configuration["ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"ConnectionString is required for module '{ModuleName}'.");
        }

        // DbContext
        services.AddDbContext<NomeModuloDbContext>((sp, options) =>
        {
            if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(
                    connectionString,
                    sqlite => sqlite.MigrationsHistoryTable("__EFMigrationsHistory"));
            }
            else if (string.IsNullOrWhiteSpace(databaseProvider)
                     || string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(
                    connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "nomemodulo_schema"));
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsupported DatabaseProvider '{databaseProvider}' for module '{ModuleName}'. Supported values: SqlServer, Sqlite.");
            }

            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<SqliteRowVersionSaveChangesInterceptor>());
        });

        // Messaging workers
        services.AddHostedService<InboxProcessingWorker<NomeModuloModule, NomeModuloDbContext>>();
        services.AddHostedService<InboxRecoveryWorker<NomeModuloModule, NomeModuloDbContext>>();
        services.AddHostedService<InboxCleanupWorker<NomeModuloModule, NomeModuloDbContext>>();
        services.AddHostedService<OutboxProcessingWorker<NomeModuloModule, NomeModuloDbContext>>();
        services.AddHostedService<OutboxRecoveryWorker<NomeModuloModule, NomeModuloDbContext>>();
        services.AddHostedService<OutboxCleanupWorker<NomeModuloModule, NomeModuloDbContext>>();

        // Services
        services.AddScoped<IInboxWriter<NomeModuloDbContext>, InboxWriter<NomeModuloModule, NomeModuloDbContext>>();
        services.AddScoped<IIntegrationEventPublisher<NomeModuloDbContext>, IntegrationEventPublisher<NomeModuloModule, NomeModuloDbContext>>();
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher<NomeModuloModule, NomeModuloDbContext>>();
    }

    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Optional: map minimal endpoints.
        // ExampleEndpoints.Map(endpoints);
    }
}
