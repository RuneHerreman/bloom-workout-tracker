using Bloom.Application.Contracts;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace UnitTests.Application.Contracts.Data;

public sealed class DataRecordTests
{
    [Fact]
    public void LoggedExerciseData_ShouldInitializeProperties()
    {
        Guid id = Guid.NewGuid();
        Guid exerciseId = Guid.NewGuid();
        var data = new LoggedExerciseData
        {
            Id = id,
            ExerciseId = exerciseId,
            Order = 2,
            Sets = [new LoggedSetData { Id = Guid.NewGuid(), Order = 0, Type = ExerciseType.Strength }]
        };

        Assert.Equal(id, data.Id);
        Assert.Equal(exerciseId, data.ExerciseId);
        Assert.Equal(2, data.Order);
        Assert.Single(data.Sets);
    }

    [Fact]
    public void LoggedSetData_ShouldInitializeProperties()
    {
        var data = new LoggedSetData
        {
            Id = Guid.NewGuid(),
            Order = 0,
            Type = ExerciseType.Cardio,
            Duration = TimeSpan.FromMinutes(20),
            Distance = new DistanceData { Value = 5m, Unit = DistanceUnit.Km },
            Reps = 8,
            Weight = new WeightData { Value = 60m, Unit = WeightUnit.Kg },
            Rir = 2
        };

        Assert.Equal(ExerciseType.Cardio, data.Type);
        Assert.Equal(TimeSpan.FromMinutes(20), data.Duration);
        Assert.NotNull(data.Distance);
        Assert.Equal(5m, data.Distance.Value);
        Assert.Equal(8, data.Reps);
        Assert.NotNull(data.Weight);
        Assert.Equal(60m, data.Weight.Value);
        Assert.Equal(2, data.Rir);
    }

    [Fact]
    public void DistanceData_ShouldInitializeProperties()
    {
        var data = new DistanceData { Value = 1.5m, Unit = DistanceUnit.Miles };

        Assert.Equal(1.5m, data.Value);
        Assert.Equal(DistanceUnit.Miles, data.Unit);
    }

    [Fact]
    public void WeightData_ShouldInitializeProperties()
    {
        var data = new WeightData { Value = 80m, Unit = WeightUnit.Kg };

        Assert.Equal(80m, data.Value);
        Assert.Equal(WeightUnit.Kg, data.Unit);
    }

    [Fact]
    public void TemplateExerciseData_ShouldInitializeProperties()
    {
        var data = new TemplateExerciseData
        {
            Id = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            Order = 1,
            Sets = [new PlannedSetData { Id = Guid.NewGuid(), Order = 0, Type = ExerciseType.Strength, Reps = 8 }]
        };

        Assert.Equal(1, data.Order);
        Assert.Single(data.Sets);
    }

    [Fact]
    public void PlannedSetData_ShouldInitializeProperties()
    {
        var data = new PlannedSetData
        {
            Id = Guid.NewGuid(),
            Order = 0,
            Type = ExerciseType.Cardio,
            Duration = TimeSpan.FromMinutes(15),
            Distance = new PlannedDistanceData { Value = 3m, Unit = PlannedDistanceUnit.Km }
        };

        Assert.NotNull(data.Distance);
        Assert.Equal(3m, data.Distance.Value);
    }

    [Fact]
    public void PlannedDistanceData_ShouldInitializeProperties()
    {
        var data = new PlannedDistanceData { Value = 2.5m, Unit = PlannedDistanceUnit.Miles };

        Assert.Equal(2.5m, data.Value);
        Assert.Equal(PlannedDistanceUnit.Miles, data.Unit);
    }
}
