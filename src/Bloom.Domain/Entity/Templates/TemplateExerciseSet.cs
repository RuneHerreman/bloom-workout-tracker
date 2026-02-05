namespace Bloom.Domain.Entity;

public class TemplateExerciseSet : ExerciseSet
{
    public Guid WorkoutTemplateExerciseId { get; set; }
    public WorkoutTemplateExercise WorkoutTemplateExercise { get; set; } = null!;}