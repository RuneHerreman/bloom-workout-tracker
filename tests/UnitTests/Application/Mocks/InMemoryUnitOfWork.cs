using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.Shared.DomainEvents;

namespace UnitTests.Application.Mocks;

public sealed class InMemoryUnitOfWork(IDomainEventPublisher domainEventPublisher) : IUnitOfWork
{
    private readonly List<IAggregateRoot> _aggregates = [];
    private readonly Dictionary<string, object> _repositoriesWithRepoKey = [];

    public async Task Do()
    {
        IReadOnlyList<IDomainEvent> domainEvents =
        [
            .. _aggregates
                .Where(x => x.DomainEvents.Count != 0)
                .SelectMany(x =>
                {
                    var events = x.DomainEvents.ToList();
                    x.ClearDomainEvents();
                    return events;
                })
        ];

        foreach (var domainEvent in domainEvents)
            await domainEventPublisher.Publish(domainEvent);
    }

    public TRepository Repo<TRepository>()
        where TRepository : IRepository
    {
        string repoKey = typeof(TRepository).FullName
            ?? throw new InvalidOperationException("Cannot get full name of type.");

        if (_repositoriesWithRepoKey.TryGetValue(repoKey, out object? value))
            return (TRepository)value;

        throw new InvalidOperationException($"Repository for type {repoKey} not found.");
    }

    public async Task Save<TRepository>(IAggregateRoot aggregate)
        where TRepository : IRepository
    {
        string repoKey = typeof(TRepository).FullName
            ?? throw new InvalidOperationException("Cannot get full name of type.");

        if (!_repositoriesWithRepoKey.TryGetValue(repoKey, out object? repositoryObj))
            throw new InvalidOperationException($"Repository for type {repoKey} not found.");

        await ((dynamic)repositoryObj).Save((dynamic)aggregate);

        _aggregates.Add(aggregate);
    }

    public void RegisterRepository<TRepository>(IRepository repository)
        where TRepository : IRepository
    {
        string repoKey = typeof(TRepository).FullName
            ?? throw new InvalidOperationException("Cannot get full name of type.");

        _repositoriesWithRepoKey[repoKey] = repository;
    }

    public void TrackAggregate(IAggregateRoot aggregate)
    {
        _aggregates.Add(aggregate);
    }
}
