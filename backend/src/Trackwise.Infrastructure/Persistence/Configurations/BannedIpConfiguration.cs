using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackwise.Domain.Entities;

namespace Trackwise.Infrastructure.Persistence.Configurations;

public class BannedIpConfiguration : IEntityTypeConfiguration<BannedIp>
{
    public void Configure(EntityTypeBuilder<BannedIp> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IpAddress).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasIndex(x => x.IpAddress).IsUnique();
    }
}