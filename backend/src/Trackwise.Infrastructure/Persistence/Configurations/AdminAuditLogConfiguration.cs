using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackwise.Domain.Entities;

namespace Trackwise.Infrastructure.Persistence.Configurations;

public class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Details).HasMaxLength(1000);
        builder.Property(x => x.IpAddress).HasMaxLength(64);

        builder.HasOne(x => x.Admin)
            .WithMany()
            .HasForeignKey(x => x.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetUser)
            .WithMany()
            .HasForeignKey(x => x.TargetUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.AdminId);
        builder.HasIndex(x => x.TargetUserId);
    }
}