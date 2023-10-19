using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using Postgres.Sockets.Core;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Database.Tests;

public class TestEntityRepositoryTests : TestBase
{
    private CancellationTokenSource _cts;

    private IConfiguration _configuration;
    private IWebSocketManager _webSocketManager;
    
    private TestEntityRepository _sut;

    [SetUp]
    public void SetUp()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new List<KeyValuePair<string, string>>
                {
                    new("ConnectionStrings:postgres", SetupFixture.DbConnectionString)
                }!)
            .Build();

        _webSocketManager = Substitute.For<IWebSocketManager>();
        
        _sut = new TestEntityRepository(
            SetupFixture.DbContext,
            _configuration,
            new NullLogger<TestEntityRepository>(),
            _webSocketManager);
        
        _cts = new CancellationTokenSource();
    }

    [Test]
    public async Task InsertTestEntityAsync_WhenTestEntityInserted_AssertTestEntityInDatabase()
    {
        //arrange
        var testEntityName = Guid.NewGuid().ToString();
        SetupFixture.DbContext.ChangeTracker.Clear();
        
        //act
        var act = await _sut.InsertTestEntityAsync(
            new TestEntityData { Name = testEntityName },
            _cts.Token);

        //assert
        var allTestEntities = await DatabaseHelper.GetTestEntitiesAsync(_cts.Token);
        allTestEntities.Count.Should().Be(1);
        allTestEntities.Single().TestEntityId.Should().Be(act.TestEntityId);
        allTestEntities.Single().Name.Should().Be(testEntityName);
    }
    
    [Test]
    public async Task UpdateTestEntityAsync_WhenTestEntityUpdated_AssertTestEntityUpdatedInDatabase()
    {
        //arrange
        var testEntity = await DatabaseHelper.InsertTestEntityAsync(
            new TestEntityData { Name = Guid.NewGuid().ToString() },
            _cts.Token);
        
        var allTestEntities = await DatabaseHelper.GetTestEntitiesAsync(_cts.Token);
        allTestEntities.Count.Should().Be(1);
        allTestEntities.Single().TestEntityId.Should().Be(testEntity.TestEntityId);
        
        //act
        var updateEntity = new TestEntityData
        {
            TestEntityId = testEntity.TestEntityId,
            Name = Guid.NewGuid().ToString()
        };
        var act = await _sut.UpdateTestEntityAsync(
            updateEntity,
            _cts.Token);

        //assert
        act.Should().BeTrue();
        allTestEntities = await DatabaseHelper.GetTestEntitiesAsync(_cts.Token);
        allTestEntities.Count.Should().Be(1);
        allTestEntities.Single().TestEntityId.Should().Be(testEntity.TestEntityId);
        allTestEntities.Single().Name.Should().Be(updateEntity.Name);
    }

    [Test]
    public async Task DeleteTestEntityAsync_WhenTestEntityDeleted_AssertTestEntityNotInDatabase()
    {
        //arrange
        var testEntity = await DatabaseHelper.InsertTestEntityAsync(
            new TestEntityData { Name = Guid.NewGuid().ToString() },
            _cts.Token);
        
        var allTestEntities = await DatabaseHelper.GetTestEntitiesAsync(_cts.Token);
        allTestEntities.Count.Should().Be(1);
        allTestEntities.Single().TestEntityId.Should().Be(testEntity.TestEntityId);
        
        //act
        var act = await _sut.DeleteTestEntityAsync(
            testEntity.TestEntityId,
            _cts.Token);

        //assert
        act.Should().BeTrue();
        allTestEntities = await DatabaseHelper.GetTestEntitiesAsync(_cts.Token);
        allTestEntities.Count.Should().Be(0);
    }

    [Test]
    public async Task GetTestEntitiesAsync_WhenTestEntityInserted_AssertOneTestEntityReturned()
    {
        //arrange
        var testEntity = await DatabaseHelper.InsertTestEntityAsync(
            new TestEntityData { Name = Guid.NewGuid().ToString() },
            _cts.Token);
        
        //act
        var act = await _sut.GetTestEntitiesAsync(_cts.Token);

        //assert
        act.Count.Should().Be(1);
        act.Single().TestEntityId.Should().Be(testEntity.TestEntityId);
        act.Single().Name.Should().Be(testEntity.Name);
    }

    [Test]
    public async Task GetTestEntityAsync_WhenTestEntityInserted_AssertTestEntityReturned()
    {
        //arrange
        var testEntity = await DatabaseHelper.InsertTestEntityAsync(
            new TestEntityData { Name = Guid.NewGuid().ToString() },
            _cts.Token);
        
        //act
        var act = await _sut.GetTestEntityAsync(testEntity.TestEntityId, _cts.Token);

        //assert
        act.Should().NotBeNull();
        act!.TestEntityId.Should().Be(testEntity.TestEntityId);
        act.Name.Should().Be(testEntity.Name);
    }

    [Test]
    public async Task GetTestEntityAsync_WhenTestEntityDoesNotExistForTestEntityId_AssertNullTestEntityReturned()
    {
        //act
        var act = await _sut.GetTestEntityAsync(-1, _cts.Token);

        //assert
        act.Should().BeNull();
    }
}