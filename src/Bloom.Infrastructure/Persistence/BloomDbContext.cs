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
            entity.Property(e => e.PrimaryMuscleGroup).HasMaxLength(100);
            
            entity.HasData(
                // STRENGTH EXERCISES
                new { Id = Guid.NewGuid(), Name = "Abductor", Description = "Machine hip abduction", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Glutes" },
                new { Id = Guid.NewGuid(), Name = "Adductor", Description = "Machine hip adduction", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Adductors" },
                new { Id = Guid.NewGuid(), Name = "Back Extension", Description = "Hyperextension machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Lower Back" },
                new { Id = Guid.NewGuid(), Name = "Bayesian Curl", Description = "Cable behind-body bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Bench Press", Description = "Barbell flat bench press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Bent Over Row", Description = "Barbell bent over row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Cable Curl", Description = "Standing cable bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Cable Fly", Description = "Cable crossover chest fly", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Cable Hammer Curl", Description = "Cable hammer bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Cable Row", Description = "Seated cable row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Calf Press", Description = "Machine calf press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Calves" },
                new { Id = Guid.NewGuid(), Name = "Calf Raise", Description = "Standing calf raise", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Calves" },
                new { Id = Guid.NewGuid(), Name = "Chest Fly", Description = "Dumbbell chest fly", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Chest Press", Description = "Machine chest press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Chest Supported Row", Description = "Dumbbell chest supported row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Converging High Machine Row", Description = "High position machine row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Cross Body Lat Pull-Around", Description = "Single arm cable lat pulldown", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Crunch", Description = "Bodyweight abdominal crunch", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = Guid.NewGuid(), Name = "Deadlift", Description = "Conventional barbell deadlift", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Hamstrings" },
                new { Id = Guid.NewGuid(), Name = "Decline Bench Press", Description = "Barbell decline bench press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Dips", Description = "Parallel bar dips", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Dumbbell Bicep Curl", Description = "Standing dumbbell curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Dumbbell Curl", Description = "Seated dumbbell bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Dumbbell Shoulder Press", Description = "Seated dumbbell shoulder press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Forearm Curl", Description = "Dumbbell forearm curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Forearms" },
                new { Id = Guid.NewGuid(), Name = "Hack Squat", Description = "Machine hack squat", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Quads" },
                new { Id = Guid.NewGuid(), Name = "Hammer Curl", Description = "Neutral grip dumbbell curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Horizontal Cable Row", Description = "Low cable row variation", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Horizontal Row", Description = "Machine horizontal row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Incline Dumbbell Curl", Description = "Incline bench bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Incline Dumbbell Press", Description = "Incline dumbbell chest press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Incline Smith Press", Description = "Smith machine incline press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Lat Pulldown", Description = "Wide grip lat pulldown", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Lateral Raise", Description = "Dumbbell side lateral raise", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Leg Curl", Description = "Seated leg curl machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Hamstrings" },
                new { Id = Guid.NewGuid(), Name = "Leg Extension", Description = "Machine leg extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Quads" },
                new { Id = Guid.NewGuid(), Name = "Leg Press", Description = "45-degree leg press machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Quads" },
                new { Id = Guid.NewGuid(), Name = "Leg Raise", Description = "Hanging leg raise", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = Guid.NewGuid(), Name = "Low Row", Description = "Low position cable row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Lower Back Extension", Description = "45-degree back extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Lower Back" },
                new { Id = Guid.NewGuid(), Name = "Lying Leg Curl", Description = "Prone lying leg curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Hamstrings" },
                new { Id = Guid.NewGuid(), Name = "Machine Crunch", Description = "Abdominal crunch machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = Guid.NewGuid(), Name = "Machine Preacher Curl", Description = "Preacher curl machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Machine Row", Description = "Seated machine row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Machine Shoulder Press", Description = "Machine shoulder press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Military Shoulder Press", Description = "Standing barbell overhead press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Overhead Dumbbell Shoulder Press", Description = "Seated dumbbell overhead press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Overhead Shoulder Press", Description = "Barbell overhead press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Overhead Tricep Extension", Description = "Cable overhead tricep extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = Guid.NewGuid(), Name = "Plank", Description = "Core isometric hold plank", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = Guid.NewGuid(), Name = "Preacher Curl", Description = "EZ-bar preacher curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = Guid.NewGuid(), Name = "Pull Up", Description = "Bodyweight wide grip pull up", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Rear Delt", Description = "Rear deltoid machine fly", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Reverse Fly", Description = "Dumbbell reverse pec deck fly", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Reverse Forearm Curl", Description = "Reverse wrist curl for extensors", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Forearms" },
                new { Id = Guid.NewGuid(), Name = "Russian Twist", Description = "Seated rotational core exercise", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = Guid.NewGuid(), Name = "Seated Cable Row", Description = "V-handle seated cable row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Seated Dip", Description = "Machine assisted dips", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Seated Machine Row", Description = "Leverage seated row machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = Guid.NewGuid(), Name = "Shoulder Press", Description = "Neutral grip shoulder press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = Guid.NewGuid(), Name = "Sit Up", Description = "Bodyweight sit up", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = Guid.NewGuid(), Name = "Smith Incline Bench Press", Description = "Smith machine incline press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Smith Incline Press", Description = "Smith incline chest press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = Guid.NewGuid(), Name = "Squat", Description = "Barbell back squat", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Quads" },
                new { Id = Guid.NewGuid(), Name = "Tredmill Incline Walk", Description = "Incline treadmill walking", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
                new { Id = Guid.NewGuid(), Name = "Tricep Extension", Description = "Overhead dumbbell tricep extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = Guid.NewGuid(), Name = "Tricep Overhead Extension", Description = "Cable rope overhead extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = Guid.NewGuid(), Name = "Tricep Pulldown", Description = "Straight bar tricep pulldown", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = Guid.NewGuid(), Name = "Tricep Pushdown", Description = "Rope tricep pushdown", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = Guid.NewGuid(), Name = "Weighted Crunch", Description = "Weighted decline crunch", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
        
            // CARDIO EXERCISES
            new { Id = Guid.NewGuid(), Name = "Running", Description = "Outdoor or treadmill running", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = Guid.NewGuid(), Name = "Jogging", Description = "Light pace endurance running", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = Guid.NewGuid(), Name = "Walking", Description = "Brisk walking pace", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = Guid.NewGuid(), Name = "Treadmill Incline Walk", Description = "Incline treadmill walking", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = Guid.NewGuid(), Name = "Cycling", Description = "Stationary bike or road cycling", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = Guid.NewGuid(), Name = "Rowing", Description = "Rowing machine cardio", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = Guid.NewGuid(), Name = "Snowboarding", Description = "Downhill snowboarding session", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = Guid.NewGuid(), Name = "Bouldering", Description = "Short intense climbing routes", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = Guid.NewGuid(), Name = "Climbing", Description = "Rope climbing or bouldering", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            
            // PLYOMETRIC EXERCISES
            new { Id = Guid.NewGuid(), Name = "Box Jump", Description = "Explosive jump onto box", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = Guid.NewGuid(), Name = "Burpee", Description = "Full body explosive burpee", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Full Body" },
            new { Id = Guid.NewGuid(), Name = "Jump Squat", Description = "Bodyweight squat with jump", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = Guid.NewGuid(), Name = "Tuck Jump", Description = "Knee tuck jump", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = Guid.NewGuid(), Name = "Depth Jump", Description = "Drop and immediate rebound jump", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = Guid.NewGuid(), Name = "Clap Pushup", Description = "Explosive pushup with hand clap", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Chest" },
            new { Id = Guid.NewGuid(), Name = "Bounding", Description = "Exaggerated running stride jumps", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = Guid.NewGuid(), Name = "Medicine Ball Slam", Description = "Overhead explosive ball slam", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Core" }
    );
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
                .WithOne(wte => wte.WorkoutTemplate)
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
                .WithMany(wte => wte.Sets)
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