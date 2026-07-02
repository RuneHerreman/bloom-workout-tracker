using Bloom.Domain.Strava;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Convertors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class StravaConnectionConfiguration(IDataProtector? tokenProtector = null)
    : IEntityTypeConfiguration<StravaConnection>
{
    public void Configure(EntityTypeBuilder<StravaConnection> builder)
    {
        builder.ToTable("StravaConnections");

        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Id).ValueGeneratedNever();

        builder.Property(sc => sc.UserId).IsRequired();
        builder.Property(sc => sc.StravaAthleteId).IsRequired();
        builder.Property(sc => sc.AccessToken).IsRequired().HasMaxLength(2048);
        builder.Property(sc => sc.RefreshToken).IsRequired().HasMaxLength(2048);

        if (tokenProtector is not null)
        {
            var converter = new ProtectedTokenConverter(tokenProtector);
            builder.Property(sc => sc.AccessToken).HasConversion(converter);
            builder.Property(sc => sc.RefreshToken).HasConversion(converter);
        }
        builder.Property(sc => sc.ExpiresAt).IsRequired();
        builder.Property(sc => sc.AthleteName).IsRequired().HasMaxLength(200);
        builder.Property(sc => sc.ConnectedAt).IsRequired();
        builder.Property(sc => sc.LastSyncedAt).IsRequired(false);

        builder.HasIndex(sc => sc.UserId).IsUnique();
        builder.HasIndex(sc => sc.StravaAthleteId).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(sc => sc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
