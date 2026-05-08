using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Users;
using Bloom.Domain.WorkoutTemplates;

namespace UnitTests.Domain;

public sealed class IdConstructorTests
{
    [Fact]
    public void ExerciseId_ShouldStoreValue()
    {
        Guid guid = Guid.NewGuid();
        ExerciseId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void UserId_ShouldStoreValue()
    {
        Guid guid = Guid.NewGuid();
        UserId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void LoggedWorkoutId_ShouldStoreValue()
    {
        Guid guid = Guid.NewGuid();
        LoggedWorkoutId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void LoggedExerciseId_ShouldStoreValue()
    {
        Guid guid = Guid.NewGuid();
        LoggedExerciseId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void LoggedSetId_ShouldStoreValue()
    {
        Guid guid = Guid.NewGuid();
        LoggedSetId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void WorkoutTemplateId_ShouldStoreValue()
    {
        Guid guid = Guid.NewGuid();
        WorkoutTemplateId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void TemplateExerciseId_ShouldStoreValue()
    {
        Guid guid = Guid.NewGuid();
        TemplateExerciseId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void PlannedSetId_ShouldStoreValue()
    {
        Guid guid = Guid.NewGuid();
        PlannedSetId id = new(guid);
        Assert.Equal(guid, id.Value);
    }
}
