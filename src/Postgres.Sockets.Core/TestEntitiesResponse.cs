using System.ComponentModel.DataAnnotations;

namespace Postgres.Sockets.Core;

public sealed record TestEntitiesResponse
{
    [Required]
    public List<TestEntity> TestEntities { get; init; } = new();
}