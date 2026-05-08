using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.DomainEvents;
using Bloom.Domain.Shared;

namespace UnitTests.Domain.LoggedWorkouts.DomainEvents;

public sealed class LoggedWorkoutDomainEventsTests
{
    [Fact]
    public void WorkoutLogged_ShouldExposeIdAndFqdn()
    {
        LoggedWorkoutId id = EntityId.New<LoggedWorkoutId>();

        WorkoutLogged evt = new(id);

        Assert.Equal(id.Value.ToString(), evt.LoggedWorkoutId);
        Assert.Equal("Bloom.Bloom.LoggedWorkout.WorkoutLogged", evt.FQDN);
    }

    [Fact]
    public void LoggedWorkoutUpdated_ShouldExposeIdAndFqdn()
    {
        LoggedWorkoutId id = EntityId.New<LoggedWorkoutId>();

        LoggedWorkoutUpdated evt = new(id);

        Assert.Equal(id.Value.ToString(), evt.LoggedWorkoutId);
        Assert.Equal("Bloom.Bloom.LoggedWorkout.LoggedWorkoutUpdated", evt.FQDN);
    }

    [Fact]
    public void LoggedWorkoutDeleted_ShouldExposeIdAndFqdn()
    {
        LoggedWorkoutId id = EntityId.New<LoggedWorkoutId>();

        LoggedWorkoutDeleted evt = new(id);

        Assert.Equal(id.Value.ToString(), evt.LoggedWorkoutId);
        Assert.Equal("Bloom.Bloom.LoggedWorkout.LoggedWorkoutDeleted", evt.FQDN);
    }

    [Fact]
    public void SetLogged_CanBeInstantiated()
    {
        SetLogged evt = new();
        Assert.NotNull(evt);
    }

    [Fact]
    public void WorkoutCompleted_CanBeInstantiated()
    {
        WorkoutCompleted evt = new();
        Assert.NotNull(evt);
    }
}
