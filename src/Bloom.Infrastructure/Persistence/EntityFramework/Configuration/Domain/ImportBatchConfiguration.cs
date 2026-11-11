using Bloom.Domain.Imports;
using Bloom.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.FileName).IsRequired().HasMaxLength(ImportBatch.MaxFileNameLength);
        builder.Property(b => b.ImportedAt).IsRequired();
        builder.Property(b => b.UndoneAt).IsRequired(false);
        builder.Ignore(b => b.IsUndone);

        // Stored as uuid[] columns: the ids are only read back to undo the import.
        builder.PrimitiveCollection(b => b.WorkoutIds).IsRequired();
        builder.PrimitiveCollection(b => b.ExerciseIds).IsRequired();
        builder.PrimitiveCollection(b => b.TemplateIds).IsRequired();

        builder.HasIndex(b => b.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
