using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Postgres.Sockets.Core.Outgoing;
using Postgres.Sockets.Database;

namespace Postgres.Sockets.Tests;

internal static class DatabaseHelper
{
    public static async Task<List<TestEntityData>> GetTestEntitiesAsync(
        PostgresDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await dbContext.TestEntities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public static async Task<TestEntityData> InsertTestEntityAsync(
        PostgresDbContext dbContext,
        TestEntityData testEntity, 
        CancellationToken cancellationToken)
    {
        dbContext.ChangeTracker.Clear(); 
        dbContext.TestEntities.Add(testEntity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return testEntity;
    }
}