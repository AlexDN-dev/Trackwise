using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackwise.Domain.Entities;

namespace Trackwise.Infrastructure.Persistence.Configurations;

public class UserCompetenceConfiguration : IEntityTypeConfiguration<UserCompetence>
{
    public void Configure(EntityTypeBuilder<UserCompetence> builder)
    {
        builder.HasKey(uc => new { uc.UserId, uc.CompetenceId });

        builder.HasOne(uc => uc.User)
            .WithMany(u => u.UserCompetences)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.Competence)
            .WithMany()
            .HasForeignKey(uc => uc.CompetenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(uc => uc.UserId);
    }
}