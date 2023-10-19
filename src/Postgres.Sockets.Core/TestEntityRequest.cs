using System.ComponentModel.DataAnnotations;

namespace Postgres.Sockets.Core;

public record TestEntityRequest
{
    [Required]
    public string? Name { get; init; }
}