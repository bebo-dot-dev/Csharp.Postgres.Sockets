using System.Threading.Tasks;
using NUnit.Framework;

namespace Postgres.Sockets.Database.Tests;

public abstract class TestBase
{
    [TearDown]
    public async Task TearDown()
    {
        await SetupFixture.ClearDownDatabase();
    }
}