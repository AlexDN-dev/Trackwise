using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackwise.Domain.Entities;

namespace Trackwise.Infrastructure.Persistence.Configurations;

public class EntrepriseConfiguration : IEntityTypeConfiguration<Entreprise>
{
    public void Configure(EntityTypeBuilder<Entreprise> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(150);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.Sector).HasMaxLength(100);
        builder.Property(e => e.Website).HasMaxLength(256);
    }
}