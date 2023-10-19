using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Tests;

public class GetTestEntityTests : TestBase
{
    private CancellationTokenSource _cts;

    [SetUp]
    public void TestSetUp()
    {
        _cts = new CancellationTokenSource();
    }

    [Test]
    public async Task GetTestEntity_WhenNoTestEntityExists_AssertNotFoundStatusCode()
    {
        //act
        var responseMessage = await Client.GetAsync("v1/1");

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetTestEntity_WhenTestEntityExists_AssertTestEntity()
    {
        //arrange
        var testEntity = await DatabaseHelper.InsertTestEntityAsync(
            DbContext,
            new TestEntityData
            {
                Name = Guid.NewGuid().ToString()
            }, _cts.Token);

        //act
        var responseMessage = await Client.GetAsync($"v1/{testEntity.TestEntityId}");

        //assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await responseMessage.Content.ReadFromJsonAsync<Core.TestEntity>();
        response.Should().NotBeNull();
        response!.TestEntityId.Should().Be(testEntity.TestEntityId);
        response.Name.Should().Be(testEntity.Name);
    }
}