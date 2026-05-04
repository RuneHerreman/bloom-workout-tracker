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
        
        builder.OwnsMany(wt => wt.TemplateExercises, te => te.ToJson());
    }
}