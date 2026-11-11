using Bloom.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        
        builder.Property(u => u.Username).IsRequired();
        builder.Property(u => u.Email).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.HashedPassword).IsRequired();
        builder.Property(u => u.FirstName).IsRequired();
        builder.Property(u => u.LastName).IsRequired();
        builder.Property(u => u.Weight).HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(u => u.Height).IsRequired();
        builder.Property(u => u.ActiveDays).IsRequired();
        builder.Property(u => u.BirthDate).IsRequired();
        builder.Property(u => u.TechnicalPoints);
        builder.Property(u => u.Gear).HasColumnType("jsonb");
        // No HasDefaultValue: with a bool the CLR default (false) would be treated as
        // "unset" and silently replaced by the DB default. Existing rows are backfilled
        // to true by the AddLeaderboardVisibility migration instead.
        builder.Property(u => u.ShowOnLeaderboard).IsRequired();
        builder.Property(u => u.Sex).HasConversion<string>().HasMaxLength(16).IsRequired(false);
    }
}