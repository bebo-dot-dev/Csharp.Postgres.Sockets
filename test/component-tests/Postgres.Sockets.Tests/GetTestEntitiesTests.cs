using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Postgres.Sockets.Core;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Tests;

public class GetTestEntitiesTests : TestBase
{
    private CancellationTokenSource _cts;

    [SetUp]
    public void TestSetUp()
    {
        _cts = new CancellationTokenSource();
    }

    [Test]
    public async Task GetTestEntities_WhenNoTestEntitiesExists_AssertNoTestEntities()
    {
        //act
        var responseMessage = await Client.GetAsync("v1/testEntities");

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await responseMessage.Content.ReadFromJsonAsync<TestEntitiesResponse>();
        response!.TestEntities.Count.Should().Be(0);
    }

    [Test]
    public async Task GetTestEntities_WhenTestEntitiesExists_AssertTestEntities()
    {
        //arrange
        var testEntity = await DatabaseHelper.InsertTestEntityAsync(
            DbContext,
            new TestEntityData
            {
                Name = Guid.NewGuid().ToString()
            }, _cts.Token);

        //act
        var responseMessage = await Client.GetAsync("v1/testEntities");

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await responseMessage.Content.ReadFromJsonAsync<TestEntitiesResponse>();
        response!.TestEntities.Count.Should().Be(1);
        response.TestEntities.Single().TestEntityId.Should().Be(testEntity.TestEntityId);
        response.TestEntities.Single().Name.Should().Be(testEntity.Name);
    }
}