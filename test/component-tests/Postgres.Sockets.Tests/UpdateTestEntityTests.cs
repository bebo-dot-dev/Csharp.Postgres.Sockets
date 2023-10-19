using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Postgres.Sockets.Core;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Tests;

public class UpdateTestEntityTests : TestBase
{
    private CancellationTokenSource _cts;
    
    [SetUp]
    public void TestSetUp()
    {
        _cts = new CancellationTokenSource();
    }
    
    [Test]
    public async Task UpdateTestEntity_WhenBadTestEntityUpdate_AssertBadRequest()
    {
        //act
        var payload = new TestEntityRequest();
        var responseMessage = await Client.PatchAsync(
            $"v1/1", 
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json-patch+json"),
            _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task UpdateTestEntity_WhenNoTestEntityExists_AssertNotFoundStatusCode()
    {
        var payload = new TestEntityRequest
        {
            Name = Guid.NewGuid().ToString()
        };
        
        //act
        var responseMessage = await Client.PatchAsync(
            "v1/1", 
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json-patch+json"),
            _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateTestEntity_WhenTestEntityUpdate_AssertTestEntityUpdated()
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
        
        var updatePayload = new TestEntityRequest
        {
            Name = Guid.NewGuid().ToString()
        };

        //act
        var responseMessage = await Client.PatchAsync(
            $"v1/{testEntity.TestEntityId}", 
            new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json-patch+json"),
            _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);

        testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(1);
        testEntities.Single().Name.Should().Be(updatePayload.Name);
    }
    
    [Test]
    public async Task UpdateTestEntity_WhenTestEntityUpdateWithWebSocket_AssertTestEntityUpdatedAndUpdateNotificationReceived()
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
        
        var updatePayload = new TestEntityRequest
        {
            Name = Guid.NewGuid().ToString()
        };
        
        await CreateWebSocketAsync(WebSocketContextType.Update, _cts.Token);

        //act
        var responseMessage = await Client.PatchAsync(
            $"v1/{testEntity.TestEntityId}", 
            new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json-patch+json"),
            _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);

        testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(1);
        testEntities.Single().Name.Should().Be(updatePayload.Name);
        
        var updateSocketMessage = await GetWebSocketNotificationAsync(_cts.Token);
        updateSocketMessage.Operation.Should().Be(WebSocketContextType.Update);
        updateSocketMessage.Data.TestEntityId.Should().Be(testEntity.TestEntityId);
        updateSocketMessage.Data.Name.Should().Be(updatePayload.Name);
    }
}