using Bloom.Application.Body;
using Bloom.Application.Users;
using Bloom.Domain.Body;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Body;

public sealed class BodyLogTests : ApplicationTestBase
{
    private static readonly DateOnly Mon = new(2026, 11, 2);
    private static readonly DateOnly Wed = new(2026, 11, 4);
    private static readonly DateOnly Fri = new(2026, 11, 6);

    private async Task<User> SeedUser(decimal weight = 80m)
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", weight, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return user;
    }

    private LogBodyMeasurement Log(User user) => new(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<LogBodyMeasurement>());
    private DeleteBodyMeasurement Delete(User user) => new(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<DeleteBodyMeasurement>());
    private ListBodyMeasurements List(User user) => new(UnitOfWork, StubCurrentUser.With(user.Id));

    // ── Log ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Log_NewDay_ShouldCreateAnEntryAndUpdateCurrentWeight()
    {
        var user = await SeedUser();

        var output = await Log(user).Execute(new LogBodyMeasurementInput(Mon, 78.4m, 18.2m));

        Assert.True(output.Created);
        Assert.Equal(new BodyMeasurementData(Mon, 78.4m, 18.2m, "Manual"), output.Entry);
        Assert.Equal(78.4m, (await UserRepository.ById(user.Id)).Value.Weight);
    }

    [Fact]
    public async Task Log_SameDayAgain_ShouldUpdateTheEntry_KeepingBodyFat()
    {
        var user = await SeedUser();
        await Log(user).Execute(new LogBodyMeasurementInput(Mon, 78.4m, 18.2m));

        var output = await Log(user).Execute(new LogBodyMeasurementInput(Mon, 78.1m));

        Assert.False(output.Created);
        Assert.Equal(78.1m, output.Entry.WeightKg);
        Assert.Equal(18.2m, output.Entry.BodyFatPct);
        Assert.Single(await BodyMeasurementRepository.ForUser(user.Id));
    }

    [Fact]
    public async Task Log_WithClearBodyFat_ShouldRemoveTheReading()
    {
        var user = await SeedUser();
        await Log(user).Execute(new LogBodyMeasurementInput(Mon, 78.4m, 18.2m));

        var output = await Log(user).Execute(new LogBodyMeasurementInput(Mon, 78.4m, ClearBodyFat: true));

        Assert.Null(output.Entry.BodyFatPct);
    }

    [Fact]
    public async Task Log_AnEarlierDay_ShouldNotChangeCurrentWeight()
    {
        var user = await SeedUser();
        await Log(user).Execute(new LogBodyMeasurementInput(Fri, 77m));

        await Log(user).Execute(new LogBodyMeasurementInput(Mon, 79m));

        Assert.Equal(77m, (await UserRepository.ById(user.Id)).Value.Weight);
    }

    [Theory]
    [InlineData(19.9, null)]
    [InlineData(401, null)]
    [InlineData(80, 2.0)]
    [InlineData(80, 71.0)]
    public async Task Log_WithValuesOutOfRange_ShouldBeRejected(decimal kg, double? bodyFat)
    {
        var user = await SeedUser();

        await Assert.ThrowsAsync<InvalidBodyMeasurementException>(() =>
            Log(user).Execute(new LogBodyMeasurementInput(Mon, kg, bodyFat is null ? null : (decimal)bodyFat)));
        Assert.Empty(await BodyMeasurementRepository.ForUser(user.Id));
    }

    [Fact]
    public async Task Log_InTheFuture_ShouldBeRejected()
    {
        var user = await SeedUser();
        var future = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(3);

        await Assert.ThrowsAsync<InvalidBodyMeasurementException>(() => Log(user).Execute(new LogBodyMeasurementInput(future, 80m)));
    }

    [Fact]
    public async Task Log_Tomorrow_ShouldBeAllowed_ForTimeZonesAheadOfUtc()
    {
        var user = await SeedUser();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var output = await Log(user).Execute(new LogBodyMeasurementInput(tomorrow, 80m));

        Assert.True(output.Created);
    }

    [Fact]
    public async Task Log_ForMissingUser_ShouldThrow()
    {
        var log = new LogBodyMeasurement(UnitOfWork, StubCurrentUser.Random(), CreateLogger<LogBodyMeasurement>());

        await Assert.ThrowsAsync<UserNotFoundException>(() => log.Execute(new LogBodyMeasurementInput(Mon, 80m)));
    }

    // ── List ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_ShouldReturnOnlyYourEntries_OldestFirst_WithinRange()
    {
        var user = await SeedUser();
        var other = User.Create("bob@example.com", "bob", "hash", "Bob", "Jones", 90m, 185, 3, new DateOnly(1988, 5, 5));
        await UserRepository.Save(other);
        await Log(user).Execute(new LogBodyMeasurementInput(Fri, 77m));
        await Log(user).Execute(new LogBodyMeasurementInput(Mon, 79m));
        await Log(user).Execute(new LogBodyMeasurementInput(Wed, 78m));
        await Log(other).Execute(new LogBodyMeasurementInput(Wed, 90m));

        var all = await List(user).Execute(new ListBodyMeasurementsInput());
        var fromWed = await List(user).Execute(new ListBodyMeasurementsInput(From: Wed));

        Assert.Equal([Mon, Wed, Fri], all.Entries.Select(e => e.Date));
        Assert.Equal([Wed, Fri], fromWed.Entries.Select(e => e.Date));
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_TheLatestDay_ShouldFallBackToThePreviousWeight()
    {
        var user = await SeedUser();
        await Log(user).Execute(new LogBodyMeasurementInput(Mon, 79m));
        await Log(user).Execute(new LogBodyMeasurementInput(Fri, 77m));

        await Delete(user).Execute(new DeleteBodyMeasurementInput(Fri));

        Assert.Equal(79m, (await UserRepository.ById(user.Id)).Value.Weight);
        Assert.Equal([Mon], (await BodyMeasurementRepository.ForUser(user.Id)).Select(m => m.Date));
    }

    [Fact]
    public async Task Delete_AnEarlierDay_ShouldKeepCurrentWeight()
    {
        var user = await SeedUser();
        await Log(user).Execute(new LogBodyMeasurementInput(Mon, 79m));
        await Log(user).Execute(new LogBodyMeasurementInput(Fri, 77m));

        await Delete(user).Execute(new DeleteBodyMeasurementInput(Mon));

        Assert.Equal(77m, (await UserRepository.ById(user.Id)).Value.Weight);
    }

    [Fact]
    public async Task Delete_TheOnlyEntry_ShouldKeepCurrentWeight()
    {
        var user = await SeedUser();
        await Log(user).Execute(new LogBodyMeasurementInput(Mon, 79m));

        await Delete(user).Execute(new DeleteBodyMeasurementInput(Mon));

        Assert.Equal(79m, (await UserRepository.ById(user.Id)).Value.Weight);
        Assert.Empty(await BodyMeasurementRepository.ForUser(user.Id));
    }

    [Fact]
    public async Task Delete_AMissingDay_ShouldBeNotFound()
    {
        var user = await SeedUser();

        await Assert.ThrowsAsync<BodyMeasurementNotFoundException>(() => Delete(user).Execute(new DeleteBodyMeasurementInput(Mon)));
    }

    [Fact]
    public async Task Delete_AnotherUsersDay_ShouldBeNotFound()
    {
        var user = await SeedUser();
        await Log(user).Execute(new LogBodyMeasurementInput(Mon, 79m));
        var stranger = new DeleteBodyMeasurement(UnitOfWork, StubCurrentUser.Random(), CreateLogger<DeleteBodyMeasurement>());

        await Assert.ThrowsAsync<BodyMeasurementNotFoundException>(() => stranger.Execute(new DeleteBodyMeasurementInput(Mon)));
        Assert.Single(await BodyMeasurementRepository.ForUser(user.Id));
    }

    // ── Profile weight ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUserInfo_WithNewWeight_ShouldLogToday()
    {
        var user = await SeedUser(80m);
        var update = new UpdateUserInfo(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<UpdateUserInfo>());

        await update.Execute(new UpdateUserInfoInput("user@example.com", "alice", "Alice", "Smith", 78.5m, 180, 4, new DateOnly(1990, 1, 1)));

        var entry = Assert.Single(await BodyMeasurementRepository.ForUser(user.Id));
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), entry.Date);
        Assert.Equal(78.5m, entry.WeightKg);
    }

    [Fact]
    public async Task UpdateUserInfo_WithSameWeight_ShouldNotLogAnything()
    {
        var user = await SeedUser(80m);
        var update = new UpdateUserInfo(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<UpdateUserInfo>());

        await update.Execute(new UpdateUserInfoInput("user@example.com", "alice", "Alice", "Smith", 80m, 181, 4, new DateOnly(1990, 1, 1)));

        Assert.Empty(await BodyMeasurementRepository.ForUser(user.Id));
    }
}
