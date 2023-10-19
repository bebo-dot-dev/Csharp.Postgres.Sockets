using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Testcontainers.PostgreSql;

// ReSharper disable once CheckNamespace

namespace Postgres.Sockets.Database.Tests;

/// <summary>
/// The non-namespaced Nunit SetupFixture for all test fixtures in this project
/// </summary>
[SetUpFixture]
internal class SetupFixture
{
    private static PostgreSqlContainer _postgresTestContainer;
    private static IConfiguration _configuration;

    public static string DbConnectionString;
    public static PostgresDbContext DbContext;

    [OneTimeSetUp]
    public static async Task OneTimeSetup()
    {
        _postgresTestContainer = new PostgreSqlBuilder()
            .WithImage("postgres")
            .Build();

        await _postgresTestContainer.StartAsync();

        DbConnectionString = _postgresTestContainer.GetConnectionString();
        CreateDatabaseContext();
    }

    private static void CreateDatabaseContext()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:postgres", DbConnectionString)
                })
            .Build();

        var options = new DbContextOptions<PostgresDbContext>();
        DbContext = new PostgresDbContext(options, _configuration);
        DbContext.Database.Migrate();
    }

    public static async Task ClearDownDatabase()
    {
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE \"testEntity\" RESTART IDENTITY");
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        await DbContext.Database.CloseConnectionAsync();
        await DbContext.DisposeAsync();

        await _postgresTestContainer.StopAsync();
        await _postgresTestContainer.DisposeAsync();
    }
}