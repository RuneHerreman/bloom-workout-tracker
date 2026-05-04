using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedSetId(Guid Value) : IEntityId;

public class LoggedSet: Entity<LoggedSetId>
{
    public override void ValidateState()
    {
        throw new NotImplementedException();
    }
}