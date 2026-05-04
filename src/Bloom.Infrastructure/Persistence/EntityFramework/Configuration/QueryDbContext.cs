using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

public class QueryDbContext: DbContext
{
    protected QueryDbContext()
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder
        base.OnModelCreating(modelBuilder);
    }
}