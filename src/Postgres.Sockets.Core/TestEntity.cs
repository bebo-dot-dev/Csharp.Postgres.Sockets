namespace Postgres.Sockets.Core;

public record TestEntity
{
    public int TestEntityId { get; init; }

    public string Name { get; init; } = null!;
}