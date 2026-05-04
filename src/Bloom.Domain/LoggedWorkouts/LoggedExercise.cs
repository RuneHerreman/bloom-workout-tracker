using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedExerciseId(Guid Value) : IEntityId;

public class LoggedExercise: Entity<LoggedExerciseId>
{
    private readonly List<LoggedSet> _sets = new();
    
    public ExerciseId ExerciseId { get; private set; }
    public IReadOnlyList<LoggedSet> Sets => _sets.AsReadOnly();
    
    private LoggedExercise() {}
    
    private LoggedExercise(LoggedExerciseId id, ExerciseId exerciseId, List<LoggedSet> sets) : base(id)
    {
        ExerciseId = exerciseId;
        _sets = sets;
    }

    public static LoggedExercise Create(ExerciseId exerciseId, List<LoggedSet> sets)
    {
        var id = EntityId.New<LoggedExerciseId>();
        var loggedExercise = new LoggedExercise(id, exerciseId, sets);
        
        loggedExercise.ValidateState();
        
        return loggedExercise;
    }
    
    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(ExerciseId);
        Asserts.EnsureNotEmpty(Sets);
    }
}