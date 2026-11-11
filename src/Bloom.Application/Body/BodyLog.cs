using Bloom.Domain.Body;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.Body;

/// <summary>An entry in the body log as the API returns it.</summary>
public sealed record BodyMeasurementData(DateOnly Date, decimal WeightKg, decimal? BodyFatPct, string Source);

internal static class BodyLog
{
    /// <summary>Clients send their local date, which can be a day ahead of UTC.</summary>
    public static DateOnly LatestAllowedDate() => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
    public static readonly DateOnly EarliestAllowedDate = new(1900, 1, 1);

    public static BodyMeasurementData ToData(this BodyMeasurement m) =>
        new(m.Date, m.WeightKg, m.BodyFatPct, m.Source.ToString());

    /// <summary>Checks a reading before it reaches the domain, so a bad value is a 400 with a reason.</summary>
    public static void Validate(DateOnly date, decimal weightKg, decimal? bodyFatPct)
    {
        if (date > LatestAllowedDate())
            throw new InvalidBodyMeasurementException("The date can't be in the future.");
        if (date < EarliestAllowedDate)
            throw new InvalidBodyMeasurementException("The date is too far in the past.");
        if (!BodyMeasurement.IsValidWeight(weightKg))
            throw new InvalidBodyMeasurementException($"Weight must be between {BodyMeasurement.MinWeightKg:0} and {BodyMeasurement.MaxWeightKg:0} kg.");
        if (bodyFatPct is { } bf && !BodyMeasurement.IsValidBodyFat(bf))
            throw new InvalidBodyMeasurementException($"Body fat must be between {BodyMeasurement.MinBodyFatPct:0} and {BodyMeasurement.MaxBodyFatPct:0}%.");
    }
}
