using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.Users;

public sealed class GetCurrentUserTests
{
    private static UserData SampleUser(Guid id) => new()
    {
        Id = id,
        Email = "alice@example.com",
        Username = "alice",
        FirstName = "Alice",
        LastName = "Smith",
        Weight = 70m,
        Height = 175,
        ActiveDays = 4
    };

    [Fact]
    public async Task Execute_WhenUserExists_ShouldReturnCurrentUser()
    {
        var userId = Guid.NewGuid();
        var useCase = new GetCurrentUser(
            StubCurrentUser.With(userId),
            new MockFindUsersQuery([SampleUser(userId)])
        );

        var output = await useCase.Execute(new GetCurrentUserInput());

        Assert.Equal(userId, output.User.Id);
        Assert.Equal("alice@example.com", output.User.Email);
    }

    [Fact]
    public async Task Execute_WhenUserNotFound_ShouldThrowUserNotFoundException()
    {
        var useCase = new GetCurrentUser(
            StubCurrentUser.Random(),
            new MockFindUsersQuery([])
        );

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => useCase.Execute(new GetCurrentUserInput()));
    }
}

public sealed class MockFindUsersQuery(IEnumerable<UserData> data) : IFindUsersQuery
{
    private readonly List<UserData> _data = data.ToList();

    public Task<IReadOnlyList<UserData>> Fetch(Expression<Func<UserData, bool>> filter, CancellationToken ct = default)
    {
        IReadOnlyList<UserData> filtered = _data.AsQueryable().Where(filter).ToList();
        return Task.FromResult(filtered);
    }
}
