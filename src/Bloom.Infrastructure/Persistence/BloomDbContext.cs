using Bloom.Domain.Entity;
using Bloom.Domain.Entity.Logs;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence;

public class BloomDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<BodyMetric> BodyMetrics { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<TemplateExerciseSet> TemplateExerciseSets { get; set; } = null!;
    public DbSet<WorkoutTemplate> WorkoutTemplates { get; set; } = null!;
    public DbSet<WorkoutTemplateExercise> WorkoutTemplateExercises { get; set; } = null!;
    public DbSet<LoggedWorkout> LoggedWorkouts { get; set; } = null!;
    public DbSet<LoggedExercise> LoggedExercises { get; set; } = null!;
    public DbSet<LoggedSet> LoggedSets { get; set; } = null!;
    
    public BloomDbContext(DbContextOptions<BloomDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity => // USER
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Height).HasPrecision(5, 2);
            entity.Property(u => u.Weight).HasPrecision(5, 2);        
        });
        
        modelBuilder.Entity<BodyMetric>(entity => // BODY METRIC
        {
            entity.HasKey(bm => bm.Id);
            
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(bm => bm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(bm => bm.Weight).HasPrecision(5, 2);
            entity.Property(bm => bm.BodyFatPercentage).HasPrecision(5, 2);
        });
        
        modelBuilder.Entity<Exercise>(entity => // EXERCISE
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
        
        modelBuilder.Entity<WorkoutTemplate>(entity => // WORKOUT TEMPLATE
        {
            entity.HasKey(wt => wt.Id);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(wt => wt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(wt => wt.Name).HasMaxLength(100);
            
            entity.HasMany(wt => wt.Exercises)
                .WithOne()
                .HasForeignKey(wte => wte.WorkoutTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<WorkoutTemplateExercise>(entity => // WORKOUT TEMPLATE EXERCISE
        {
            entity.HasKey(wte => wte.Id);
            entity.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(wte => wte.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<TemplateExerciseSet>(entity => // TEMPLATE EXERCISE SET (abstract handled by EF)
        {
            entity.HasKey(s => s.Id);
            entity.HasOne<WorkoutTemplateExercise>()
                .WithMany()
                .HasForeignKey(s => s.WorkoutTemplateExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<LoggedWorkout>(entity => // LOGGED WORKOUT
        {
            entity.HasKey(lw => lw.Id);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(lw => lw.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(lw => lw.Volume).HasPrecision(10, 2);
        });

        modelBuilder.Entity<LoggedExercise>(entity => // LOGGED EXERCISE
        {
            entity.HasKey(le => le.Id);
            entity.HasOne<LoggedWorkout>()
                .WithMany()
                .HasForeignKey(le => le.LoggedWorkoutId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(le => le.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoggedSet>(entity => // LOGGED SET
        {
            entity.HasKey(ls => ls.Id);
            entity.HasOne<LoggedExercise>()
                .WithMany(le => le.Sets)
                .HasForeignKey(ls => ls.LoggedExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(ls => ls.Weight).HasPrecision(6, 2);
        });
    }
}