namespace Bloom.Domain.WorkoutTemplates.DomainEvents;

public class WorkoutTemplateUpdated(
    WorkoutTemplateId workoutTemplateId
): WorkoutTemplateDomainEvent(nameof(WorkoutTemplateUpdated))
{
    public string WorkoutTemplateId = workoutTemplateId.Value.ToString();
}