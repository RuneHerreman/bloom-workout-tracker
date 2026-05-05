using Bloom.Application.Contracts;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

public class QueryDbContext: DbContext
{
    public DbSet<ExerciseData> Exercises { get; set; }
    
    
    protected QueryDbContext()
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new ExerciseDataConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}