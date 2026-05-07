namespace Bloom.Domain.WorkoutTemplates.DomainEvents;

public class WorkoutTemplateDeleted(
    WorkoutTemplateId workoutTemplateId
): WorkoutTemplateDomainEvent(nameof(WorkoutTemplateDeleted))
{
    public string WorkoutTemplateId = workoutTemplateId.Value.ToString();
}