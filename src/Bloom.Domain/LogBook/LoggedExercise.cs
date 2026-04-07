using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LogBook;

public readonly record struct LoggedExerciseId(Guid Value) : IEntityId;

public class LoggedExercise: Entity<LoggedExerciseId>
{
    public ExerciseId ExerciseId { get; private set; }
    public int Order { get; private set; }
    public LoggedWorkoutId LoggedWorkoutId { get; private set; }
    public virtual List<LoggedStrengthSet> StrengthSets { get; private set; }
    public virtual List<LoggedCardioSet> CardioSets { get; private set; }

    // EF Core requires a parameterless constructor
    private LoggedExercise() 
    {
        StrengthSets = new List<LoggedStrengthSet>();
        CardioSets = new List<LoggedCardioSet>();
    }

    private LoggedExercise(LoggedExerciseId id, LoggedWorkoutId loggedWorkoutId, ExerciseId exerciseId, int order) : base(id)
    {
        LoggedWorkoutId = loggedWorkoutId;
        ExerciseId = exerciseId;
        Order = order;
        StrengthSets = new List<LoggedStrengthSet>();
        CardioSets = new List<LoggedCardioSet>();
    }

    public static LoggedExercise Create(LoggedWorkoutId loggedWorkoutId, ExerciseId exerciseId, int order, LoggedExerciseId? id = null)
    {
        LoggedExercise exercise = new(
            id ?? EntityId.New<LoggedExerciseId>(),
            loggedWorkoutId,
            exerciseId,
            order
        );
        exercise.ValidateState();
        return exercise;
    }

    public void AddSet(LoggedStrengthSet set)
    {
        StrengthSets.Add(set);
    }

    public void AddSet(LoggedCardioSet set)
    {
        CardioSets.Add(set);
    }

    public override void ValidateState()
    {
        if (ExerciseId == default)
            throw new InvalidOperationException("ExerciseId must be set.");

        if (Order < 0)
            throw new InvalidOperationException("Order must be a non-negative integer.");
    }
}