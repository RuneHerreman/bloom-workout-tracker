using Bloom.Application.Contracts;
using Bloom.Application.Users;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using UnitTests.Application.LoggedWorkouts;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;
using UnitTests.Application.WorkoutTemplates;

namespace UnitTests.Application.Users;

public sealed class RenameUserGearTests : ApplicationTestBase
{
    private async Task<User> SeedUser(params string[] gear)
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        user.UpdateGear([.. gear]);
        await UserRepository.Save(user);
        return user;
    }

    private async Task<LoggedWorkout> SeedLog(UserId userId, params string[] gear)
    {
        var log = LoggedWorkout.Create(userId, "Run",
        [
            LoggedExercise.Create(EntityId.New<ExerciseId>(), 0,
                [LoggedSet.CreateStrength(0, 8, 60m, WeightUnit.Kg, 2)], gear: [.. gear])
        ]);
        await LoggedWorkoutRepository.Save(log);
        return log;
    }

    private async Task<WorkoutTemplate> SeedTemplate(UserId userId, params string[] gear)
    {
        var template = WorkoutTemplate.Create(userId, "Long run",
        [
            TemplateExercise.Create(EntityId.New<ExerciseId>(), 0,
                [PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 8)], gear: [.. gear])
        ]);
        await WorkoutTemplateRepository.Save(template);
        return template;
    }

    /// <summary>The query side, built from what the repositories hold now.</summary>
    private RenameUserGear CreateUseCase(UserId userId, IEnumerable<LoggedWorkout> logs, IEnumerable<WorkoutTemplate> templates) => new(
        UnitOfWork,
        StubCurrentUser.With(userId),
        new MockFindLoggedWorkoutsQuery(logs.Select(l => new LoggedWorkoutData
        {
            Id = l.Id.Value,
            UserId = l.UserId.Value,
            Name = l.Name,
            LoggedExercises = l.LoggedExercises.Select(e => new LoggedExerciseData { Id = e.Id.Value, Gear = [.. e.Gear] }).ToList(),
        })),
        new MockFindWorkoutTemplatesQuery(templates.Select(t => new WorkoutTemplateData
        {
            Id = t.Id.Value,
            UserId = t.UserId.Value,
            TemplateExercises = t.TemplateExercises.Select(e => new TemplateExerciseData { Id = e.Id.Value, Gear = [.. e.Gear] }).ToList(),
        })),
        CreateLogger<RenameUserGear>());

    [Fact]
    public async Task Execute_ShouldRenameInGearListLogsAndTemplates()
    {
        var user = await SeedUser("Pegasus", "Road bike");
        var used = await SeedLog(user.Id, "Pegasus");
        var other = await SeedLog(user.Id, "Road bike");
        var template = await SeedTemplate(user.Id, "Pegasus");

        var output = await CreateUseCase(user.Id, [used, other], [template])
            .Execute(new RenameUserGearInput("Pegasus", "Pegasus 41"));

        Assert.Equal(new RenameUserGearOutput(1, 1), output);
        Assert.Equal(["Pegasus 41", "Road bike"], (await UserRepository.ById(user.Id)).Value.Gear);
        Assert.Equal(["Pegasus 41"], (await LoggedWorkoutRepository.ById(used.Id)).Value.LoggedExercises[0].Gear);
        Assert.Equal(["Road bike"], (await LoggedWorkoutRepository.ById(other.Id)).Value.LoggedExercises[0].Gear);
        Assert.Equal(["Pegasus 41"], (await WorkoutTemplateRepository.ById(template.Id)).Value.TemplateExercises[0].Gear);
    }

    [Fact]
    public async Task Execute_ShouldTidyWhitespaceInNewName()
    {
        var user = await SeedUser("Pegasus");

        await CreateUseCase(user.Id, [], []).Execute(new RenameUserGearInput("Pegasus", "  Nike   Pegasus  "));

        Assert.Equal(["Nike Pegasus"], (await UserRepository.ById(user.Id)).Value.Gear);
    }

    [Fact]
    public async Task Execute_WithCaseOnlyChange_ShouldRename()
    {
        var user = await SeedUser("pegasus");

        await CreateUseCase(user.Id, [], []).Execute(new RenameUserGearInput("pegasus", "Pegasus"));

        Assert.Equal(["Pegasus"], (await UserRepository.ById(user.Id)).Value.Gear);
    }

    [Fact]
    public async Task Execute_WithNameOfOtherItem_ShouldThrow()
    {
        var user = await SeedUser("Pegasus", "Road bike");

        await Assert.ThrowsAsync<BloomGeneralException>(
            () => CreateUseCase(user.Id, [], []).Execute(new RenameUserGearInput("Pegasus", "road bike")));
    }

    [Fact]
    public async Task Execute_WithUnknownItem_ShouldThrow()
    {
        var user = await SeedUser("Pegasus");

        await Assert.ThrowsAsync<BloomGeneralException>(
            () => CreateUseCase(user.Id, [], []).Execute(new RenameUserGearInput("Vaporfly", "Vaporfly 3")));
    }

    [Fact]
    public async Task Execute_WithBlankName_ShouldThrow()
    {
        var user = await SeedUser("Pegasus");

        await Assert.ThrowsAsync<BloomGeneralException>(
            () => CreateUseCase(user.Id, [], []).Execute(new RenameUserGearInput("Pegasus", "   ")));
    }

    [Fact]
    public async Task Execute_ShouldLeaveOtherUsersLogsAlone()
    {
        var user = await SeedUser("Pegasus");
        var stranger = EntityId.New<UserId>();
        var theirs = await SeedLog(stranger, "Pegasus");

        var output = await CreateUseCase(user.Id, [theirs], []).Execute(new RenameUserGearInput("Pegasus", "Pegasus 41"));

        Assert.Equal(0, output.Workouts);
        Assert.Equal(["Pegasus"], (await LoggedWorkoutRepository.ById(theirs.Id)).Value.LoggedExercises[0].Gear);
    }
}
