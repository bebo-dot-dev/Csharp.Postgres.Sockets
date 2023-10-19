using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Core.HostedService;

internal class DataListener : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DataListener(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var listenerJob = scope.ServiceProvider.GetRequiredService<IBackgroundJob>();
        await listenerJob.ExecuteAsync(stoppingToken);
    }
}

internal interface IBackgroundJob
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}

internal class DataListenerJob : IBackgroundJob
{
    private readonly ITestEntityRepository _repository;

    public DataListenerJob(ITestEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _repository.ListenForEntityChangesAsync(stoppingToken);
    }
}