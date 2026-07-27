using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackwise.Domain.Entities;

namespace Trackwise.Infrastructure.Persistence.Configurations;

public class CompetenceConfiguration : IEntityTypeConfiguration<Competence>
{
    public void Configure(EntityTypeBuilder<Competence> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100);
        builder.Property(c => c.NormalizedName).HasMaxLength(100);

        builder.HasIndex(c => c.NormalizedName)
            .IsUnique()
            .HasFilter("[NormalizedName] IS NOT NULL");
    }
}