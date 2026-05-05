using Bloom.Domain.Shared;
using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace Bloom.Domain.LoggedWorkouts;

public class LoggedCardioSet : LoggedSet
{
    public Duration Duration { get; init; }
    public Distance Distance { get; init; }

    private LoggedCardioSet() { }

    private LoggedCardioSet(
        LoggedSetId id,
        Duration duration,
        Distance distance) : base(id)
    {
        Duration = duration;
        Distance = distance;
    }

    public static LoggedCardioSet Create(
        TimeSpan duration,
        decimal distance,
        DistanceUnit distanceUnit)
    {
        var set = new LoggedCardioSet(
            EntityId.New<LoggedSetId>(),
            Duration.Create(duration),
            Distance.Create(distance, distanceUnit));

        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(Duration);
        Asserts.EnsureNotEmpty(Distance);
    }
}