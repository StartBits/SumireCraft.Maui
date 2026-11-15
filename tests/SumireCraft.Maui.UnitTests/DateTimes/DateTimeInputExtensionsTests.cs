using SumireCraft.DateTimes;

namespace SumireCraft.Maui.UnitTests.DateTimes;

public class DateTimeInputExtensionsTests
{
    private static readonly DateTime SomeDateTime = new(2026, 11, 8, 10, 30, 15);

    [Fact]
    public void ADateTime_BecomesAnInputHoldingIt()
    {
        var input = SomeDateTime.ToDateTimeInput();

        Assert.Equal(SomeDateTime, input.DateAndTime);
        Assert.True(input.IsComplete);
    }

    [Fact]
    public void ANullableDateTime_BecomesAnInputHoldingIt()
    {
        DateTime? value = SomeDateTime;

        var input = value.ToDateTimeInput();

        Assert.Equal(SomeDateTime, input.DateAndTime);
        Assert.True(input.IsComplete);
    }

    [Fact]
    public void Null_BecomesAnEmptyInput()
    {
        DateTime? value = null;

        var input = value.ToDateTimeInput();

        Assert.NotNull(input);
        Assert.Null(input.DateAndTime);
        Assert.Null(input.DateAsDateOnly);
        Assert.Null(input.TimeAsTimeOnly);
    }

    [Fact]
    public void Null_ComesBackAsNull()
    {
        DateTime? value = null;

        DateTime? back = value.ToDateTimeInput();

        Assert.Null(back);
    }
}
