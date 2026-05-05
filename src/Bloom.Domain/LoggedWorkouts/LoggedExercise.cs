using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedExerciseId(Guid Value) : IEntityId;

public class LoggedExercise : Entity<LoggedExerciseId>
{
    private readonly List<LoggedStrengthSet> _strengthSets = [];
    private readonly List<LoggedCardioSet> _cardioSets = [];

    public ExerciseId ExerciseId { get; private set; }
    public IEnumerable<LoggedStrengthSet> StrengthSets => _strengthSets.AsReadOnly();
    public IEnumerable<LoggedCardioSet> CardioSets => _cardioSets.AsReadOnly();
    public IEnumerable<LoggedSet> Sets => [.._strengthSets, .._cardioSets];

    private LoggedExercise() { }

    private LoggedExercise(
        LoggedExerciseId id,
        ExerciseId exerciseId,
        List<LoggedStrengthSet> strengthSets,
        List<LoggedCardioSet> cardioSets) : base(id)
    {
        ExerciseId = exerciseId;
        _strengthSets = strengthSets;
        _cardioSets = cardioSets;
    }

    public static LoggedExercise Create(
        ExerciseId exerciseId,
        List<LoggedStrengthSet> strengthSets,
        List<LoggedCardioSet> cardioSets)
    {
        var loggedExercise = new LoggedExercise(EntityId.New<LoggedExerciseId>(), exerciseId, strengthSets, cardioSets);
        loggedExercise.ValidateState();
        return loggedExercise;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(ExerciseId);
        Asserts.EnsureNotEmpty(Sets);
    }
}
