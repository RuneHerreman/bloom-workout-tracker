using Bloom.Application.Contracts;
using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using UnitTests.Application.Exercises;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class GetLoggedExerciseVolumeTests
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

    private LoggedWorkoutData WorkoutWithSet(Guid exerciseId, decimal weight, DateTime loggedAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = _userId,
        LoggedAt = loggedAt,
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

    private GetLoggedExerciseVolume BuildUseCase(
        IEnumerable<LoggedWorkoutData> logs,
        IEnumerable<ExerciseData> catalog)
    {
        return new GetLoggedExerciseVolume(
            StubCurrentUser.With(_userId),
            new MockFindLoggedWorkoutsQuery(logs),
            new MockSearchExerciseCatalogQuery(catalog)
        );
    }

    [Fact]
    public async Task Execute_WithNoFilters_ShouldReturnVolumeForAllLoggedExercises()
    {
        var useCase = BuildUseCase(
            logs:
            [
                WorkoutWithSet(_benchId, 100m, new DateTime(2025, 1, 10)),
                WorkoutWithSet(_squatId, 120m, new DateTime(2025, 1, 15))
            ],
            catalog: [BenchPress(), Squat()]
        );

        var output = await useCase.Execute(new GetLoggedExerciseVolumeInput(null, null, null, null, null, null, null));

        Assert.Equal(2, output.Exercises.Count);
    }

    [Fact]
    public async Task Execute_ShouldReturnMaxWeightPerMonth()
    {
        var logs = new List<LoggedWorkoutData>
        {
            WorkoutWithSet(_benchId, 80m, new DateTime(2025, 1, 5)),
            WorkoutWithSet(_benchId, 100m, new DateTime(2025, 1, 20)),
            WorkoutWithSet(_benchId, 90m, new DateTime(2025, 1, 25))
        };
        var useCase = BuildUseCase(logs, [BenchPress()]);

        var output = await useCase.Execute(new GetLoggedExerciseVolumeInput(null, null, null, null, null, null, null));

        var entry = Assert.Single(output.Exercises);
        var month = Assert.Single(entry.MonthlyVolume);
        Assert.Equal(100m, month.MaxWeight);
        Assert.Equal(2025, month.Year);
        Assert.Equal(1, month.Month);
    }

    [Fact]
    public async Task Execute_ShouldGroupIntoSeparateMonths()
    {
        var logs = new List<LoggedWorkoutData>
        {
            WorkoutWithSet(_benchId, 80m, new DateTime(2025, 1, 10)),
            WorkoutWithSet(_benchId, 100m, new DateTime(2025, 2, 10)),
            WorkoutWithSet(_benchId, 95m, new DateTime(2025, 3, 10))
        };
        var useCase = BuildUseCase(logs, [BenchPress()]);

        var output = await useCase.Execute(new GetLoggedExerciseVolumeInput(null, null, null, null, null, null, null));

        var entry = Assert.Single(output.Exercises);
        Assert.Equal(3, entry.MonthlyVolume.Count);
    }

    [Fact]
    public async Task Execute_ShouldOrderMonthsChronologically()
    {
        var logs = new List<LoggedWorkoutData>
        {
            WorkoutWithSet(_benchId, 100m, new DateTime(2025, 3, 1)),
            WorkoutWithSet(_benchId, 80m, new DateTime(2025, 1, 1)),
            WorkoutWithSet(_benchId, 90m, new DateTime(2025, 2, 1))
        };
        var useCase = BuildUseCase(logs, [BenchPress()]);

        var output = await useCase.Execute(new GetLoggedExerciseVolumeInput(null, null, null, null, null, null, null));

        var months = Assert.Single(output.Exercises).MonthlyVolume;
        Assert.Equal(1, months[0].Month);
        Assert.Equal(2, months[1].Month);
        Assert.Equal(3, months[2].Month);
    }

    [Fact]
    public async Task Execute_WithNoWeightedSets_ShouldReturnEmpty()
    {
        var logWithoutWeight = new LoggedWorkoutData
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            LoggedAt = new DateTime(2025, 1, 10),
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

        var output = await useCase.Execute(new GetLoggedExerciseVolumeInput(null, null, null, null, null, null, null));

        Assert.Empty(output.Exercises);
    }

    [Fact]
    public async Task Execute_WithFromDateRange_ShouldExcludeEarlierMonths()
    {
        var logs = new List<LoggedWorkoutData>
        {
            WorkoutWithSet(_benchId, 80m, new DateTime(2024, 12, 1)),
            WorkoutWithSet(_benchId, 100m, new DateTime(2025, 1, 1)),
            WorkoutWithSet(_benchId, 110m, new DateTime(2025, 2, 1))
        };
        var useCase = BuildUseCase(logs, [BenchPress()]);

        var output = await useCase.Execute(new GetLoggedExerciseVolumeInput(null, null, null, 2025, 1, null, null));

        var months = Assert.Single(output.Exercises).MonthlyVolume;
        Assert.Equal(2, months.Count);
        Assert.All(months, m => Assert.True(m.Year > 2024));
    }

    [Fact]
    public async Task Execute_WithToDateRange_ShouldExcludeLaterMonths()
    {
        var logs = new List<LoggedWorkoutData>
        {
            WorkoutWithSet(_benchId, 80m, new DateTime(2025, 1, 1)),
            WorkoutWithSet(_benchId, 100m, new DateTime(2025, 2, 1)),
            WorkoutWithSet(_benchId, 110m, new DateTime(2025, 3, 1))
        };
        var useCase = BuildUseCase(logs, [BenchPress()]);

        var output = await useCase.Execute(new GetLoggedExerciseVolumeInput(null, null, null, null, null, 2025, 2));

        var months = Assert.Single(output.Exercises).MonthlyVolume;
        Assert.Equal(2, months.Count);
        Assert.DoesNotContain(months, m => m.Month == 3);
    }

    [Fact]
    public async Task Execute_WithNameFilter_ShouldOnlyReturnMatchingExercises()
    {
        var useCase = BuildUseCase(
            logs:
            [
                WorkoutWithSet(_benchId, 100m, new DateTime(2025, 1, 1)),
                WorkoutWithSet(_squatId, 120m, new DateTime(2025, 1, 1))
            ],
            catalog: [BenchPress(), Squat()]
        );

        var output = await useCase.Execute(new GetLoggedExerciseVolumeInput("squat", null, null, null, null, null, null));

        var entry = Assert.Single(output.Exercises);
        Assert.Equal("Squat", entry.ExerciseName);
    }
}
