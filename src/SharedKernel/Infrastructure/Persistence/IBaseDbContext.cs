using Cysharp.EntityFrameworkCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularAPITemplate.SharedKernel.Domain;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// DbContext base que despacha eventos de domínio automaticamente ao salvar.
/// Todos os módulos devem herdar deste DbContext.
/// </summary>
public interface IBaseDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
