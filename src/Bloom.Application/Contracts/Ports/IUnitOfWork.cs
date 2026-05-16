using Bloom.Domain.Shared;

namespace Bloom.Application.Contracts.Ports;

public interface IUnitOfWork
{
    Task Do(CancellationToken ct = default);
    Task Save<TRepository>(IAggregateRoot aggregateRoot) where TRepository : IRepository;
    TRepository Repo<TRepository>() where TRepository : IRepository;
}
