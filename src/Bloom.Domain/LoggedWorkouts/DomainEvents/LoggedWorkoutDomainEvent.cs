using Bloom.Domain.Shared.DomainEvents;

namespace Bloom.Domain.LoggedWorkouts.DomainEvents;

public class LoggedWorkoutDomainEvent(
    string eventName
) : BaseDomainEvent(
    eventName: eventName,
    aggregateName:  nameof(LoggedWorkout)
) { }