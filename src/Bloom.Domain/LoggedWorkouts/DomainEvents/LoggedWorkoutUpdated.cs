namespace Bloom.Domain.LoggedWorkouts.DomainEvents;

public class LoggedWorkoutUpdated(
    LoggedWorkoutId loggedWorkoutId
) : LoggedWorkoutDomainEvent(nameof(LoggedWorkoutUpdated))
{
    public string LoggedWorkoutId = loggedWorkoutId.Value.ToString();
}