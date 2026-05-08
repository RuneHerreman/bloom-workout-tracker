using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace UnitTests.Application.Mocks;

public sealed class StubCurrentUser(UserId userId) : ICurrentUser
{
    public UserId UserId { get; } = userId;

    public static StubCurrentUser With(UserId userId) => new(userId);
    public static StubCurrentUser With(Guid userId) => new(EntityId.New<UserId>(userId));
    public static StubCurrentUser Random() => new(EntityId.New<UserId>());
}
