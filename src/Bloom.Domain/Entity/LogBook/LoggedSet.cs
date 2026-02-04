namespace Bloom.Domain.Entity.Logs;

public class LoggedSet : ExerciseSet
{
    public Guid LoggedExerciseId { get; set; }
    public int? Weight { get; set; }
    
    public decimal CalculateVolume() => Weight * Reps ?? 0;
}