using Bloom.Domain.Body;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace UnitTests.Domain.Body;

public sealed class BodyMeasurementTests
{
    private static readonly UserId User = EntityId.New<UserId>();
    private static readonly DateOnly Day = new(2026, 11, 11);

    [Fact]
    public void Create_ShouldRoundWeightAndBodyFat()
    {
        var m = BodyMeasurement.Create(User, Day, 78.456m, 17.84m, BodyMeasurementSource.Manual);

        Assert.Equal(78.46m, m.WeightKg);
        Assert.Equal(17.8m, m.BodyFatPct);
        Assert.Equal(Day, m.Date);
        Assert.Equal(BodyMeasurementSource.Manual, m.Source);
    }

    [Theory]
    [InlineData(19.99)]
    [InlineData(400.01)]
    [InlineData(0)]
    public void Create_WithWeightOutOfRange_ShouldThrow(decimal kg)
    {
        Assert.Throws<ArgumentException>(() => BodyMeasurement.Create(User, Day, kg, null, BodyMeasurementSource.Manual));
    }

    [Theory]
    [InlineData(2.9)]
    [InlineData(70.1)]
    public void Create_WithBodyFatOutOfRange_ShouldThrow(decimal pct)
    {
        Assert.Throws<ArgumentException>(() => BodyMeasurement.Create(User, Day, 80m, pct, BodyMeasurementSource.Manual));
    }

    [Theory]
    [InlineData(20, true)]
    [InlineData(400, true)]
    [InlineData(19.9, false)]
    public void IsValidWeight_ShouldMatchTheRange(decimal kg, bool valid) => Assert.Equal(valid, BodyMeasurement.IsValidWeight(kg));

    [Fact]
    public void Update_WithoutBodyFat_ShouldKeepTheExistingReading()
    {
        var m = BodyMeasurement.Create(User, Day, 80m, 18m, BodyMeasurementSource.Manual);

        m.Update(79.4m, null);

        Assert.Equal(79.4m, m.WeightKg);
        Assert.Equal(18m, m.BodyFatPct);
    }

    [Fact]
    public void Update_WithBodyFat_ShouldReplaceIt()
    {
        var m = BodyMeasurement.Create(User, Day, 80m, 18m, BodyMeasurementSource.Manual);

        m.Update(80m, 17.5m);

        Assert.Equal(17.5m, m.BodyFatPct);
    }

    [Fact]
    public void ClearBodyFat_ShouldRemoveTheReading()
    {
        var m = BodyMeasurement.Create(User, Day, 80m, 18m, BodyMeasurementSource.Manual);

        m.ClearBodyFat();

        Assert.Null(m.BodyFatPct);
    }

    [Fact]
    public void Update_WithInvalidWeight_ShouldThrow()
    {
        var m = BodyMeasurement.Create(User, Day, 80m, null, BodyMeasurementSource.Manual);

        Assert.Throws<ArgumentException>(() => m.Update(500m, null));
    }
}
