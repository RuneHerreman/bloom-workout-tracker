namespace Bloom.Domain.Entity;

public class WorkoutTemplateExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutTemplateId { get; set; }
    public Guid ExerciseId { get; set; }
    public int Order { get; set; }
}