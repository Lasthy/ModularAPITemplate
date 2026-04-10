using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.Modules.NomeModulo;

/// <summary>
/// Minimal module that serves static assets from wwwroot.
/// </summary>
public sealed class NomeModuloModule : IModule
{
    public static string ModuleName => "NomeModulo";

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(NomeModuloStaticFilesOptions.FromConfiguration(configuration));
        services.AddSingleton<IStartupFilter, NomeModuloStaticFilesStartupFilter>();
    }

    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<NomeModuloStaticFilesOptions>();
        var environment = endpoints.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var logger = endpoints.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<NomeModuloModule>();

        var staticRoot = ResolveStaticRoot(environment);
        if (staticRoot is null)
        {
            logger.LogWarning(
                "Module {ModuleName} could not find static assets. Expected paths: {SourcePath} or {OutputPath}.",
                ModuleName,
                BuildSourceRootPath(environment),
                BuildOutputRootPath());
            return;
        }

        var indexFilePath = Path.Combine(staticRoot, "index.html");
        if (!File.Exists(indexFilePath))
        {
            logger.LogWarning("Module {ModuleName} did not find index.html at {IndexFilePath}.", ModuleName, indexFilePath);
            return;
        }

        var fallbackPattern = BuildFallbackPattern(options.RoutePath);

        endpoints.MapFallback(fallbackPattern, async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            var html = await File.ReadAllTextAsync(indexFilePath, context.RequestAborted);
            await context.Response.WriteAsync(html, context.RequestAborted);
        });
    }

    private static string? ResolveStaticRoot(IHostEnvironment environment)
    {
        var sourceRoot = BuildSourceRootPath(environment);
        if (environment.IsDevelopment() && Directory.Exists(sourceRoot))
        {
            return sourceRoot;
        }

        var outputRoot = BuildOutputRootPath();
        if (Directory.Exists(outputRoot))
        {
            return outputRoot;
        }

        if (Directory.Exists(sourceRoot))
        {
            return sourceRoot;
        }

        return null;
    }

    private static string BuildSourceRootPath(IHostEnvironment environment)
    {
        return Path.GetFullPath(
            Path.Combine(environment.ContentRootPath, "..", "Modules", ModuleName, "wwwroot"));
    }

    private static string BuildOutputRootPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Modules", ModuleName, "wwwroot");
    }

    private static string BuildFallbackPattern(string routePath)
    {
        if (string.Equals(routePath, "/", StringComparison.Ordinal))
        {
            return "{*path:nonfile}";
        }

        var prefix = routePath.Trim('/');
        return $"{prefix}/{{*path:nonfile}}";
    }

    private sealed class NomeModuloStaticFilesStartupFilter(
        IHostEnvironment environment,
        NomeModuloStaticFilesOptions options,
        ILogger<NomeModuloStaticFilesStartupFilter> logger) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var staticRoot = ResolveStaticRoot(environment);

                if (staticRoot is null)
                {
                    next(app);
                    return;
                }

                var requestPath = string.Equals(options.RoutePath, "/", StringComparison.Ordinal)
                    ? string.Empty
                    : options.RoutePath;

                var fileProvider = new PhysicalFileProvider(staticRoot);

                app.UseDefaultFiles(new DefaultFilesOptions
                {
                    FileProvider = fileProvider,
                    RequestPath = requestPath,
                });

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = fileProvider,
                    RequestPath = requestPath,
                });

                logger.LogInformation(
                    "Module {ModuleName} serving static files from {StaticRoot} at route path {RoutePath}.",
                    ModuleName,
                    staticRoot,
                    options.RoutePath);

                next(app);
            };
        }
    }

    private sealed record NomeModuloStaticFilesOptions(string RoutePath)
    {
        public static NomeModuloStaticFilesOptions FromConfiguration(IConfiguration configuration)
        {
            var routePath = NormalizeRoutePath(configuration.GetValue<string>("RoutePath"));
            return new NomeModuloStaticFilesOptions(routePath);
        }

        private static string NormalizeRoutePath(string? routePath)
        {
            if (string.IsNullOrWhiteSpace(routePath))
            {
                return $"/{ModuleName.ToLowerInvariant()}";
            }

            var normalized = routePath.Trim();
            if (!normalized.StartsWith('/'))
            {
                normalized = "/" + normalized;
            }

            normalized = normalized.TrimEnd('/');
            return string.IsNullOrWhiteSpace(normalized) ? "/" : normalized;
        }
    }
}
