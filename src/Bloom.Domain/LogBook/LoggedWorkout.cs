using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.LogBook;

public readonly record struct LoggedWorkoutId(Guid Value) : IEntityId;

public class LoggedWorkout: AggregateRoot<LoggedWorkoutId>
{
    public string Name { get; private set; } = null!;
    public UserId UserId { get; private set; }
    public DateTime Date { get; private set; }
    public virtual List<LoggedExercise> Exercises { get; private set; }
    public decimal TotalVolume => Exercises
        .SelectMany(e => e.StrengthSets)
        .Sum(s => s.CalculateVolume());

    // EF Core requires a parameterless constructor
    private LoggedWorkout() 
    {
        Exercises = new List<LoggedExercise>();
    }

    private LoggedWorkout(LoggedWorkoutId id, string name, UserId userId, DateTime date) : base(id)
    {
        Name = name;
        UserId = userId;
        Date = date;
        Exercises = new List<LoggedExercise>();
    }

    public static LoggedWorkout Create(string name, UserId userId, DateTime date, LoggedWorkoutId? loggedWorkoutId = null)
    {
        LoggedWorkout workout = new(
            loggedWorkoutId ?? EntityId.New<LoggedWorkoutId>(),
            name,
            userId,
            date
        );
        workout.ValidateState();
        return workout;
    }

    public void AddExercise(LoggedExercise exercise)
    {
        Exercises.Add(exercise);
    }

    public override void ValidateState()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Logged workout name cannot be empty.");

        if (UserId == default)
            throw new InvalidOperationException("UserId must be set.");

        if (Date == default)
            throw new InvalidOperationException("Date must be set.");
    }
}