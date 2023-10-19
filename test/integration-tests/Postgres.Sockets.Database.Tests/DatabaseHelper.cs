using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Database.Tests;

internal static class DatabaseHelper
{
    public static async Task<List<TestEntityData>> GetTestEntitiesAsync(CancellationToken cancellationToken)
    {
        return await SetupFixture.DbContext.TestEntities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public static async Task<TestEntityData> InsertTestEntityAsync(
        TestEntityData testEntity, CancellationToken cancellationToken)
    {
        SetupFixture.DbContext.ChangeTracker.Clear();
        SetupFixture.DbContext.TestEntities.Add(testEntity);
        await SetupFixture.DbContext.SaveChangesAsync(cancellationToken);
        return testEntity;
    }
}