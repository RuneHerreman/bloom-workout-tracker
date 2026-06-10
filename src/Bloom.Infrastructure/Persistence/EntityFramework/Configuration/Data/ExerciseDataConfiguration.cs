using Bloom.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Data;

public class ExerciseDataConfiguration: IEntityTypeConfiguration<ExerciseData>
{
    public void Configure(EntityTypeBuilder<ExerciseData> builder)
    {
        builder.ToTable("Exercises");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Type).IsRequired().HasConversion<string>();
        builder.Property(e => e.OwnerUserId).IsRequired(false);
        builder.OwnsMany(e => e.TargetMuscles, tmBuilder =>
        {
            tmBuilder.ToJson();
            tmBuilder.Property(tm => tm.Value);
        });
    }
}