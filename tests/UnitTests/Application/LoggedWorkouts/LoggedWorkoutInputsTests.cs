using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.Enums;
using Bloom.Domain.Users;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class LoggedWorkoutInputsTests : ApplicationTestBase
{
    private readonly CreateLoggedWorkout _useCase;

    public LoggedWorkoutInputsTests()
    {
        User user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        UserRepository.Save(user).GetAwaiter().GetResult();

        _useCase = new CreateLoggedWorkout(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<CreateLoggedWorkout>());
    }

    private static CreateLoggedWorkoutInput WithSet(LoggedSetInput set) =>
        new("Test Workout", [new LoggedExerciseInput(Guid.NewGuid(), 0, [set])]);

    [Fact]
    public async Task ToLoggedSet_Plyometric_ShouldMap()
    {
        var input = WithSet(new LoggedSetInput("Plyometric", 0, null, null, null, 8, 20m, "Kg", 1));

        var output = await _useCase.Execute(input);

        Assert.NotEqual(Guid.Empty, output.LoggedWorkoutId);
    }

    [Fact]
    public async Task ToLoggedSet_Cardio_MissingDuration_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Cardio", 0, null, 5m, "Km", null, null, null, null));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Cardio_MissingDistance_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Cardio", 0, TimeSpan.FromMinutes(10), null, "Km", null, null, null, null));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Cardio_MissingDistanceUnit_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Cardio", 0, TimeSpan.FromMinutes(10), 5m, null, null, null, null, null));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Strength_MissingReps_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Strength", 0, null, null, null, null, 50m, "Kg", 1));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Strength_MissingWeight_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Strength", 0, null, null, null, 5, null, "Kg", 1));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Strength_MissingWeightUnit_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Strength", 0, null, null, null, 5, 50m, null, 1));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Strength_NullRir_ShouldSucceed()
    {
        var input = WithSet(new LoggedSetInput("Strength", 0, null, null, null, 5, 50m, "Kg", null));

        await _useCase.Execute(input); // RIR is nullable — null is valid
    }

    [Fact]
    public async Task ToLoggedSet_Plyometric_MissingReps_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Plyometric", 0, null, null, null, null, 50m, "Kg", 1));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Plyometric_MissingWeight_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Plyometric", 0, null, null, null, 5, null, "Kg", 1));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Plyometric_MissingWeightUnit_ShouldThrow()
    {
        var input = WithSet(new LoggedSetInput("Plyometric", 0, null, null, null, 5, 50m, null, 1));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToLoggedSet_Plyometric_NullRir_ShouldSucceed()
    {
        var input = WithSet(new LoggedSetInput("Plyometric", 0, null, null, null, 5, 50m, "Kg", null));

        await _useCase.Execute(input); // RIR is nullable — null is valid
    }

    [Fact]
    public async Task ToLoggedSet_NumericTypeOutOfRange_ShouldThrow()
    {
        // "99" parses to (ExerciseType)99, which is not in the switch — hits the default.
        var input = WithSet(new LoggedSetInput("99", 0, null, null, null, 1, 1m, "Kg", 0));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _useCase.Execute(input));
    }

    [Theory]
    [InlineData("W", SetMarker.WarmUp)]
    [InlineData("w", SetMarker.WarmUp)]
    [InlineData("WarmUp", SetMarker.WarmUp)]
    [InlineData("warmup", SetMarker.WarmUp)]
    [InlineData("D", SetMarker.DropSet)]
    [InlineData("d", SetMarker.DropSet)]
    [InlineData("DropSet", SetMarker.DropSet)]
    [InlineData("dropset", SetMarker.DropSet)]
    public void ParseMarker_WithValidValues_ShouldMap(string raw, SetMarker expected)
    {
        Assert.Equal(expected, LoggedExerciseInputExtensions.ParseMarker(raw));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseMarker_WithEmptyValues_ShouldReturnNull(string? raw)
    {
        Assert.Null(LoggedExerciseInputExtensions.ParseMarker(raw));
    }

    [Fact]
    public void ParseMarker_WithInvalidValue_ShouldThrow()
    {
        Assert.ThrowsAny<ArgumentException>(() => LoggedExerciseInputExtensions.ParseMarker("SuperSet"));
    }

    [Fact]
    public void ToLoggedSet_WithMarker_ShouldSetMarker()
    {
        var set = new LoggedSetInput("Strength", 0, null, null, null, 5, 50m, "Kg", 1, "W").ToLoggedSet();

        Assert.Equal(SetMarker.WarmUp, set.Marker);
    }

    [Fact]
    public void ToLoggedExercise_WithNoteAndGear_ShouldMap()
    {
        var input = new LoggedExerciseInput(
            Guid.NewGuid(),
            0,
            [new LoggedSetInput("Strength", 0, null, null, null, 5, 50m, "Kg", 1)],
            Note: "Felt strong today",
            Gear: ["Nike Vaporfly", "Garmin Forerunner"]
        );

        var exercise = input.ToLoggedExercise();

        Assert.Equal("Felt strong today", exercise.Note);
        Assert.Equal(["Nike Vaporfly", "Garmin Forerunner"], exercise.Gear);
    }
}
