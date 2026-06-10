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
        builder.Property(u => u.HashedPassword).IsRequired();
        builder.Property(u => u.FirstName).IsRequired();
        builder.Property(u => u.LastName).IsRequired();
        builder.Property(u => u.Weight).HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(u => u.Height).IsRequired();
        builder.Property(u => u.ActiveDays).IsRequired();
        builder.Property(u => u.BirthDate).IsRequired();
        builder.Property(u => u.TechnicalPoints);
        builder.Property(u => u.Gear).HasColumnType("jsonb");
    }
}