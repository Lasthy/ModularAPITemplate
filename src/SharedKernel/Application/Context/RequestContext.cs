using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ModularAPITemplate.SharedKernel.Application.Context;

/// <summary>
/// <see cref="IRequestContext"/> implementation based on <see cref="HttpContext"/>.
/// Extracts authentication information and claims from the current HTTP request.
/// Registered as scoped in the DI container.
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
    /// Protected constructor to allow modules to create derived contexts
    /// without depending on <see cref="IHttpContextAccessor"/>.
    /// </summary>
    protected RequestContext(ClaimsPrincipal? user)
    {
        _user = user;
    }

    public UserIdType? UserId
    {
        get
        {
            var sub = _user?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? _user?.FindFirstValue("sub");

            return sub != null ? UserIdType.Parse(sub) : (UserIdType?)null;
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
