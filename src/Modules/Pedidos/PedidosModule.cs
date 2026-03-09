using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularAPITemplate.Modules.Pedidos.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.Modules.Pedidos;

/// <summary>
/// Módulo Pedidos — ponto de entrada para registro de serviços e endpoints.
/// </summary>
public sealed class PedidosModule : IModule
{
    public static string ModuleName => "Pedidos";

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<PedidosDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("PedidosDb"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "pedidos")));

        // MediatR handlers do módulo
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(PedidosModule).Assembly));
    }

    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // TODO: Mapear endpoints
        // ExemploEndpoints.Map(endpoints);
    }
}
