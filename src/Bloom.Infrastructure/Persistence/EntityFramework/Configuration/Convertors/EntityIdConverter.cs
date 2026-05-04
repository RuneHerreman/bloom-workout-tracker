using Bloom.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Howestprime.Movies.Infrastructure.Persistence.EntityFramework.Configuration.Converters;

public static class EntityIdConverter
{
    public static void AddConventions(ModelConfigurationBuilder configurationBuilder)
    {
        var entityIdTypes = typeof(IEntityId).Assembly
            .GetTypes()
            .Where(type =>
                type.IsValueType &&
                !type.IsAbstract &&
                type.IsAssignableTo(typeof(IEntityId))
            );

        foreach (var type in entityIdTypes)
        {
            var converterType = typeof(EntityIdConverter<>).MakeGenericType(type);
            configurationBuilder.Properties(type).HaveConversion(converterType);
        }
    }
}

public class EntityIdConverter<TId> : ValueConverter<TId, Guid>
    where TId : struct, IEntityId
{
    public EntityIdConverter() 
        : base(
            id => id.Value,                    
            value => EntityId.New<TId>(value)    
        )
    {
    }
}
