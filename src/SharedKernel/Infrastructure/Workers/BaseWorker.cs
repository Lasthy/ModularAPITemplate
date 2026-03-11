using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Workers;

/// <summary>
/// Worker base com logging e tratamento de erro padronizado.
/// Módulos podem criar workers herdando desta classe.
/// </summary>
public abstract class BaseWorker(
    IServiceScopeFactory scopeFactory,
    ILogger logger,
    TimeSpan? interval = null) : BackgroundService
{
    protected TimeSpan Interval { get; set; } = interval ?? TimeSpan.FromMinutes(1);
    private bool _cancellationRequested = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workerName = GetType().Name;
        logger.LogInformation("Worker {WorkerName} started.", workerName);

        while (!stoppingToken.IsCancellationRequested && !_cancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                await ExecuteJobAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error in worker {WorkerName}.", workerName);
            }

            await Task.Delay(Interval, stoppingToken);
        }

        logger.LogInformation("Worker {WorkerName} stopped.", workerName);
    }

    protected void RequestCancellation()
    {
        _cancellationRequested = true;
    }

    /// <summary>
    /// Implementação do trabalho do worker. Chamado a cada intervalo.
    /// </summary>
    protected abstract Task ExecuteJobAsync(IServiceProvider services, CancellationToken cancellationToken);
}
