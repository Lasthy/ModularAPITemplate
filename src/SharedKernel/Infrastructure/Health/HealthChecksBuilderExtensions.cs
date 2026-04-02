using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModularAPITemplate.SharedKernel.Infrastructure.Persistence;
using ModularAPITemplate.SharedKernel.Modules;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Health;

/// <summary>
/// Extension methods for registering module health checks.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    /// Registers a module health check that validates database connectivity and inbox/outbox backlog.
    /// </summary>
    /// <typeparam name="TContext">Module DbContext type.</typeparam>
    /// <typeparam name="TModule">Module marker type.</typeparam>
    /// <param name="builder">Health checks builder.</param>
    /// <param name="healthCheckName">Optional custom check name. Defaults to lower-case module name.</param>
    /// <returns>The same builder for fluent chaining.</returns>
    public static IHealthChecksBuilder AddModuleHealthCheck<TContext, TModule>(
        this IHealthChecksBuilder builder,
        string? healthCheckName = null)
        where TContext : DbContext, IBaseDbContext
        where TModule : IModule
    {
        var checkName = string.IsNullOrWhiteSpace(healthCheckName)
            ? TModule.ModuleName.ToLowerInvariant()
            : healthCheckName;

        return builder.AddCheck<ModuleHealthCheck<TContext, TModule>>(
            checkName,
            tags: new[] { "module", TModule.ModuleName });
    }
}