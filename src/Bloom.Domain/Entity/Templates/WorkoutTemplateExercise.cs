namespace Bloom.Domain.Entity;

public class WorkoutTemplateExercise
{
    public Guid Id { get; set; }

    public Guid WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;

    public Guid ExerciseId { get; set; }
    public int Order { get; set; }

    public List<TemplateExerciseSet> Sets { get; set; } = new();
}
