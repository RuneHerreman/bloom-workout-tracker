using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct PlannedSetId(Guid Value) : IEntityId;

public class PlannedSet: Entity<PlannedSetId>
{
    public override void ValidateState()
    {
        throw new NotImplementedException();
    }
}