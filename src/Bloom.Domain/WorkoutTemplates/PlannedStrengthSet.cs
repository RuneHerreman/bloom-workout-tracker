using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace Bloom.Domain.WorkoutTemplates;

public class PlannedStrengthSet: PlannedSet
{
    public PlannedReps Reps { get; private set; }

    private PlannedStrengthSet() { }

    private PlannedStrengthSet(
        PlannedSetId id,
        PlannedReps reps) : base(id)
    {
        Reps = reps;
    }

    public static PlannedStrengthSet Create(int reps)
    {
        var set = new PlannedStrengthSet(
            EntityId.New<PlannedSetId>(),
            PlannedReps.Create(reps));

        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(Reps);
    }
}