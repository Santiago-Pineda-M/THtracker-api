using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using THtracker.Domain.Entities;

namespace THtracker.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");
        builder.HasKey(x => x.Id);
        
        builder.HasOne<Activity>()
               .WithMany()
               .HasForeignKey(x => x.ActivityId)
               .OnDelete(DeleteBehavior.Cascade);
               
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.EndedAt);
    }
}
