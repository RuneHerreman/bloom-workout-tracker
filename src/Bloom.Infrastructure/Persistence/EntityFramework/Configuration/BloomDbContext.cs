using Bloom.Domain.Exercises;
using Bloom.Domain.LogBook;
using Bloom.Domain.Shared;
using Bloom.Domain.Templates;
using Bloom.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence;

public class BloomDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<BodyMetric> BodyMetrics { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<ExerciseSet> ExerciseSets { get; set; } = null!;
    public DbSet<WorkoutTemplate> WorkoutTemplates { get; set; } = null!;
    public DbSet<WorkoutTemplateExercise> WorkoutTemplateExercises { get; set; } = null!;
    public DbSet<LoggedWorkout> LoggedWorkouts { get; set; } = null!;
    public DbSet<LoggedExercise> LoggedExercises { get; set; } = null!;
    
    public BloomDbContext(DbContextOptions<BloomDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity => // USER
        {
            entity.HasKey(u => u.Id);
            entity.Property(e => e.Id).HasConversion(id => id.Value, value => new UserId(value));
            
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Height).HasPrecision(5, 2);
            entity.Property(u => u.Weight).HasPrecision(5, 2);        
        });
        
        modelBuilder.Entity<BodyMetric>(entity => // BODY METRIC
        {
            entity.HasKey(bm => bm.Id);
            entity.Property(e => e.Id).HasConversion(id => id.Value, d => new BodyMetricId(d));
            entity.Property(bm => bm.UserId).HasConversion(id => id.Value, d => new UserId(d));
            
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
            entity.Property(e => e.Id).HasConversion(id => id.Value, d => new ExerciseId(d));
            
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PrimaryMuscleGroup).HasMaxLength(100);
            
            entity.HasData(
                // STRENGTH EXERCISES
                new { Id = EntityId.New<ExerciseId>(), Name = "Abductor", Description = "Machine hip abduction", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Glutes" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Adductor", Description = "Machine hip adduction", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Adductors" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Back Extension", Description = "Hyperextension machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Lower Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Bayesian Curl", Description = "Cable behind-body bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Bench Press", Description = "Barbell flat bench press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Bent Over Row", Description = "Barbell bent over row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Cable Curl", Description = "Standing cable bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Cable Fly", Description = "Cable crossover chest fly", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Cable Hammer Curl", Description = "Cable hammer bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Cable Row", Description = "Seated cable row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Calf Press", Description = "Machine calf press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Calves" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Calf Raise", Description = "Standing calf raise", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Calves" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Chest Fly", Description = "Dumbbell chest fly", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Chest Press", Description = "Machine chest press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Chest Supported Row", Description = "Dumbbell chest supported row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Converging High Machine Row", Description = "High position machine row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Cross Body Lat Pull-Around", Description = "Single arm cable lat pulldown", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Crunch", Description = "Bodyweight abdominal crunch", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Deadlift", Description = "Conventional barbell deadlift", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Hamstrings" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Decline Bench Press", Description = "Barbell decline bench press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Dips", Description = "Parallel bar dips", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Dumbbell Bicep Curl", Description = "Standing dumbbell curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Dumbbell Curl", Description = "Seated dumbbell bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Dumbbell Shoulder Press", Description = "Seated dumbbell shoulder press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Forearm Curl", Description = "Dumbbell forearm curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Forearms" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Hack Squat", Description = "Machine hack squat", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Quads" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Hammer Curl", Description = "Neutral grip dumbbell curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Horizontal Cable Row", Description = "Low cable row variation", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Horizontal Row", Description = "Machine horizontal row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Incline Dumbbell Curl", Description = "Incline bench bicep curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Incline Dumbbell Press", Description = "Incline dumbbell chest press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Incline Smith Press", Description = "Smith machine incline press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Lat Pulldown", Description = "Wide grip lat pulldown", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Lateral Raise", Description = "Dumbbell side lateral raise", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Leg Curl", Description = "Seated leg curl machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Hamstrings" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Leg Extension", Description = "Machine leg extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Quads" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Leg Press", Description = "45-degree leg press machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Quads" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Leg Raise", Description = "Hanging leg raise", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Low Row", Description = "Low position cable row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Lower Back Extension", Description = "45-degree back extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Lower Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Lying Leg Curl", Description = "Prone lying leg curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Hamstrings" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Machine Crunch", Description = "Abdominal crunch machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Machine Preacher Curl", Description = "Preacher curl machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Machine Row", Description = "Seated machine row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Machine Shoulder Press", Description = "Machine shoulder press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Military Shoulder Press", Description = "Standing barbell overhead press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Overhead Dumbbell Shoulder Press", Description = "Seated dumbbell overhead press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Overhead Shoulder Press", Description = "Barbell overhead press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Overhead Tricep Extension", Description = "Cable overhead tricep extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Plank", Description = "Core isometric hold plank", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Preacher Curl", Description = "EZ-bar preacher curl", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Biceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Pull Up", Description = "Bodyweight wide grip pull up", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Rear Delt", Description = "Rear deltoid machine fly", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Reverse Fly", Description = "Dumbbell reverse pec deck fly", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Reverse Forearm Curl", Description = "Reverse wrist curl for extensors", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Forearms" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Russian Twist", Description = "Seated rotational core exercise", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Seated Cable Row", Description = "V-handle seated cable row", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Seated Dip", Description = "Machine assisted dips", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Seated Machine Row", Description = "Leverage seated row machine", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Back" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Shoulder Press", Description = "Neutral grip shoulder press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Shoulders" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Sit Up", Description = "Bodyweight sit up", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Smith Incline Bench Press", Description = "Smith machine incline press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Smith Incline Press", Description = "Smith incline chest press", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Chest" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Squat", Description = "Barbell back squat", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Quads" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Tredmill Incline Walk", Description = "Incline treadmill walking", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Tricep Extension", Description = "Overhead dumbbell tricep extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Tricep Overhead Extension", Description = "Cable rope overhead extension", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Tricep Pulldown", Description = "Straight bar tricep pulldown", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Tricep Pushdown", Description = "Rope tricep pushdown", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Triceps" },
                new { Id = EntityId.New<ExerciseId>(), Name = "Weighted Crunch", Description = "Weighted decline crunch", Type = ExerciseType.Strength, PrimaryMuscleGroup = "Core" },
        
            // CARDIO EXERCISES
            new { Id = EntityId.New<ExerciseId>(), Name = "Running", Description = "Outdoor or treadmill running", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Jogging", Description = "Light pace endurance running", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Walking", Description = "Brisk walking pace", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Treadmill Incline Walk", Description = "Incline treadmill walking", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Cycling", Description = "Stationary bike or road cycling", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Rowing", Description = "Rowing machine cardio", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Snowboarding", Description = "Downhill snowboarding session", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Bouldering", Description = "Short intense climbing routes", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Climbing", Description = "Rope climbing or bouldering", Type = ExerciseType.Cardio, PrimaryMuscleGroup = "Cardio" },
            
            // PLYOMETRIC EXERCISES
            new { Id = EntityId.New<ExerciseId>(), Name = "Box Jump", Description = "Explosive jump onto box", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Burpee", Description = "Full body explosive burpee", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Full Body" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Jump Squat", Description = "Bodyweight squat with jump", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Tuck Jump", Description = "Knee tuck jump", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Depth Jump", Description = "Drop and immediate rebound jump", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Clap Pushup", Description = "Explosive pushup with hand clap", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Chest" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Bounding", Description = "Exaggerated running stride jumps", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Quads" },
            new { Id = EntityId.New<ExerciseId>(), Name = "Medicine Ball Slam", Description = "Overhead explosive ball slam", Type = ExerciseType.Plyometric, PrimaryMuscleGroup = "Core" }
    );
        });
        
        modelBuilder.Entity<WorkoutTemplate>(entity => // WORKOUT TEMPLATE
        {
            entity.HasKey(wt => wt.Id);
            entity.Property(e => e.Id).HasConversion(id => id.Value, d => new WorkoutTemplateId(d));
            entity.Property(e => e.UserId).HasConversion(id => id.Value, d => new UserId(d));
            
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
            entity.Property(e => e.Id).HasConversion(id => id.Value, d => new WorkoutTemplateExerciseId(d));
            entity.Property(e => e.WorkoutTemplateId).HasConversion(id => id.Value, d => new WorkoutTemplateId(d));
            entity.Property(e => e.ExerciseId).HasConversion(id => id.Value, d => new ExerciseId(d));

            entity.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(wte => wte.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<ExerciseSet>(entity => // EXERCISE SET (TPH)
        {
            entity.HasKey(s => s.Id);
            entity.Property(e => e.Id).HasConversion(id => id.Value, d => new ExerciseSetId(d));
        });

        modelBuilder.Entity<LoggedStrengthSet>(entity =>
        {
            entity.Property(ls => ls.LoggedExerciseId).HasConversion(id => id.Value, d => new LoggedExerciseId(d));
            entity.Property(ls => ls.Weight).HasPrecision(6, 2);

            entity.HasOne<LoggedExercise>()
                .WithMany(le => le.StrengthSets)
                .HasForeignKey(ls => ls.LoggedExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoggedCardioSet>(entity =>
        {
            entity.Property(ls => ls.LoggedExerciseId).HasConversion(id => id.Value, d => new LoggedExerciseId(d));

            entity.HasOne<LoggedExercise>()
                .WithMany(le => le.CardioSets)
                .HasForeignKey(ls => ls.LoggedExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateStrengthSet>(entity =>
        {
            entity.Property(ls => ls.WorkoutTemplateExerciseId).HasConversion(id => id.Value, d => new WorkoutTemplateExerciseId(d));

            entity.HasOne<WorkoutTemplateExercise>()
                .WithMany(wte => wte.StrengthSets)
                .HasForeignKey(ls => ls.WorkoutTemplateExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateCardioSet>(entity =>
        {
            entity.Property(ls => ls.WorkoutTemplateExerciseId).HasConversion(id => id.Value, d => new WorkoutTemplateExerciseId(d));

            entity.HasOne<WorkoutTemplateExercise>()
                .WithMany(wte => wte.CardioSets)
                .HasForeignKey(ls => ls.WorkoutTemplateExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<LoggedWorkout>(entity => // LOGGED WORKOUT
        {
            entity.HasKey(lw => lw.Id);
            entity.Property(e => e.Id).HasConversion(id => id.Value, d => new LoggedWorkoutId(d));
            entity.Property(e => e.UserId).HasConversion(id => id.Value, d => new UserId(d));
            entity.Ignore(lw => lw.TotalVolume);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(lw => lw.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(lw => lw.Exercises)
                .WithOne()
                .HasForeignKey(le => le.LoggedWorkoutId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoggedExercise>(entity => // LOGGED EXERCISE
        {
            entity.HasKey(le => le.Id);
            entity.Property(e => e.Id).HasConversion(id => id.Value, d => new LoggedExerciseId(d));
            entity.Property(e => e.LoggedWorkoutId).HasConversion(id => id.Value, d => new LoggedWorkoutId(d));
            entity.Property(e => e.ExerciseId).HasConversion(id => id.Value, d => new ExerciseId(d));

            entity.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(le => le.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}