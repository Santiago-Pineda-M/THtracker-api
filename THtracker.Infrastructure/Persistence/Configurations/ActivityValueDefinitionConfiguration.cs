using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using THtracker.Domain.Entities;

namespace THtracker.Infrastructure.Persistence.Configurations;

public class ActivityValueDefinitionConfiguration
    : IEntityTypeConfiguration<ActivityValueDefinition>
{
    public void Configure(EntityTypeBuilder<ActivityValueDefinition> builder)
    {
        builder.ToTable("activity_value_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActivityId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ValueType).IsRequired();
        builder.Property(x => x.IsRequired).IsRequired();
        builder.Property(x => x.Unit);
        builder.Property(x => x.MinValue);
        builder.Property(x => x.MaxValue);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
