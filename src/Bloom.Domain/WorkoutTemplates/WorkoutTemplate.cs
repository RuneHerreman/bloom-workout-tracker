using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct WorkoutTemplateId(Guid Value) : IEntityId;

public class WorkoutTemplate: AggregateRoot<WorkoutTemplateId>
{
    
    
    public override void ValidateState()
    {
        throw new NotImplementedException();
    }
}