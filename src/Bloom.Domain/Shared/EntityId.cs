using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Bloom.Domain.Shared;

public interface IEntityId
{
    Guid Value { get; }
}

public static class EntityId
{
    private static readonly ConcurrentDictionary<Type, Delegate> _constructors = new();

    public static TId New<TId>(Guid? guid = default) where TId : struct, IEntityId
    {
        var factory = (Func<Guid, TId>)_constructors.GetOrAdd(typeof(TId), CreateFactory<TId>);

        Guid id = guid ?? Guid.CreateVersion7();
        
        return factory(id);
    }

    private static Func<Guid, TId> CreateFactory<TId>(Type type) where TId : struct
    {
        var constructor = type.GetConstructor([typeof(Guid)])
            ?? throw new InvalidOperationException($"Type {type.Name} must have a constructor accepting a Guid.");

        var param = Expression.Parameter(typeof(Guid), "value");
        var newExpr = Expression.New(constructor, param);
        var lambda = Expression.Lambda<Func<Guid, TId>>(newExpr, param);

        return lambda.Compile();
    }
}