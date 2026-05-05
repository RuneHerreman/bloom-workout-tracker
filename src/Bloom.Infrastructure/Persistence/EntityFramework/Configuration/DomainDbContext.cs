using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared.DomainEvents;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Domain;
using Howestprime.Movies.Infrastructure.Persistence.EntityFramework.Configuration.Converters;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

public abstract class DomainDbContext : DbContext
{
    private readonly Queue<IDomainEvent> _queuedDomainEvents = new();
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<LoggedWorkout> LoggedWorkouts { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<WorkoutTemplate> WorkoutTemplates { get; set; }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        EntityIdConverter.AddConventions(configurationBuilder);
        SinglePropertyValueObjectConverter.AddConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new ExerciseConfiguration())
            .ApplyConfiguration(new LoggedWorkoutConfiguration())
            .ApplyConfiguration(new WorkoutTemplateConfiguration())
            .ApplyConfiguration(new UserConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }

    public void QueueDomainEvent(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
            _queuedDomainEvents.Enqueue(domainEvent);
    }

    public IReadOnlyCollection<IDomainEvent> GetQueuedDomainEvents()
    {
        return _queuedDomainEvents.ToList().AsReadOnly();
    }

    public void ClearQueuedDomainEvents()
    {
        _queuedDomainEvents.Clear();
    }
}
