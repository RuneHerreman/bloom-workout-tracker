using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedSetId(Guid Value) : IEntityId;

public abstract class LoggedSet: Entity<LoggedSetId>
{
    protected LoggedSet() { }
    protected LoggedSet(LoggedSetId id) : base(id) { }
}