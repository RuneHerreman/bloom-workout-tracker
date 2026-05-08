using Bloom.Domain.Shared;

namespace UnitTests.Domain.Shared;

public sealed class AssertsTests
{
    [Fact]
    public void EnsureNotEmpty_WithEmptyString_ShouldThrow()
    {
        Action act = () => Asserts.EnsureNotEmpty("");
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureNotEmpty_WithWhitespaceString_ShouldThrow()
    {
        Action act = () => Asserts.EnsureNotEmpty("   ");
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureNotEmpty_WithValidString_ShouldNotThrow()
    {
        Action act = () => Asserts.EnsureNotEmpty("ok");
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureNotEmpty_WithNullObject_ShouldThrow()
    {
        object? value = null;
        Action act = () => Asserts.EnsureNotEmpty(value!);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureNotEmpty_WithValidObject_ShouldNotThrow()
    {
        object value = new();
        Action act = () => Asserts.EnsureNotEmpty(value);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureNotEmpty_WithEmptyList_ShouldThrow()
    {
        List<string> list = [];
        Action act = () => Asserts.EnsureNotEmpty(list);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureNotEmpty_WithNullList_ShouldThrow()
    {
        IReadOnlyList<string>? list = null;
        Action act = () => Asserts.EnsureNotEmpty(list);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureNotEmpty_WithPopulatedList_ShouldNotThrow()
    {
        IReadOnlyList<string> list = ["a"];
        Action act = () => Asserts.EnsureNotEmpty(list);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureGreaterThan_Int_NotGreater_ShouldThrow()
    {
        Action act = () => Asserts.EnsureGreaterThan(5, 5);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureGreaterThan_Int_Greater_ShouldNotThrow()
    {
        Action act = () => Asserts.EnsureGreaterThan(10, 5);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureGreaterThan_Decimal_NotGreater_ShouldThrow()
    {
        Action act = () => Asserts.EnsureGreaterThan(0m, 0m);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureGreaterThan_Decimal_Greater_ShouldNotThrow()
    {
        Action act = () => Asserts.EnsureGreaterThan(1.5m, 1m);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureGreaterThan_TimeSpan_NotGreater_ShouldThrow()
    {
        Action act = () => Asserts.EnsureGreaterThan(TimeSpan.Zero, TimeSpan.Zero);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureGreaterThan_TimeSpan_Greater_ShouldNotThrow()
    {
        Action act = () => Asserts.EnsureGreaterThan(TimeSpan.FromSeconds(1), TimeSpan.Zero);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureNotNegative_Negative_ShouldThrow()
    {
        Action act = () => Asserts.EnsureNotNegative(-1);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureNotNegative_Zero_ShouldNotThrow()
    {
        Action act = () => Asserts.EnsureNotNegative(0);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureNotNegative_Positive_ShouldNotThrow()
    {
        Action act = () => Asserts.EnsureNotNegative(1);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureLessThan_NotLess_ShouldThrow()
    {
        Action act = () => Asserts.EnsureLessThan(10, 5);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureLessThan_Equal_ShouldThrow()
    {
        Action act = () => Asserts.EnsureLessThan(5, 5);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureLessThan_Less_ShouldNotThrow()
    {
        Action act = () => Asserts.EnsureLessThan(3, 5);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureTrue_False_ShouldThrow()
    {
        Action act = () => Asserts.EnsureTrue(false);
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void EnsureTrue_True_ShouldNotThrow()
    {
        Action act = () => Asserts.EnsureTrue(true);
        Assert.Null(Record.Exception(act));
    }
}
