using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularAPITemplate.SharedKernel.Modules;

/// <summary>
/// Contrato que todo módulo deve implementar.
/// Responsável por registrar os serviços e endpoints do módulo.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Nome do módulo, usado como identificador do documento OpenAPI.
    /// </summary>
    static abstract string ModuleName { get; }

    /// <summary>
    /// Registra os serviços (DI) do módulo.
    /// </summary>
    static abstract void RegisterServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Mapeia os endpoints HTTP do módulo.
    /// </summary>
    static abstract void MapEndpoints(IEndpointRouteBuilder endpoints);
}
