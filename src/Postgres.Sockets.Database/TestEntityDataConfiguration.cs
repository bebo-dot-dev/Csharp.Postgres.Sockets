using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Postgres.Sockets.Core.Outgoing;

namespace Postgres.Sockets.Database;

internal sealed class TestEntityDataConfiguration : IEntityTypeConfiguration<TestEntityData>
{
    public void Configure(EntityTypeBuilder<TestEntityData> builder)
    {
        builder.Property(d => d.TestEntityId).HasColumnName("testEntityId");
        builder.Property(d => d.Name).HasColumnName("name");
        
        builder.HasKey(d => d.TestEntityId);
        
        builder.ToTable("testEntity");
    }
}