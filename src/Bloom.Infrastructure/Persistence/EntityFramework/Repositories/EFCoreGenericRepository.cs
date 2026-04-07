using Bloom.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public abstract class EfCoreGenericRepository<TAggregateRoot, TId>(
    BloomDbContext context
): IRepository<TAggregateRoot, TId> 
    where TAggregateRoot: AggregateRoot<TId> 
    where TId: struct, IEntityId
{
    protected readonly BloomDbContext _context = context;
    
    public virtual Task<bool> Exists(TId id)
    {
        return _context
            .Set<TAggregateRoot>()
            .AnyAsync(e => e.Id.Equals(id));
    }

    public virtual Task<TAggregateRoot> ById(TId id)
    {
        return Task.FromResult(_context.Set<TAggregateRoot>().Find(id))!;
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