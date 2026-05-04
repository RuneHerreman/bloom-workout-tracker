using Bloom.Domain.LoggedWorkouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class LoggedWorkoutConfiguration: IEntityTypeConfiguration<LoggedWorkout>
{
    public void Configure(EntityTypeBuilder<LoggedWorkout> builder)
    {
        builder.ToTable("LoggedWorkouts");
        
        builder.HasKey(lw => lw.Id);
        builder.Property(lw => lw.Id).ValueGeneratedNever();

        builder.Property(lw => lw.UserId).IsRequired();
        builder.Property(lw => lw.LoggedAt).IsRequired();
        
        builder.OwnsMany(lw => lw.LoggedExercises, le => le.ToJson());
    }
}