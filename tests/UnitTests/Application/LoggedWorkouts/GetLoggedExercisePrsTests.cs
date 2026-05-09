using Bloom.Application.Contracts;
using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using UnitTests.Application.Exercises;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class GetLoggedExercisePrsTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _benchId = Guid.NewGuid();
    private readonly Guid _squatId = Guid.NewGuid();

    private ExerciseData BenchPress() => new()
    {
        Id = _benchId,
        Name = "Bench Press",
        Type = "Strength",
        TargetMuscles = [new TargetMuscleData("Chest")]
    };

    private ExerciseData Squat() => new()
    {
        Id = _squatId,
        Name = "Squat",
        Type = "Strength",
        TargetMuscles = [new TargetMuscleData("Quadriceps")]
    };

    private LoggedWorkoutData WorkoutWithSet(Guid exerciseId, decimal weight, DateTime? loggedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = _userId,
        LoggedAt = loggedAt ?? DateTime.UtcNow,
        LoggedExercises =
        [
            new LoggedExerciseData
            {
                Id = Guid.NewGuid(),
                ExerciseId = exerciseId,
                Order = 0,
                Sets =
                [
                    new LoggedSetData
                    {
                        Id = Guid.NewGuid(),
                        Order = 0,
                        Type = ExerciseType.Strength,
                        Reps = 5,
                        Weight = new WeightData { Value = weight, Unit = WeightUnit.Kg }
                    }
                ]
            }
        ]
    };

    private GetLoggedExercisePrs BuildUseCase(
        IEnumerable<LoggedWorkoutData> logs,
        IEnumerable<ExerciseData> catalog)
    {
        return new GetLoggedExercisePrs(
            StubCurrentUser.With(_userId),
            new MockFindLoggedWorkoutsQuery(logs),
            new MockSearchExerciseCatalogQuery(catalog)
        );
    }

    [Fact]
    public async Task Execute_WithNoFilters_ShouldReturnPrForEachLoggedExercise()
    {
        var useCase = BuildUseCase(
            logs: [WorkoutWithSet(_benchId, 100m), WorkoutWithSet(_squatId, 120m)],
            catalog: [BenchPress(), Squat()]
        );

        var output = await useCase.Execute(new GetLoggedExercisePrsInput(null, null, null));

        Assert.Equal(2, output.Prs.Count);
    }

    [Fact]
    public async Task Execute_ShouldReturnHighestWeightAsPr()
    {
        var logs = new List<LoggedWorkoutData>
        {
            WorkoutWithSet(_benchId, 80m),
            WorkoutWithSet(_benchId, 100m),
            WorkoutWithSet(_benchId, 90m)
        };
        var useCase = BuildUseCase(logs, [BenchPress()]);

        var output = await useCase.Execute(new GetLoggedExercisePrsInput(null, null, null));

        var pr = Assert.Single(output.Prs);
        Assert.Equal(100m, pr.Weight);
    }

    [Fact]
    public async Task Execute_WithNoWeightedSets_ShouldReturnEmpty()
    {
        var logWithoutWeight = new LoggedWorkoutData
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            LoggedAt = DateTime.UtcNow,
            LoggedExercises =
            [
                new LoggedExerciseData
                {
                    Id = Guid.NewGuid(),
                    ExerciseId = _benchId,
                    Order = 0,
                    Sets = [new LoggedSetData { Id = Guid.NewGuid(), Order = 0, Type = ExerciseType.Cardio }]
                }
            ]
        };
        var useCase = BuildUseCase([logWithoutWeight], [BenchPress()]);

        var output = await useCase.Execute(new GetLoggedExercisePrsInput(null, null, null));

        Assert.Empty(output.Prs);
    }

    [Fact]
    public async Task Execute_WithNameFilter_ShouldOnlyReturnMatchingExercises()
    {
        var useCase = BuildUseCase(
            logs: [WorkoutWithSet(_benchId, 100m), WorkoutWithSet(_squatId, 120m)],
            catalog: [BenchPress(), Squat()]
        );

        var output = await useCase.Execute(new GetLoggedExercisePrsInput("bench", null, null));

        var pr = Assert.Single(output.Prs);
        Assert.Equal("Bench Press", pr.ExerciseName);
    }

    [Fact]
    public async Task Execute_WithMuscleGroupFilter_ShouldOnlyReturnMatchingExercises()
    {
        var useCase = BuildUseCase(
            logs: [WorkoutWithSet(_benchId, 100m), WorkoutWithSet(_squatId, 120m)],
            catalog: [BenchPress(), Squat()]
        );

        var output = await useCase.Execute(new GetLoggedExercisePrsInput(null, ["Quadriceps"], null));

        var pr = Assert.Single(output.Prs);
        Assert.Equal("Squat", pr.ExerciseName);
    }

    [Fact]
    public async Task Execute_WithExerciseTypeFilter_ShouldOnlyReturnMatchingExercises()
    {
        var cardioId = Guid.NewGuid();
        var sprint = new ExerciseData
        {
            Id = cardioId,
            Name = "Sprint",
            Type = "Cardio",
            TargetMuscles = [new TargetMuscleData("Legs")]
        };
        var useCase = BuildUseCase(
            logs: [WorkoutWithSet(_benchId, 100m), WorkoutWithSet(cardioId, 5m)],
            catalog: [BenchPress(), sprint]
        );

        var output = await useCase.Execute(new GetLoggedExercisePrsInput(null, null, ["Strength"]));

        var pr = Assert.Single(output.Prs);
        Assert.Equal("Bench Press", pr.ExerciseName);
    }

    [Fact]
    public async Task Execute_WhenExerciseNotInCatalog_ShouldBeExcluded()
    {
        var unknownId = Guid.NewGuid();
        var useCase = BuildUseCase(
            logs: [WorkoutWithSet(unknownId, 100m)],
            catalog: [BenchPress()]
        );

        var output = await useCase.Execute(new GetLoggedExercisePrsInput(null, null, null));

        Assert.Empty(output.Prs);
    }
}
