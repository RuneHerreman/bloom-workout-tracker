namespace Bloom.Domain.WorkoutTemplates.DomainEvents;

public class WorkoutTemplateCreated(
    WorkoutTemplateId workoutTemplateId
): WorkoutTemplateDomainEvent(nameof(WorkoutTemplateCreated))
{
    public string WorkoutTemplateId = workoutTemplateId.Value.ToString();
}