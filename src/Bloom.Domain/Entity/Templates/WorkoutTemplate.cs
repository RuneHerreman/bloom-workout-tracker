namespace Bloom.Domain.Entity;

public class WorkoutTemplate
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public List<WorkoutTemplateExercise> Exercises { get; set; } = new();
}