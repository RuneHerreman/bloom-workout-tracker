namespace Bloom.Domain.Entity;

public class BodyMetric
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime MeasuredDate { get; set; } = DateTime.UtcNow;
    public int Weight { get; set; } // kg
    public decimal? BodyFatPercentage { get; set; }
}