namespace Bloom.Domain.LoggedWorkouts.DomainEvents;

public class LoggedWorkoutDeleted(
    LoggedWorkoutId loggedWorkoutId
) : LoggedWorkoutDomainEvent(nameof(LoggedWorkoutDeleted))
{
    public string LoggedWorkoutId = loggedWorkoutId.Value.ToString();
}