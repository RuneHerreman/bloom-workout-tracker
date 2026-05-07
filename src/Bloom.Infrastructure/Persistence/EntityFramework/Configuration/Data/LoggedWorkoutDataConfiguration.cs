using Bloom.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Data;

public class LoggedWorkoutDataConfiguration : IEntityTypeConfiguration<LoggedWorkoutData>
{
    public void Configure(EntityTypeBuilder<LoggedWorkoutData> builder)
    {
        builder.ToTable("LoggedWorkouts");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.UserId).IsRequired();
        builder.Property(l => l.LoggedAt).IsRequired();

        builder.OwnsMany(l => l.LoggedExercises, exercises =>
        {
            exercises.ToJson();

            exercises.Property(e => e.Id).IsRequired();
            exercises.Property(e => e.ExerciseId).IsRequired();
            exercises.Property(e => e.Order).IsRequired();

            exercises.OwnsMany(e => e.Sets, sets =>
            {
                sets.Property(s => s.Id).IsRequired();
                sets.Property(s => s.Type).HasConversion<string>().IsRequired();
                sets.Property(s => s.Order).IsRequired();

                sets.Property(s => s.Duration).IsRequired(false);
                sets.Property(s => s.Reps).IsRequired(false);
                sets.Property(s => s.Rir).IsRequired(false);

                sets.OwnsOne(s => s.Distance, d =>
                {
                    d.Property(p => p.Value);
                    d.Property(p => p.Unit).HasConversion<string>();
                });

                sets.OwnsOne(s => s.Weight, w =>
                {
                    w.Property(p => p.Value);
                    w.Property(p => p.Unit).HasConversion<string>();
                });
            });
        });
    }
}