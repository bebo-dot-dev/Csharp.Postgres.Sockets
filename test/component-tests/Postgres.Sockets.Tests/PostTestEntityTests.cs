using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Postgres.Sockets.Core;

namespace Postgres.Sockets.Tests;

public class PostTestEntityTests : TestBase
{
    private CancellationTokenSource _cts;
    
    [SetUp]
    public void TestSetUp()
    {
        _cts = new CancellationTokenSource();
    }
    
    [Test]
    public async Task PostTestEntity_WhenBadTestEntityPosted_AssertBadRequest()
    {
        //arrange
        var testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(0);

        //act
        var payload = new TestEntityRequest();
        var responseMessage = await Client.PostAsJsonAsync("v1", payload, _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(0);
    }

    [Test]
    public async Task PostTestEntity_WhenTestEntityPosted_AssertTestEntityCreated()
    {
        //arrange
        var testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(0);

        //act
        var payload = new TestEntityRequest
        {
            Name = Guid.NewGuid().ToString()
        };
        var responseMessage = await Client.PostAsJsonAsync("v1", payload, _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        var response = await responseMessage.Content.ReadFromJsonAsync<TestEntity>();
        response.Should().NotBeNull();

        testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(1);

        response!.TestEntityId.Should().Be(testEntities.Single().TestEntityId);
        response.Name.Should().Be(testEntities.Single().Name);
    }
    
    [Test]
    public async Task PostTestEntity_WhenTestEntityPostedWithWebSocket_AssertTestEntityCreatedAndInsertNotificationReceived()
    {
        //arrange
        var testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(0);
        
        await CreateWebSocketAsync(WebSocketContextType.Insert, _cts.Token);

        //act
        var payload = new TestEntityRequest
        {
            Name = Guid.NewGuid().ToString()
        };
        var responseMessage = await Client.PostAsJsonAsync("v1", payload, _cts.Token);

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        var response = await responseMessage.Content.ReadFromJsonAsync<TestEntity>();
        response.Should().NotBeNull();

        testEntities = await DatabaseHelper.GetTestEntitiesAsync(DbContext, _cts.Token);
        testEntities.Count.Should().Be(1);

        response!.TestEntityId.Should().Be(testEntities.Single().TestEntityId);
        response.Name.Should().Be(testEntities.Single().Name);
        
        var insertSocketMessage = await GetWebSocketNotificationAsync(_cts.Token);
        insertSocketMessage.Operation.Should().Be(WebSocketContextType.Insert);
        insertSocketMessage.Data.TestEntityId.Should().Be(testEntities.Single().TestEntityId);
        insertSocketMessage.Data.Name.Should().Be(testEntities.Single().Name);
    }
}