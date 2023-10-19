using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Database;

[ExcludeFromCodeCoverage]
public static class PostgresDataDependencyInjectionExtensions
{
    public static IServiceCollection RegisterPostgresDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PostgresDbContext>((_, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("postgres"));
        });
        
        services.AddScoped<DbContext, PostgresDbContext>();

        services.AddScoped<ITestEntityRepository, TestEntityRepository>();
        
        return services;
    }

    /// <summary>
    /// Applies database migrations at runtime
    /// Good enough for this demo application, this approach is inappropriate for managing production databases.
    /// See https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#apply-migrations-at-runtime
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/></param>
    /// <param name="configuration">The <see cref="IConfiguration"/></param>
    public static void MigrateDatabase(
        this IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        var options = new DbContextOptions<PostgresDbContext>();
        using var dbContext = new PostgresDbContext(options, configuration);
        
        var logger = serviceProvider.GetRequiredService<ILogger<PostgresDbContext>>();
        try
        {
            dbContext.Database.Migrate();
            logger.LogInformation("EF Core Postgres database migrations ran successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, "EF Core Postgres database migrations failure");
        }
    }
}