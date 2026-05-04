using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedWorkoutId(Guid Value) : IEntityId;

public class LoggedWorkout: AggregateRoot<LoggedWorkoutId>
{
    public override void ValidateState()
    {
        throw new NotImplementedException();
    }
}