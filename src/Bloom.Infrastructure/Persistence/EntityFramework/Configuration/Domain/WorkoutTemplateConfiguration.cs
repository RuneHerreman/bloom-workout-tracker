using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class WorkoutTemplateConfiguration: IEntityTypeConfiguration<WorkoutTemplate>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplate> builder)
    {
        builder.ToTable("WorkoutTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Name).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(x => x.TemplateExercises, exercises =>
        {
            exercises.ToJson();

            exercises.Property(x => x.Id).IsRequired();
            exercises.Property(x => x.ExerciseId).IsRequired();
            exercises.Property(x => x.Order).IsRequired();
            exercises.Property(x => x.Note).IsRequired(false);
            exercises.Property(x => x.Gear);

            exercises.OwnsMany(x => x.Sets, sets =>
            {
                sets.Property(x => x.Id).IsRequired();
                sets.Property(x => x.Type).HasConversion<int>().IsRequired();
                sets.Property(x => x.Order).IsRequired();

                sets.Property(x => x.Duration).IsRequired(false);
                sets.Property(x => x.Reps).IsRequired(false);

                // Multi-property VO => owned
                sets.OwnsOne(x => x.Distance, d =>
                {
                    d.Property(p => p.Value);
                    d.Property(p => p.Unit).HasConversion<int>();
                });
            });
        });

        builder.Navigation(x => x.TemplateExercises)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}