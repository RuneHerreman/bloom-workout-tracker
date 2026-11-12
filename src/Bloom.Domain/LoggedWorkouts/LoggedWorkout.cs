using Bloom.Domain.LoggedWorkouts.DomainEvents;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedWorkoutId(Guid Value) : IEntityId;

public class LoggedWorkout : AggregateRoot<LoggedWorkoutId>
{
    private readonly List<LoggedExercise> _loggedExercises = new();

    public UserId UserId { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Note { get; private set; }
    public IReadOnlyList<LoggedExercise> LoggedExercises => _loggedExercises.AsReadOnly();

    private LoggedWorkout() { }

    private LoggedWorkout(
        LoggedWorkoutId id,
        UserId userId,
        string name,
        string? note,
        DateTime loggedAt,
        List<LoggedExercise> loggedExercises) : base(id)
    {
        UserId = userId;
        Name = name;
        Note = note;
        LoggedAt = loggedAt;
        _loggedExercises = loggedExercises;
    }

    public string? ExternalId { get; private set; }

    public static LoggedWorkout Create(
        UserId userId,
        string name,
        List<LoggedExercise> loggedExercises,
        string? note = null,
        DateTime? loggedAt = null,
        string? externalId = null)
    {
        var loggedWorkout = new LoggedWorkout(
            EntityId.New<LoggedWorkoutId>(),
            userId,
            name,
            note,
            loggedAt ?? DateTime.UtcNow,
            loggedExercises
        );
        loggedWorkout.ExternalId = externalId;

        loggedWorkout.ValidateState();
        loggedWorkout.RaiseDomainEvent(new WorkoutLogged(loggedWorkout.Id));

        return loggedWorkout;
    }

    public void Update(string name, string? note, DateTime loggedAt, List<LoggedExercise> loggedExercises)
    {
        Name = name;
        Note = note;
        LoggedAt = loggedAt;
        _loggedExercises.Clear();
        _loggedExercises.AddRange(loggedExercises);

        ValidateState();
        RaiseDomainEvent(new LoggedWorkoutUpdated(Id));
    }

    /// <summary>Renames a gear item on every exercise that used it. Returns whether anything changed.</summary>
    public bool RenameGear(string from, string to)
    {
        var changed = false;
        foreach (var exercise in _loggedExercises)
            changed |= exercise.RenameGear(from, to);
        if (changed) RaiseDomainEvent(new LoggedWorkoutUpdated(Id));
        return changed;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(UserId);
        Asserts.EnsureNotEmpty(Name);
    }
}
