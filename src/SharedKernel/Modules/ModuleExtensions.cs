using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularAPITemplate.SharedKernel.Application.Context;

namespace ModularAPITemplate.SharedKernel.Modules;

/// <summary>
/// Métodos de extensão para registro simplificado de módulos no Host.
/// </summary>
public static class ModuleExtensions
{
    private static bool _sharedServicesRegistered;

    /// <summary>
    /// Registra os serviços de um módulo no container de DI.
    /// Também registra um documento OpenAPI exclusivo para o módulo.
    /// </summary>
    public static IServiceCollection AddModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TModule : IModule
    {
        // Registra serviços compartilhados apenas uma vez
        if (!_sharedServicesRegistered)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IRequestContext, RequestContext>();
            _sharedServicesRegistered = true;
        }

        // Garante que o tracker esteja registrado como singleton
        var tracker = services
            .BuildServiceProvider()
            .GetService<OpenApiModuleTracker>();

        if (tracker is null)
        {
            tracker = new OpenApiModuleTracker();
            services.AddSingleton(tracker);
        }

        // Registra o documento OpenAPI do módulo
        var moduleName = TModule.ModuleName;
        tracker.Add(moduleName);

        // Workaround: .NET 10's AddOpenApi lowercases the DocumentName internally, but the
        // default ShouldInclude uses case-sensitive comparison against GroupName. Since our
        // endpoints use PascalCase GroupName (e.g. "Identity") and DocumentName becomes
        // "identity", the default filter silently excludes all endpoints.
        // See: https://github.com/dotnet/aspnetcore — OpenApiServiceCollectionExtensions.cs
        services.AddOpenApi(moduleName, options =>
        {
            options.ShouldInclude = (description) =>
                description.GroupName is null
                || string.Equals(description.GroupName, moduleName, StringComparison.OrdinalIgnoreCase);
        });

        TModule.RegisterServices(services, configuration);
        return services;
    }

    /// <summary>
    /// Mapeia os endpoints de um módulo no pipeline HTTP.
    /// </summary>
    public static IEndpointRouteBuilder MapModuleEndpoints<TModule>(
        this IEndpointRouteBuilder endpoints)
        where TModule : IModule
    {
        TModule.MapEndpoints(endpoints);
        return endpoints;
    }
}
