using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ModularAPITemplate.SharedKernel.Application.Context;

/// <summary>
/// Implementação de <see cref="IRequestContext"/> baseada no <see cref="HttpContext"/>.
/// Extrai informações de autenticação e claims do usuário da requisição HTTP atual.
/// Registrado como Scoped no container de DI.
/// </summary>
public class RequestContext : IRequestContext
{
    private readonly ClaimsPrincipal? _user;
    private IReadOnlyList<string>? _roles;
    private IReadOnlyDictionary<string, string>? _claims;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    /// <summary>
    /// Construtor protegido para permitir que módulos criem contextos derivados
    /// sem depender de <see cref="IHttpContextAccessor"/>.
    /// </summary>
    protected RequestContext(ClaimsPrincipal? user)
    {
        _user = user;
    }

    public Guid? UserId
    {
        get
        {
            var sub = _user?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? _user?.FindFirstValue("sub");

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? UserName =>
        _user?.FindFirstValue(ClaimTypes.Name)
        ?? _user?.FindFirstValue("name");

    public bool IsAuthenticated =>
        _user?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        _roles ??= _user?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly()
        ?? [];

    public IReadOnlyDictionary<string, string> Claims =>
        _claims ??= _user?.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => g.First().Value)
            .AsReadOnly()
        ?? new Dictionary<string, string>().AsReadOnly();

    public string? GetClaim(string claimType) =>
        _user?.FindFirstValue(claimType);

    public IReadOnlyList<string> GetClaimValues(string claimType) =>
        _user?.FindAll(claimType)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly()
        ?? [];

    public bool IsInRole(string role) =>
        _user?.IsInRole(role) ?? false;
}
