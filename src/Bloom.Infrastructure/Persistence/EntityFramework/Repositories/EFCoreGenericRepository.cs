using Aornis;
using Bloom.Domain.Shared;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public abstract class EfCoreGenericRepository<TAggregateRoot, TId>(
    DomainDbContext context
): IRepository<TAggregateRoot, TId> 
    where TAggregateRoot: AggregateRoot<TId> 
    where TId: struct, IEntityId
{
    protected readonly DomainDbContext _context = context;
    
    public virtual Task<bool> Exists(TId id)
    {
        return _context
            .Set<TAggregateRoot>()
            .AnyAsync(aggregateRoot => aggregateRoot.Id.Equals(id));
    }

    public virtual async Task<Optional<TAggregateRoot>> ById(TId id)
    {
        var entity = await _context.Set<TAggregateRoot>().FindAsync(id);
        return Optional.Of(entity);
    }

    public virtual Task Save(TAggregateRoot aggregateRoot)
    {
        if (_context.Entry(aggregateRoot).State == EntityState.Detached)
            return _context.Set<TAggregateRoot>().AddAsync(aggregateRoot).AsTask();

        return Task.CompletedTask;
    }

    public virtual Task Remove(TAggregateRoot aggregateRoot)
    {
        _context.Set<TAggregateRoot>().Remove(aggregateRoot);
        return Task.CompletedTask;
    }
}