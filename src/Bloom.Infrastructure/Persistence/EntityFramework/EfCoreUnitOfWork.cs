using Microsoft.Extensions.Logging;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

namespace Bloom.Infrastructure.Persistence.EntityFramework;

public sealed class EfCoreUnitOfWork(
    BloomDbContext dbContext,
    ILogger<EfCoreUnitOfWork> logger
): IUnitOfWork
{
    private readonly Dictionary<string, object> _repositoriesWithRepoKey = [];
    
    public Task Do()
    {
        return dbContext.SaveChangesAsync();
    }

    public Task Save<TRepository>(IAggregateRoot aggregateRoot) 
        where TRepository : IRepository
    {
        string aggregateTypeName = aggregateRoot.GetType().FullName ?? "UnknownAggregateRoot";
        logger.LogInformation(
            "Saving aggregate of type: {AggregateType} using repository of type: {RepositoryType}",
            aggregateTypeName, typeof(TRepository).FullName
        );
        
        string repoKey = typeof(TRepository).FullName
            ?? throw new InvalidOperationException("Cannot get full name of type.");

        if (!_repositoriesWithRepoKey.TryGetValue(repoKey, out object? repositoryObject))
            throw new InvalidOperationException($"Repository for type {repoKey} not found.");
        
        return ((dynamic)repositoryObject).Save((dynamic)aggregateRoot);
    }

    public TRepository Repo<TRepository>() where TRepository : IRepository
    {
        logger.LogInformation(
            "Retrieving repository of type: {RepositoryType}",
            typeof(TRepository).FullName
        );
        
        string repoKey = typeof(TRepository).FullName
            ?? throw new InvalidOperationException("Cannot get full name of type.");
        
        if (_repositoriesWithRepoKey.TryGetValue(repoKey, out object? repositoryObject))
            return (TRepository)repositoryObject;
        
        throw new InvalidOperationException($"Repository for type {repoKey} not found.");
    }

    public void RegisterRepository<TRepository>(TRepository repository)
        where TRepository : IRepository
    {
        string repokey = typeof(TRepository).FullName
                         ?? throw new InvalidOperationException("Cannot get full name of type.");
        
        _repositoriesWithRepoKey[repokey] = repository;
        
        logger.LogInformation(
            "Registered repository of type: {RepositoryType} with key: {RepoKey}",
            typeof(TRepository).FullName, repokey
        );
    }
}