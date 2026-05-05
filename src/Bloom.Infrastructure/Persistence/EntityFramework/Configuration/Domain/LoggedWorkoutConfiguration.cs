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

        builder.ComplexCollection(lw => lw.LoggedExercises, leBuilder =>
        {
            leBuilder.ToJson();
            leBuilder.Property(le => le.Id).IsRequired();
            leBuilder.Property(le => le.ExerciseId).IsRequired();
            leBuilder.Ignore(le => le.Sets);

            leBuilder.ComplexCollection(le => le.StrengthSets, ssBuilder =>
            {
                ssBuilder.Property(ss => ss.Id).IsRequired();
                
                ssBuilder.ComplexProperty(ss => ss.Weight).IsRequired();
                ssBuilder.ComplexProperty(ss => ss.Reps).IsRequired();
                ssBuilder.ComplexProperty(ss => ss.RIR).IsRequired();
            });
            
            leBuilder.ComplexCollection(le => le.CardioSets, csBuilder =>
            {
                csBuilder.Property(cs => cs.Id).IsRequired();
                
                csBuilder.ComplexProperty(cs => cs.Distance, dBuilder =>
                {
                    dBuilder.Property(d => d.Value).IsRequired();
                    dBuilder.Property(d => d.Unit).IsRequired();
                }).IsRequired();
                csBuilder.Property(cs => cs.Duration).IsRequired();
            });
        });
    }
}
