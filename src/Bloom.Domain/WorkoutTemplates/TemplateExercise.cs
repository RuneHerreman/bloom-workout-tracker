using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct TemplateExerciseId(Guid Value) : IEntityId;

public class TemplateExercise: Entity<TemplateExerciseId>
{
    public override void ValidateState()
    {
        throw new NotImplementedException();
    }
}