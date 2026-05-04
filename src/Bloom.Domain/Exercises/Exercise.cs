using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises;

public readonly record struct ExerciseId(Guid Value) : IEntityId;

public class Exercise: AggregateRoot<ExerciseId>
{
    public override void ValidateState()
    {
        throw new NotImplementedException();
    }
}