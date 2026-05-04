namespace Bloom.Domain.LoggedWorkouts.DomainEvents;

public class WorkoutLogged(
    LoggedWorkoutId loggedWorkoutId
) : LoggedWorkoutDomainEvent(nameof(WorkoutLogged))
{
    public string LoggedWorkoutId = loggedWorkoutId.Value.ToString();
}