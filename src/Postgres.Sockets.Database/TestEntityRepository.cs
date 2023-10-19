using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Postgres.Sockets.Core;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Database;

internal sealed class TestEntityRepository : ITestEntityRepository
{
    private readonly PostgresDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TestEntityRepository> _logger;
    private readonly IWebSocketManager _webSocketManager;

    public TestEntityRepository(
        PostgresDbContext context, 
        IConfiguration configuration, 
        ILogger<TestEntityRepository> logger, 
        IWebSocketManager webSocketManager)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _webSocketManager = webSocketManager;
    }

    public async Task<List<TestEntityData>> GetTestEntitiesAsync(
        CancellationToken cancellationToken)
    {
        return await _context.TestEntities
            .ToListAsync(cancellationToken);
    }

    public async Task<TestEntityData?> GetTestEntityAsync(
        int testEntityId, CancellationToken cancellationToken)
    {
        return await _context.TestEntities
            .Where(d => d.TestEntityId == testEntityId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TestEntityData> InsertTestEntityAsync(
        TestEntityData testEntity, CancellationToken cancellationToken)
    {
        _context.TestEntities.Add(testEntity);
        await _context.SaveChangesAsync(cancellationToken);
        return testEntity;
    }

    public async Task<bool> UpdateTestEntityAsync(
        TestEntityData testEntity, 
        CancellationToken cancellationToken)
    {
        var affectedRowCount = await _context.TestEntities
            .Where(d => d.TestEntityId == testEntity.TestEntityId)    
            .ExecuteUpdateAsync(p => 
                p.SetProperty(te => te.Name, te => testEntity.Name),
                cancellationToken);
        return affectedRowCount == 1;
    }

    public async Task<bool> DeleteTestEntityAsync(
        int testEntityId, CancellationToken cancellationToken)
    {
        var affectedRowCount = await _context.TestEntities
            .Where(d => d.TestEntityId == testEntityId)
            .ExecuteDeleteAsync(cancellationToken);
        return affectedRowCount == 1;
    }

    public async Task ListenForEntityChangesAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("postgres"));
        await conn.OpenAsync(cancellationToken);
        conn.Notification += async (_, args) =>
        {
            _logger.LogInformation("Received data change notification:\n {payload} from process {PID}", args.Payload, args.PID);
            await _webSocketManager.HandleDataChangeNotificationAsync(args.Payload, cancellationToken);
        };

        await using (var cmd = new NpgsqlCommand(@"LISTEN ""test_entity_data_changed""", conn)) {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        while (!cancellationToken.IsCancellationRequested) {
            await conn.WaitAsync(cancellationToken); // Thread block waiting for data change notifications.
        }
    }
}