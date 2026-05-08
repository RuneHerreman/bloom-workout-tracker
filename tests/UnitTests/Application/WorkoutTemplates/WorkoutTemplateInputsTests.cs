using Bloom.Application.WorkoutTemplates;
using Bloom.Domain.Users;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.WorkoutTemplates;

public sealed class WorkoutTemplateInputsTests : ApplicationTestBase
{
    private readonly CreateWorkoutTemplate _useCase;

    public WorkoutTemplateInputsTests()
    {
        User user = User.Create("user@example.com", "alice", "hash");
        UserRepository.Save(user).GetAwaiter().GetResult();

        _useCase = new CreateWorkoutTemplate(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<CreateWorkoutTemplate>());
    }

    private static CreateWorkoutTemplateInput WithSet(PlannedSetInput set) =>
        new("Day", [new TemplateExerciseInput(Guid.NewGuid(), 0, [set])]);

    [Fact]
    public async Task ToPlannedSet_Cardio_ShouldMap()
    {
        var input = WithSet(new PlannedSetInput("Cardio", 0, null, TimeSpan.FromMinutes(10), 3m, "Km"));

        var output = await _useCase.Execute(input);

        Assert.NotEqual(Guid.Empty, output.WorkoutTemplateId);
    }

    [Fact]
    public async Task ToPlannedSet_Plyometric_ShouldMap()
    {
        var input = WithSet(new PlannedSetInput("Plyometric", 0, 5, null, null, null));

        var output = await _useCase.Execute(input);

        Assert.NotEqual(Guid.Empty, output.WorkoutTemplateId);
    }

    [Fact]
    public async Task ToPlannedSet_Cardio_MissingDuration_ShouldThrow()
    {
        var input = WithSet(new PlannedSetInput("Cardio", 0, null, null, 3m, "Km"));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToPlannedSet_Cardio_MissingDistance_ShouldThrow()
    {
        var input = WithSet(new PlannedSetInput("Cardio", 0, null, TimeSpan.FromMinutes(10), null, "Km"));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToPlannedSet_Cardio_MissingDistanceUnit_ShouldThrow()
    {
        var input = WithSet(new PlannedSetInput("Cardio", 0, null, TimeSpan.FromMinutes(10), 3m, null));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToPlannedSet_Strength_MissingReps_ShouldThrow()
    {
        var input = WithSet(new PlannedSetInput("Strength", 0, null, null, null, null));

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task ToPlannedSet_NumericTypeOutOfRange_ShouldThrow()
    {
        // "99" parses to (ExerciseType)99 — falls through the switch to the default.
        var input = WithSet(new PlannedSetInput("99", 0, 1, null, null, null));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _useCase.Execute(input));
    }
}
