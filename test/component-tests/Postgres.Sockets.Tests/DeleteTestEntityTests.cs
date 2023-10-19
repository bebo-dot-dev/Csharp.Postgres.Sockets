using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Postgres.Sockets.Core;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Tests;

public class DeleteTestEntityTests : TestBase
{
    private CancellationTokenSource _cts;
    
    [SetUp]
    public void TestSetUp()
    {
        _cts = new CancellationTokenSource();
    }

    [Test]
    public async Task DeleteTestEntity_WhenNoTestEntityExists_AssertNotFoundStatusCode()
    {
        //act
        var responseMessage = await Client.DeleteAsync("v1/1");

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteTestEntity_WhenTestEntityDelete_AssertTestEntityDeleted()
    {
        //arrange
        var testEntity = await DatabaseHelper.InsertTestEntityAsync(
            DbContext,
            new TestEntityData
            {
                Name = Guid.NewGuid().ToString()
            }, _cts.Token);
        var testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(1);

        //act
        var responseMessage = await Client.DeleteAsync($"v1/{testEntity.TestEntityId}", _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);

        testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(0);
    }
    
    [Test]
    public async Task DeleteTestEntity_WhenTestEntityDeleteWithWebSocket_AssertTestEntityDeletedAndDeleteNotificationReceived()
    {
        //arrange
        var testEntity = await DatabaseHelper.InsertTestEntityAsync(
            DbContext,
            new TestEntityData
            {
                Name = Guid.NewGuid().ToString()
            }, _cts.Token);
        var testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(1);

        await CreateWebSocketAsync(WebSocketContextType.Delete, _cts.Token);
        
        //act
        var responseMessage = await Client.DeleteAsync($"v1/{testEntity.TestEntityId}", _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);

        testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(0);
        
        var deleteSocketMessage = await GetWebSocketNotificationAsync(_cts.Token);
        deleteSocketMessage.Operation.Should().Be(WebSocketContextType.Delete);
        deleteSocketMessage.Data.TestEntityId.Should().Be(testEntity.TestEntityId);
        deleteSocketMessage.Data.Name.Should().Be(testEntity.Name);
    }
}