using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using THtracker.Domain.Entities;

namespace THtracker.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token).IsRequired().HasMaxLength(200); // Or however long the token is
        builder.HasIndex(x => x.Token).IsUnique();

        builder.Property(x => x.DeviceInfo).HasMaxLength(200);
        builder.Property(x => x.CreatedByIp).HasMaxLength(45); // IPv6 max length
        builder.Property(x => x.RevokedByIp).HasMaxLength(45);
    }
}
