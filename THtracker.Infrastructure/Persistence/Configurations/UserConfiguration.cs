using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using THtracker.Domain.Entities;

namespace THtracker.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(150);

        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash);
        builder.Property(x => x.SecurityStamp);

        builder
            .HasMany(x => x.Logins)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.RefreshTokens)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Roles)
               .WithMany()
               .UsingEntity<UserRole>(
                   j => j.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId),
                   j => j.HasOne(ur => ur.User).WithMany().HasForeignKey(ur => ur.UserId),
                   j =>
                   {
                       j.ToTable("user_roles");
                       j.HasKey(ur => ur.Id);
                   });
    }
}
