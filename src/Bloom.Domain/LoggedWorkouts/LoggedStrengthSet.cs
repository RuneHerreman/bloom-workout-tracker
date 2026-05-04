using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public class LoggedStrengthSet: LoggedSet
{
    public Reps Reps { get; private set; }
    public Weight Weight { get; private set; }
    public RIR RIR { get; private set; }
    
    private LoggedStrengthSet() { }

    private LoggedStrengthSet(
        LoggedSetId id,
        Reps reps,
        Weight weight,
        RIR rir) : base(id)
    {
        Reps = reps;
        Weight = weight;
        RIR = rir;
    }

    public static LoggedStrengthSet Create(
        int reps,
        decimal weight,
        WeightUnit weightUnit,
        int rir)
    {
        var set = new LoggedStrengthSet(
            EntityId.New<LoggedSetId>(),
            Reps.Create(reps),
            Weight.Create(weight, weightUnit),
            RIR.Create(rir));

        set.ValidateState();
        return set;
    }
    
    
    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(Reps);
        Asserts.EnsureNotEmpty(Weight);
        Asserts.EnsureNotEmpty(RIR);
    }
}