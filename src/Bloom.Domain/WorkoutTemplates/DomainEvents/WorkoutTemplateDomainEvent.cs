using Bloom.Domain.Shared.DomainEvents;

namespace Bloom.Domain.WorkoutTemplates.DomainEvents;

public class WorkoutTemplateDomainEvent(string eventName): BaseDomainEvent(eventName, nameof(WorkoutTemplate))
{
    
}