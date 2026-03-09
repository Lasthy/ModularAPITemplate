using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularAPITemplate.Modules.Produtos.Endpoints;
using ModularAPITemplate.Modules.Produtos.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.Modules.Produtos;

/// <summary>
/// Módulo de Produtos — ponto de entrada para registro de serviços e endpoints.
/// </summary>
public sealed class ProdutosModule : IModule
{
    public static string ModuleName => "Produtos";

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ProdutosDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("ProdutosDb"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "produtos")));

        // MediatR handlers do módulo
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ProdutosModule).Assembly));
    }

    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ProdutoEndpoints.Map(endpoints);
    }
}
