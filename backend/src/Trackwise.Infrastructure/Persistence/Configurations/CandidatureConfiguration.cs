using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackwise.Domain.Entities;

namespace Trackwise.Infrastructure.Persistence.Configurations;

public class CandidatureConfiguration : IEntityTypeConfiguration<Candidature>
{
    public void Configure(EntityTypeBuilder<Candidature> builder)
    {
        builder.Property(c => c.Title).HasMaxLength(100);
        builder.Property(c => c.Location).HasMaxLength(100);
        builder.Property(c => c.UrlOffer).HasMaxLength(2000);
        builder.Property(c => c.Description).HasMaxLength(2000);

        builder.HasOne(c => c.Entreprise)
            .WithMany(e => e.Candidatures)
            .HasForeignKey(c => c.EntrepriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Candidatures)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}