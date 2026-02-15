using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using THtracker.Domain.Entities;

namespace THtracker.Infrastructure.Persistence.Configurations;

public class ActivityLogValueConfiguration : IEntityTypeConfiguration<ActivityLogValue>
{
    public void Configure(EntityTypeBuilder<ActivityLogValue> builder)
    {
        builder.ToTable("activity_log_values");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActivityLogId).IsRequired();
        builder.Property(x => x.ValueDefinitionId).IsRequired();
        builder.Property(x => x.Value).IsRequired();
        builder.HasOne<ActivityLog>()
            .WithMany(l => l.LogValues)
            .HasForeignKey(x => x.ActivityLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ValueDefinition)
            .WithMany()
            .HasForeignKey(x => x.ValueDefinitionId);
    }
}
