using Bloom.Domain.Shared;

namespace Bloom.Domain.Users;

public readonly record struct BodyMetricId(Guid Value) : IEntityId;

public class BodyMetric : Entity<BodyMetricId>
{
    public UserId UserId { get; private set; }
    public DateTime MeasuredDate { get; private set; }
    public decimal Weight { get; private set; } // kg
    public decimal? BodyFatPercentage { get; private set; }

    // EF Core requires a parameterless constructor
    private BodyMetric() {}

    private BodyMetric(
        BodyMetricId id,  
        UserId userId,
        DateTime measuredDate,
        decimal weight,
        decimal? bodyFatPercentage) : base(id)
    {
        UserId = userId;
        MeasuredDate = measuredDate;
        Weight = weight;
        BodyFatPercentage = bodyFatPercentage;
    }

    public static BodyMetric Create(
        UserId userId,
        DateTime measuredDate,
        decimal weight,
        decimal? bodyFatPercentage,
        BodyMetricId? id = null)
    {
        var metric = new BodyMetric(
            id ?? EntityId.New<BodyMetricId>(),
            userId,
            measuredDate,
            weight,
            bodyFatPercentage
        );
        metric.ValidateState();
        return metric;
    }

    public override void ValidateState()
    {
        if (Weight <= 0 || Weight > 500)
            throw new InvalidOperationException("Weight must be between 0 and 500 kg.");

        if (BodyFatPercentage.HasValue && (BodyFatPercentage < 0 || BodyFatPercentage > 100))
            throw new InvalidOperationException("Body fat percentage must be between 0 and 100.");

        if (MeasuredDate > DateTime.UtcNow)
            throw new InvalidOperationException("Measured date cannot be in the future.");
    }
}