using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using THtracker.Domain.Entities;

namespace THtracker.Infrastructure.Persistence.Configurations;

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable("user_logins");

        builder.HasKey(x => x.Id);

        // Composite Index for Provider + Key to ensure uniqueness across providers
        builder.HasIndex(x => new { x.LoginProvider, x.ProviderKey }).IsUnique();

        builder.Property(x => x.LoginProvider).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ProviderKey).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProviderDisplayName).HasMaxLength(200);
    }
}
