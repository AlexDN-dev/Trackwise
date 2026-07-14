using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackwise.Domain.Entities;

namespace Trackwise.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(256);
        builder.Property(u => u.Firstname).HasMaxLength(50);
        builder.Property(u => u.Lastname).HasMaxLength(50);
        builder.Property(u => u.PasswordHash).HasMaxLength(500);
    }
}