using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Application.Context;
using ModularAPITemplate.SharedKernel.Infrastructure.Configuration;
using ModularAPITemplate.SharedKernel.Infrastructure.Events;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Infrastructure.Requests;

namespace ModularAPITemplate.SharedKernel.Modules;

/// <summary>
/// Extension methods to simplify module registration in the Host application.
/// </summary>
public static class ModuleExtensions
{
    private record ModuleAssemblyLoading { };

    /// <summary>
    /// Loads and registers modules defined in the <c>Modules</c> configuration section.
    /// </summary>
    /// <param name="services">DI service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>Updated service collection.</returns>
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while trying to add module {ModuleName}", moduleName);
            }
        }

        return services;
    }

    /// <summary>
    /// Registers a specific module into the DI container and configures its OpenAPI document.
    /// </summary>
    /// <typeparam name="TModule">Module type.</typeparam>
    /// <param name="services">DI service collection.</param>
    /// <param name="configuration">Module configuration section.</param>
    /// <returns>Updated service collection.</returns>
    public static IServiceCollection AddModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TModule : IModule
    {
        // Register shared services only once
        if (!services.Any(s => s.ServiceType == typeof(IRequestContext)))
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IRequestContext, RequestContext>();
            services.AddScoped<AuditSaveChangesInterceptor>();
            services.AddTransient<IDispatcher, Dispatcher>();
        }

        // Register module-specific messaging configuration for DI consumption
        services.AddTransient<OutboxConfiguration<TModule>>();
        services.AddTransient<InboxConfiguration<TModule>>();

        // Ensure a tracker is available (singleton) for collecting OpenAPI doc names
        var tracker = services
            .BuildServiceProvider()
            .GetService<OpenApiModuleTracker>();

        if (tracker is null)
        {
            tracker = new OpenApiModuleTracker();
            services.AddSingleton(tracker);
        }

        // Register the module's OpenAPI document name for the host to discover
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

        // Let the module register its own services
        TModule.RegisterServices(services, configuration);

        // Scan the module assembly for handlers and register them
        RegisterEventHandlers(services, typeof(TModule).Assembly);
        RegisterRequestHandlers(services, typeof(TModule).Assembly);

        return services;
    }

    /// <summary>
    /// Scans the given assembly and registers all implementations of <see cref="IEventHandler{T}"/>.
    /// </summary>
    private static void RegisterEventHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IEventHandler<>);
        var implementations = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface &&
                       t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface))
            .ToList();

        foreach (var implementation in implementations)
        {
            var interfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                .ToList();

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, implementation);
            }
        }
    }

    /// <summary>
    /// Scans the given assembly and registers all implementations of <see cref="IRequestHandler{TRequest}"/> and <see cref="IRequestHandler{TRequest,TResponse}"/>.
    /// </summary>
    private static void RegisterRequestHandlers(IServiceCollection services, Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface &&
                       t.GetInterfaces().Any(
                            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)
                            || i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        foreach (var implementation in implementations)
        {
            var interfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)
                            || i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .ToList();

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, implementation);
            }
        }
    }

    /// <summary>
    /// Invokes the module's endpoint configuration on the provided <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <typeparam name="TModule">Type of the module.</typeparam>
    /// <param name="endpoints">Endpoint builder.</param>
    /// <returns>The same endpoint builder for fluent chaining.</returns>
    public static IEndpointRouteBuilder MapModuleEndpoints<TModule>(
        this IEndpointRouteBuilder endpoints)
        where TModule : IModule
    {
        TModule.MapEndpoints(endpoints);
        return endpoints;
    }
}
