using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Postgres.Sockets.Core.Incoming.Commands;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Core.Tests;

public class MapperExtensionTests
{
    private string _entityName;
    
    [SetUp]
    public void SetUp()
    {
        _entityName = Guid.NewGuid().ToString();
    }
    
    [Test]
    public void ToTestEntities_WhenGivenListOfTestEntityData_AssertMapsCorrectly()
    {
        var input = new List<TestEntityData>
        {
            new()
            {
                TestEntityId = 1,
                Name = _entityName
            }
        };

        var act = input.ToTestEntities();

        act.Should().BeEquivalentTo(
            new List<TestEntity>
            {
                new()
                {
                    TestEntityId = 1,
                    Name = _entityName
                }
            });
    }
    
    [Test]
    public void ToTestEntity_WhenGivenTestEntityData_AssertMapsCorrectly()
    {
        var input = new TestEntityData
        {
            TestEntityId = 1,
            Name = _entityName
        };

        var act = input.ToTestEntity();

        act.Should().BeEquivalentTo(
            new TestEntity
            {
                TestEntityId = 1,
                Name = _entityName
            });
    }
    
    [Test]
    public void ToTestEntity_WhenGivenNullTestEntityData_AssertMapsNullCorrectly()
    {
        var act = ((TestEntityData)null).ToTestEntity();
        act.Should().BeNull();
    }

    [Test]
    public void ToCreateTestEntityCommand_WhenGivenCreateTestEntityRequest_AssertMapsCorrectly()
    {
        var input = new TestEntityRequest
        {
            Name = _entityName
        };
        var act = input.ToInsertTestEntityCommand();
        act.Should().BeEquivalentTo(
            new InsertTestEntityCommand
            {
                Name = _entityName
            });
    }
}