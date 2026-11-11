using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Body;

public readonly record struct BodyMeasurementId(Guid Value) : IEntityId;

public enum BodyMeasurementSource
{
    Manual,
    Import,
    Signup
}

/// <summary>
/// One day in the body log: a weigh-in, plus a body fat reading when there was one.
/// A user has at most one per day; logging the same day again updates it.
/// </summary>
public class BodyMeasurement : AggregateRoot<BodyMeasurementId>
{
    public const decimal MinWeightKg = 20m;
    public const decimal MaxWeightKg = 400m;
    public const decimal MinBodyFatPct = 3m;
    public const decimal MaxBodyFatPct = 70m;

    public UserId UserId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal WeightKg { get; private set; }
    public decimal? BodyFatPct { get; private set; }
    public BodyMeasurementSource Source { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private BodyMeasurement() { }

    private BodyMeasurement(BodyMeasurementId id, UserId userId, DateOnly date, decimal weightKg, decimal? bodyFatPct, BodyMeasurementSource source)
        : base(id)
    {
        UserId = userId;
        Date = date;
        WeightKg = Round(weightKg, 2);
        BodyFatPct = bodyFatPct is { } bf ? Round(bf, 1) : null;
        Source = source;
        CreatedAt = DateTime.UtcNow;
    }

    public static BodyMeasurement Create(UserId userId, DateOnly date, decimal weightKg, decimal? bodyFatPct, BodyMeasurementSource source)
    {
        var m = new BodyMeasurement(EntityId.New<BodyMeasurementId>(), userId, date, weightKg, bodyFatPct, source);
        m.ValidateState();
        return m;
    }

    /// <summary>A new weigh-in for the same day. Body fat is replaced only when a new reading is given.</summary>
    public void Update(decimal weightKg, decimal? bodyFatPct)
    {
        WeightKg = Round(weightKg, 2);
        if (bodyFatPct is { } bf) BodyFatPct = Round(bf, 1);
        ValidateState();
    }

    public void ClearBodyFat() => BodyFatPct = null;

    public static bool IsValidWeight(decimal kg) => kg >= MinWeightKg && kg <= MaxWeightKg;
    public static bool IsValidBodyFat(decimal pct) => pct >= MinBodyFatPct && pct <= MaxBodyFatPct;

    private static decimal Round(decimal v, int decimals) => Math.Round(v, decimals, MidpointRounding.AwayFromZero);

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(UserId);
        if (!IsValidWeight(WeightKg))
            throw new ArgumentException($"Weight must be between {MinWeightKg} and {MaxWeightKg} kg.", nameof(WeightKg));
        if (BodyFatPct is { } bf && !IsValidBodyFat(bf))
            throw new ArgumentException($"Body fat must be between {MinBodyFatPct} and {MaxBodyFatPct}%.", nameof(BodyFatPct));
    }
}
