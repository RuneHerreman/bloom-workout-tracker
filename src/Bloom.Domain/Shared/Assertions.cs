using System.Runtime.CompilerServices;

namespace Bloom.Domain.Shared;

public static class Asserts
{
    public static void EnsureNotEmpty(
        string value,
        [CallerArgumentExpression(nameof(value))] string? paramName = ""
    )
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty.", paramName);
    }
    
    public static void EnsureNotEmpty(
        object value,
        [CallerArgumentExpression(nameof(value))] string? paramName = ""
    )
    {
        if (value == null)
            throw new ArgumentException("Value cannot be empty.", paramName);
    }
    
    public static void EnsureNotEmpty<T>(
        IReadOnlyList<T>? value, 
        [CallerArgumentExpression(nameof(value))] string? paramName = ""
    )
    {
        if (value == null || value.Count == 0)
        {
            throw new ArgumentException("Collection cannot be null or empty.", paramName);
        }
    }
    

    public static void EnsureGreaterThan(
        int value,
        int threshold,
        [CallerArgumentExpression(nameof(value))] string? paramName = ""
    )
    {
        if (value <= threshold)
            throw new ArgumentException($"Value must be greater than {threshold}.", paramName);
    }
    
    public static void EnsureGreaterThan(
        decimal value,
        decimal threshold,
        [CallerArgumentExpression(nameof(value))] string? paramName = ""
    )
    {
        if (value <= threshold)
            throw new ArgumentException($"Value must be greater than {threshold}.", paramName);
    }

    public static void EnsureNotNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = ""
    )
    {
        if (value < 0)
            throw new ArgumentException("Value cannot be negative.", paramName);
    }
    
    public static void EnsureLessThan(
        int value,
        int threshold,
        [CallerArgumentExpression(nameof(value))] string? paramName = ""
    )
    {
        if (value >= threshold)
            throw new ArgumentException($"Value must be less than {threshold}.", paramName);
    }
}
