namespace Bloom.Domain.Entity.Logs;

public class LoggedExercise
{
    public Guid Id { get; set; }
    public Guid LoggedWorkoutId { get; set; }
    public Guid ExerciseId { get; set; }
    public int Order { get; set; }
    
    public LoggedWorkout Workout { get; set; } = null!;
    public List<LoggedSet> Sets { get; set; } = new();
}