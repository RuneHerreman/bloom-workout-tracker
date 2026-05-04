using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedExerciseId(Guid Value) : IEntityId;

public class LoggedExercise: Entity<LoggedExerciseId>
{
    public override void ValidateState()
    {
        throw new NotImplementedException();
    }
}