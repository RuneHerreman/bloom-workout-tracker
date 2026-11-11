using Bloom.Domain.Body;
using Bloom.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class BodyMeasurementConfiguration : IEntityTypeConfiguration<BodyMeasurement>
{
    public void Configure(EntityTypeBuilder<BodyMeasurement> builder)
    {
        builder.ToTable("BodyMeasurements");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.Date).IsRequired();
        builder.Property(m => m.WeightKg).HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(m => m.BodyFatPct).HasColumnType("numeric(4,1)").IsRequired(false);
        builder.Property(m => m.Source).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();

        // One entry per user per day; also the index every read goes through.
        builder.HasIndex(m => new { m.UserId, m.Date }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
