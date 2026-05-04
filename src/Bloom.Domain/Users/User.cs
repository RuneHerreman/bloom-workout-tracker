using Bloom.Domain.Shared;

namespace Bloom.Domain.Users;

public readonly record struct UserId(Guid Value) : IEntityId;

public class User: AggregateRoot<UserId>
{
    public override void ValidateState()
    {
        throw new NotImplementedException();
    }
}