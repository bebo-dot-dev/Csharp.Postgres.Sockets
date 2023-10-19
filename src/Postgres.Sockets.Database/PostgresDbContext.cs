using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Database;

public sealed class PostgresDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }
    
    public DbSet<TestEntityData> TestEntities { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("postgres"), o =>
            {
                o.CommandTimeout(5);
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresDbContext).Assembly);
    }
}