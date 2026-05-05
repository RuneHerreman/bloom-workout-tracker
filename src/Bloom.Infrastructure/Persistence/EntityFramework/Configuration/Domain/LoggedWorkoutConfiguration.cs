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
            leBuilder.Ignore(le => le.Sets);
            leBuilder.ComplexCollection(le => le.StrengthSets);
            leBuilder.ComplexCollection(le => le.CardioSets);
        });
    }
}
