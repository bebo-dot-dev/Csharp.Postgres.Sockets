using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Postgres.Sockets.Core;
using Postgres.Sockets.Database;
using Testcontainers.PostgreSql;

namespace Postgres.Sockets.Tests;

public abstract class TestBase
{
    private WebApplicationFactory<Program> _application;
    private PostgreSqlContainer _postgresTestContainer;

    protected HttpClient Client;
    protected PostgresDbContext DbContext;
    
    private WebSocket _socket;
    private ArraySegment<byte> _socketBuffer;
    
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
        IncludeFields = true
    };

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _postgresTestContainer = new PostgreSqlBuilder()
            .WithImage("postgres")
            .Build();

        await _postgresTestContainer.StartAsync();

        var connectionString = _postgresTestContainer.GetConnectionString();
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                b => { b.UseSetting("ConnectionStrings:postgres", connectionString); });

        Client = _application.CreateClient();
        
        CreateDatabaseContext(connectionString);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await ClearDownDatabase();
    }

    /// <summary>
    /// Create a new client web socket and a worker thread for WebSocket.ReceiveAsync to work in the background.
    /// </summary>
    protected async Task CreateWebSocketAsync(WebSocketContextType context, CancellationToken cancellationToken)
    {
        var socketClient = _application.Server.CreateWebSocketClient();
        var wsUri = new UriBuilder(_application.Server.BaseAddress) 
        {
            Scheme = "ws",
            Path = "ws/" + context
        }.Uri;
        _socket = await socketClient.ConnectAsync(wsUri, cancellationToken);
        
        var socketThread = new Thread(SocketWorker);
        socketThread.Start(cancellationToken);
    }
    
    private async void SocketWorker(object cancellationToken)
    {
        Thread.CurrentThread.IsBackground = true;
        _socketBuffer = new ArraySegment<byte>(new byte[1024]);
        var receiveResult = await _socket.ReceiveAsync(_socketBuffer, (CancellationToken)cancellationToken);
        _socketBuffer = new ArraySegment<byte>(_socketBuffer.ToArray(), 0, receiveResult.Count);
    }

    protected async Task<NotificationMessage> GetWebSocketNotificationAsync(CancellationToken cancellationToken)
    {
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "client socket shutdown", cancellationToken);
        var messagePayload = Encoding.UTF8.GetString(_socketBuffer.ToArray());
        return JsonSerializer.Deserialize<NotificationMessage>(messagePayload, _jsonSerializerOptions);
    }

    private void CreateDatabaseContext(string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:postgres", connectionString)
                }!)
            .Build();

        var options = new DbContextOptions<PostgresDbContext>();
        DbContext = new PostgresDbContext(options, configuration);
    }

    private async Task ClearDownDatabase()
    {
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE \"testEntity\" RESTART IDENTITY");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _postgresTestContainer.StopAsync();
        await _postgresTestContainer.DisposeAsync();
        await _application.DisposeAsync();
    }
}