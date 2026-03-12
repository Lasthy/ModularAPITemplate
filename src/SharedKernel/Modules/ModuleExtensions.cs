using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Application.Context;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

namespace ModularAPITemplate.SharedKernel.Modules;

/// <summary>
/// Métodos de extensão para registro simplificado de módulos no Host.
/// </summary>
public static class ModuleExtensions
{
    private static bool _sharedServicesRegistered;

    private record ModuleAssemblyLoading { };

    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration is null)
        {
            return services;
        }

        var modulesSection = configuration.GetSection("Modules");
        if (!modulesSection.Exists())
            return services;

        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ModuleAssemblyLoading>>();

        foreach (var moduleSection in modulesSection.GetChildren())
        {
            var moduleName = moduleSection.Key;
            if (string.IsNullOrWhiteSpace(moduleName))
                continue;

            var assemblyName = string.Concat(moduleName, ".Module");

            // Try to find a loaded assembly first, then attempt to load by name
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));

            if (assembly is null)
            {
                try
                {
                    assembly = Assembly.Load(new AssemblyName(assemblyName));
                }
                catch
                {
                    continue;
                }
            }

            Type? moduleType = null;
            try
            {
                moduleType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error occurred while trying to get module type from assembly {AssemblyName}", assemblyName);   

                continue;
            }

            if (moduleType is null)
                continue;

            // Invoke AddModule<TModule>(IServiceCollection, IConfiguration) via reflection,
            // passing the module-specific IConfigurationSection
            var addModuleMethod = typeof(ModuleExtensions).GetMethod(nameof(AddModule), BindingFlags.Public | BindingFlags.Static);
            if (addModuleMethod is null)
                continue;

            var generic = addModuleMethod.MakeGenericMethod(moduleType);
            try
            {
                generic.Invoke(null, new object[] { services, moduleSection });
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error occurred while trying to add module {ModuleName}", moduleName);
            }
        }

        return services;
    }

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
            services.AddScoped<AuditSaveChangesInterceptor>();
            _sharedServicesRegistered = true;
        }
        
        // Registra a configuração do Outbox do módulo para injeção, se necessário
        services.AddTransient<OutboxConfiguration<TModule>>();

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
