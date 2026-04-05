namespace Bloom.Domain.Shared;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : struct, IEntityId
{
    public TId Id { get; protected set; }
    
    protected Entity() { }
    
    protected Entity(TId id)
    {
        Id = id;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        
        return Equals((Entity<TId>)obj);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        // Optimized: Compare the primitive GUID values directly
        return Id.Value.Equals(other.Id.Value);
    }

    public override int GetHashCode()
    {
        return Id.Value.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    public abstract void ValidateState();
}