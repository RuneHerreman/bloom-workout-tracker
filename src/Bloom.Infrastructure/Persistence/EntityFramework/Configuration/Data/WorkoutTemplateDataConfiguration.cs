using Bloom.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Data;

public class WorkoutTemplateDataConfiguration : IEntityTypeConfiguration<WorkoutTemplateData>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplateData> builder)
    {
        builder.ToTable("WorkoutTemplates");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.Name).IsRequired();

        builder.OwnsMany(t => t.TemplateExercises, exercises =>
        {
            exercises.ToJson();

            exercises.Property(e => e.Id).IsRequired();
            exercises.Property(e => e.ExerciseId).IsRequired();
            exercises.Property(e => e.Order).IsRequired();

            exercises.OwnsMany(e => e.Sets, sets =>
            {
                sets.Property(s => s.Id).IsRequired();
                sets.Property(s => s.Type).HasConversion<int>().IsRequired();
                sets.Property(s => s.Order).IsRequired();

                sets.Property(s => s.Duration).IsRequired(false);
                sets.Property(s => s.Reps).IsRequired(false);

                sets.OwnsOne(s => s.Distance, d =>
                {
                    d.Property(p => p.Value);
                    d.Property(p => p.Unit).HasConversion<int>();
                });
            });
        });
    }
}