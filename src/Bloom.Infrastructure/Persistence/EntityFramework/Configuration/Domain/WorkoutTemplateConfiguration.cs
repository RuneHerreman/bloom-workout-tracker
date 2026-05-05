using Bloom.Domain.WorkoutTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class WorkoutTemplateConfiguration: IEntityTypeConfiguration<WorkoutTemplate>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplate> builder)
    {
        builder.ToTable("WorkoutTemplates");
        
        builder.HasKey(wt => wt.Id);
        builder.Property(wt => wt.Id).ValueGeneratedNever();
        
        builder.Property(wt => wt.UserId).IsRequired();
        builder.Property(wt => wt.Name).IsRequired();

        builder.ComplexCollection(wt => wt.TemplateExercises, teBuilder =>
        {
            teBuilder.ToJson();
            teBuilder.Property(te => te.ExerciseId).IsRequired();
            teBuilder.Ignore(te => te.PlannedSets);

            teBuilder.ComplexCollection(te => te.PlannedStrengthSets, psBuilder =>
            {
                psBuilder.Property(ps => ps.Id).IsRequired();
                psBuilder.Property(ps => ps.Reps).IsRequired();
            });

            teBuilder.ComplexCollection(te => te.PlannedCardioSets, peBuilder =>
            {
                peBuilder.Property(pe => pe.Id).IsRequired();
                peBuilder.ComplexProperty(pe => pe.Distance, dBuilder =>
                {
                    dBuilder.Property(d => d.Value).IsRequired();
                    dBuilder.Property(d => d.Unit).IsRequired();
                }).IsRequired();
                peBuilder.Property(pe => pe.Duration).IsRequired();
            });
        });
    }
}