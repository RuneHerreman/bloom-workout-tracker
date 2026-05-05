using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public record LoggedStrengthSet: ValueObject
{
    public Reps Reps { get; init; }
    public Weight Weight { get; init; }
    public RIR RIR { get; init; }
    
    private LoggedStrengthSet() { }

    private LoggedStrengthSet(
        LoggedSetId id,
        Reps reps,
        Weight weight,
        RIR rir
    )
    {
        Reps = reps;
        Weight = weight;
        RIR = rir;
    }

    public static LoggedStrengthSet Create(
        int reps,
        decimal weight,
        WeightUnit weightUnit,
        int rir
    )
    {
        var set = new LoggedStrengthSet(
            EntityId.New<LoggedSetId>(),
            Reps.Create(reps),
            Weight.Create(weight, weightUnit),
            RIR.Create(rir));

        set.ValidateState();
        return set;
    }
    
    private void ValidateState()
    {
        Asserts.EnsureNotEmpty(Reps);
        Asserts.EnsureNotEmpty(Weight);
        Asserts.EnsureNotEmpty(RIR);
    }
}