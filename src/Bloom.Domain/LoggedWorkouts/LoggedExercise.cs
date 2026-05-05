using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedExerciseId(Guid Value) : IEntityId;

public class LoggedExercise : Entity<LoggedExerciseId>
{
    private readonly List<LoggedSet> _sets = [];

    public ExerciseId ExerciseId { get; private set; }
    public int Order { get; private set; }

    // The only persisted collection
    public IReadOnlyList<LoggedSet> Sets => _sets.AsReadOnly();

    // Convenience (computed) views - do NOT map these in EF
    [NotMapped]
    public IEnumerable<LoggedSet> CardioSets => _sets.Where(s => s.Type == ExerciseType.Cardio);
    [NotMapped]
    public IEnumerable<LoggedSet> StrengthSets => _sets.Where(s => s.Type == ExerciseType.Strength);
    [NotMapped]
    public IEnumerable<LoggedSet> PlyometricSets => _sets.Where(s => s.Type == ExerciseType.Plyometric);

    private LoggedExercise() { }

    private LoggedExercise(
        LoggedExerciseId id,
        ExerciseId exerciseId,
        int order,
        List<LoggedSet> sets) : base(id)
    {
        ExerciseId = exerciseId;
        Order = order;
        _sets = sets;
    }

    public static LoggedExercise Create(ExerciseId exerciseId, int order, List<LoggedSet> sets)
    {
        var loggedExercise = new LoggedExercise(
            EntityId.New<LoggedExerciseId>(),
            exerciseId,
            order,
            sets);

        loggedExercise.ValidateState();
        return loggedExercise;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(ExerciseId);
        Asserts.EnsureNotNegative(Order);
        Asserts.EnsureNotEmpty(_sets);
    }
}