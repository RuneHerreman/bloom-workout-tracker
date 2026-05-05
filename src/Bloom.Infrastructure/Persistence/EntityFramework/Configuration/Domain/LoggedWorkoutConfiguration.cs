using Bloom.Domain.LoggedWorkouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class LoggedWorkoutConfiguration : IEntityTypeConfiguration<LoggedWorkout>
{
    public void Configure(EntityTypeBuilder<LoggedWorkout> builder)
    {
        builder.ToTable("LoggedWorkouts");

        builder.HasKey(lw => lw.Id);
        builder.Property(lw => lw.Id).ValueGeneratedNever();

        builder.Property(lw => lw.UserId).IsRequired();
        builder.Property(lw => lw.LoggedAt).IsRequired();

        builder.OwnsMany(lw => lw.LoggedExercises, leBuilder =>
        {
            leBuilder.ToJson();

            leBuilder.Property(le => le.Id).IsRequired();
            leBuilder.Property(le => le.ExerciseId).IsRequired();

            leBuilder.OwnsMany(le => le.Sets, sets =>
            {
                sets.Property(s => s.Id).IsRequired();
                sets.Property(s => s.Type).HasConversion<string>().IsRequired();

                sets.Property(x => x.Duration).IsRequired(false);
                sets.Property(x => x.Reps).IsRequired(false);    
                sets.Property(x => x.Rir).IsRequired(false);     

                sets.OwnsOne(x => x.Distance, d =>
                {
                    d.Property(p => p.Value);
                    d.Property(p => p.Unit).HasConversion<string>();
                });

                sets.OwnsOne(x => x.Weight, w =>
                {
                    w.Property(p => p.Value);
                    w.Property(p => p.Unit).HasConversion<string>();
                });

            });
        });
    }
}
