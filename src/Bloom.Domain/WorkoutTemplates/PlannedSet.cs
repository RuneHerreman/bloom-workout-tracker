using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct PlannedSetId(Guid Value) : IEntityId;

public abstract class PlannedSet: Entity<PlannedSetId>
{
    
}