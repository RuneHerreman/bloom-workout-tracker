using System.Reflection;
using Bloom.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Howestprime.Movies.Infrastructure.Persistence.EntityFramework.Configuration.Converters;

public static class SinglePropertyValueObjectConverter
{
    public static void AddConventions(ModelConfigurationBuilder configurationBuilder)
    {
        var valueObjectTypes = typeof(ValueObject).Assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                type.IsAssignableTo(typeof(ValueObject))
            );

        foreach (var type in valueObjectTypes)
        {
            PropertyInfo[] properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead)
                .ToArray();

            if (properties.Length != 1)
                continue;

            Type valueType = properties[0].PropertyType;

            if (!CanCreateFromValue(type, valueType))
                continue;

            var converterType = typeof(SinglePropertyValueObjectConverter<,>).MakeGenericType(type, valueType);
            configurationBuilder.Properties(type).HaveConversion(converterType);
        }
    }

    private static bool CanCreateFromValue(Type valueObjectType, Type valueType)
    {
        bool hasCreateMethod = valueObjectType.GetMethod(
            "Create",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [valueType],
            modifiers: null
        ) is not null;

        if (hasCreateMethod)
            return true;

        bool hasFromMethod = valueObjectType.GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [valueType],
            modifiers: null
        ) is not null;

        if (hasFromMethod)
            return true;

        return valueObjectType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [valueType],
            modifiers: null
        ) is not null;
    }
}

/*
    Generic converter for ValueObjects with exactly one readable property.
    Converts between the ValueObject type and its underlying property type.
*/
public class SinglePropertyValueObjectConverter<TValueObject, TValue> : ValueConverter<TValueObject, TValue>
    where TValueObject : ValueObject
{
    private static readonly PropertyInfo ValueProperty = GetValueProperty();
    private static readonly Func<TValue, TValueObject> FromValue = CreateFromValue();

    public SinglePropertyValueObjectConverter()
        : base(
            valueObject => ToProvider(valueObject),
            value => FromProvider(value)
        )
    {
    }

    private static TValue ToProvider(TValueObject valueObject)
    {
        object? value = ValueProperty.GetValue(valueObject);
        return value is TValue castValue
            ? castValue
            : throw new InvalidOperationException(
                $"{typeof(TValueObject).Name}.{ValueProperty.Name} must be assignable to {typeof(TValue).Name}."
            );
    }

    private static TValueObject FromProvider(TValue value) => FromValue(value);

    private static PropertyInfo GetValueProperty()
    {
        PropertyInfo[] properties = typeof(TValueObject)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead)
            .ToArray();

        if (properties.Length != 1)
            throw new InvalidOperationException(
                $"{typeof(TValueObject).Name} must have exactly one readable property."
            );

        PropertyInfo valueProperty = properties[0];
        if (valueProperty.PropertyType != typeof(TValue))
            throw new InvalidOperationException(
                $"{typeof(TValueObject).Name}.{valueProperty.Name} must be of type {typeof(TValue).Name}."
            );

        return valueProperty;
    }

    private static Func<TValue, TValueObject> CreateFromValue()
    {
        MethodInfo? createMethod = typeof(TValueObject).GetMethod(
            "Create",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(TValue)],
            modifiers: null
        );

        if (createMethod is not null)
            return value => (TValueObject)createMethod.Invoke(null, [value])!;

        MethodInfo? fromMethod = typeof(TValueObject).GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(TValue)],
            modifiers: null
        );

        if (fromMethod is not null)
            return value => (TValueObject)fromMethod.Invoke(null, [value])!;

        ConstructorInfo? constructor = typeof(TValueObject).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(TValue)],
            modifiers: null
        );

        if (constructor is not null)
            return value => (TValueObject)constructor.Invoke([value]);

        throw new InvalidOperationException(
            $"{typeof(TValueObject).Name} must expose Create({typeof(TValue).Name}), From({typeof(TValue).Name}), or a matching constructor."
        );
    }
}
