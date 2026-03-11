namespace ModularAPITemplate.SharedKernel.Application.Context;

/// <summary>
/// Contexto da requisição HTTP atual.
/// Fornece informações de autenticação e claims do usuário logado.
/// Pode ser injetado em handlers, use cases e serviços.
/// </summary>
public interface IRequestContext
{
    /// <summary>
    /// Identificador do usuário autenticado (claim "sub" ou "nameidentifier").
    /// Null se não autenticado.
    /// </summary>
    UserIdType? UserId { get; }

    /// <summary>
    /// Nome do usuário autenticado.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Indica se a requisição possui um usuário autenticado.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Roles do usuário autenticado.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Todas as claims do usuário como dicionário (tipo → valor).
    /// Claims com múltiplos valores terão apenas o primeiro valor.
    /// Use <see cref="GetClaimValues"/> para obter todos os valores.
    /// </summary>
    IReadOnlyDictionary<string, string> Claims { get; }

    /// <summary>
    /// Retorna o valor de uma claim específica, ou null se não existir.
    /// </summary>
    string? GetClaim(string claimType);

    /// <summary>
    /// Retorna todos os valores de uma claim específica.
    /// </summary>
    IReadOnlyList<string> GetClaimValues(string claimType);

    /// <summary>
    /// Verifica se o usuário possui uma role específica.
    /// </summary>
    bool IsInRole(string role);
}
