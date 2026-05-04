using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

public class BloomDbContext : DbContext
{
    
    
    public BloomDbContext(DbContextOptions<BloomDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}