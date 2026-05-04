using Bloom.Domain.LoggedWorkouts.DomainEvents;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedWorkoutId(Guid Value) : IEntityId;

public class LoggedWorkout: AggregateRoot<LoggedWorkoutId>
{
    private readonly List<LoggedExercise> _loggedExercises = new();
    
    public UserId UserId { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public IReadOnlyList<LoggedExercise> LoggedExercises => _loggedExercises.AsReadOnly();
    
    private LoggedWorkout() {}
    
    private LoggedWorkout(
        LoggedWorkoutId id,
        UserId userId,
        DateTime loggedAt,
        List<LoggedExercise> loggedExercises) : base(id)
    {
        UserId = userId;
        LoggedAt = loggedAt;
        _loggedExercises = loggedExercises;
    }
    
    public static LoggedWorkout Create(
        UserId userId,
        List<LoggedExercise> loggedExercises)
    {
        var loggedWorkout = new LoggedWorkout(
            EntityId.New<LoggedWorkoutId>(),
            userId,
            DateTime.UtcNow,
            loggedExercises
        );

        loggedWorkout.ValidateState();
        loggedWorkout.RaiseDomainEvent(new WorkoutLogged(loggedWorkout.Id));

        return loggedWorkout;
    }
    
    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(UserId);
        Asserts.EnsureNotEmpty(_loggedExercises);
    }
}