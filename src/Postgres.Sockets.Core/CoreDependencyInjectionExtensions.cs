using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Postgres.Sockets.Core.HostedService;

namespace Postgres.Sockets.Core;

[ExcludeFromCodeCoverage]
public static class CoreDependencyInjectionExtensions
{
    public static IServiceCollection RegisterCoreServices(
        this IServiceCollection services)
    {
        return services
            .AddHostedService<DataListener>()
            .AddTransient<IBackgroundJob, DataListenerJob>()
            .AddSingleton<IWebSocketManager, WebSocketManager>();
    }
}