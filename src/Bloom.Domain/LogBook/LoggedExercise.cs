using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LogBook;

public readonly record struct LoggedExerciseId(Guid Value) : IEntityId;

public class LoggedExercise: Entity<LoggedExerciseId>
{
    public ExerciseId ExerciseId { get; private set; }
    public int Order { get; private set; }
    public LoggedWorkoutId LoggedWorkoutId { get; private set; }
    public virtual List<ExerciseSet> Sets { get; private set; }

    // EF Core requires a parameterless constructor
    private LoggedExercise() 
    {
        Sets = new List<ExerciseSet>();
    }

    private LoggedExercise(LoggedExerciseId id, LoggedWorkoutId loggedWorkoutId, ExerciseId exerciseId, int order) : base(id)
    {
        LoggedWorkoutId = loggedWorkoutId;
        ExerciseId = exerciseId;
        Order = order;
        Sets = new List<ExerciseSet>();
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

    public void AddSet(ExerciseSet set)
    {
        if (set is not LoggedStrengthSet && set is not LoggedCardioSet)
            throw new ArgumentException("Only logged sets can be added to a logged exercise.", nameof(set));
            
        Sets.Add(set);
    }

    public override void ValidateState()
    {
        if (ExerciseId == default)
            throw new InvalidOperationException("ExerciseId must be set.");

        if (Order < 0)
            throw new InvalidOperationException("Order must be a non-negative integer.");
    }
}