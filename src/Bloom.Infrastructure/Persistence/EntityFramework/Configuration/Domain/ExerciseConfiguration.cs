using Bloom.Domain.Exercises;
using Bloom.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Type).IsRequired().HasConversion<string>();
        builder.Property(e => e.OwnerUserId).IsRequired(false);
        builder.Ignore(e => e.IsCustom);

        builder.HasIndex(e => e.OwnerUserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(e => e.TargetMuscles, tmBuilder =>
        {
            tmBuilder.ToJson();
            tmBuilder.Property(tm => tm.Value);
        });
    }
}
