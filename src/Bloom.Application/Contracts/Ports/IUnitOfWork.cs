using Bloom.Domain.Shared;

namespace Bloom.Application.Contracts.Ports;

public interface IUnitOfWork
{
    // TODO: should this IUnitOfWork extend IDisposable or IAsyncDisposable?
    Task Do();
    Task Save<TRepository>(IAggregateRoot aggregateRoot) where TRepository : IRepository;
    TRepository Repo<TRepository>() where TRepository : IRepository;
}
