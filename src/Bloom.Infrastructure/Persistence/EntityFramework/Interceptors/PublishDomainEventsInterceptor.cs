using Bloom.Domain.Shared;
using Bloom.Domain.Shared.DomainEvents;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Interceptors;

public sealed class PublishDomainEventsInterceptor(
    IDomainEventPublisher domainEventPublisher,
    ILogger<PublishDomainEventsInterceptor> logger
) 
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var events = context.ChangeTracker.Entries<IAggregateRoot>()
            .Select(x => x.Entity)
            .Where(x => x.DomainEvents.Count != 0)
            .SelectMany(x => 
            {
                var domainEvents = x.DomainEvents.ToList();
                x.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        if (context is DomainDbContext domainContext && events.Count != 0)
            domainContext.QueueDomainEvent(events);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, 
        int result, 
        CancellationToken cancellationToken = default)
    {
        
        if (eventData.Context is DomainDbContext domainContext)
        {
            IReadOnlyCollection<IDomainEvent> eventsToPublish = domainContext.GetQueuedDomainEvents();
            
            foreach (var domainEvent in eventsToPublish)
                await domainEventPublisher.Publish(domainEvent);

            domainContext.ClearQueuedDomainEvents();
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}