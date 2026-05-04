using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace Bloom.Domain.WorkoutTemplates;

public class PlannedCardioSet: PlannedSet
{
    public PlannedDuration Duration { get; private set; }
    public PlannedDistance Distance { get; private set; }

    private PlannedCardioSet() { }

    private PlannedCardioSet(
        PlannedSetId id,
        PlannedDuration duration,
        PlannedDistance distance) : base(id)
    {
        Duration = duration;
        Distance = distance;
    }

    public static PlannedCardioSet Create(
        TimeSpan duration,
        decimal distance,
        PlannedDistanceUnit distanceUnit)
    {
        var set = new PlannedCardioSet(
            EntityId.New<PlannedSetId>(),
            PlannedDuration.Create(duration),
            PlannedDistance.Create(distance, distanceUnit));

        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(Duration);
        Asserts.EnsureNotEmpty(Distance);
    }
}