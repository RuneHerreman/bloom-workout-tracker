using Bloom.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence;

public class BloomDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    
    public BloomDbContext(DbContextOptions<BloomDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Height).HasPrecision(5, 2);
            entity.Property(u => u.Weight).HasPrecision(5, 2);        
        });
    }
}