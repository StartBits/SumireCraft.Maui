using SumireCraft.DateTimes;

namespace SumireCraft.Maui.UnitTests.DateTimes;

public class DateTimeInputTests
{
    private static readonly DateOnly SomeDate = new(2026, 11, 8);
    private static readonly TimeOnly SomeTime = new(10, 30, 15);
    private static readonly DateTime SomeDateTime = new(2026, 11, 8, 10, 30, 15);

    [Fact]
    public void TheDefaultConstructor_LeavesEverythingEmpty()
    {
        var input = new DateTimeInput();

        Assert.Null(input.DateAsDateOnly);
        Assert.Null(input.TimeAsTimeOnly);
        Assert.Null(input.DateAndTime);
        Assert.False(input.IsComplete);
    }

    [Fact]
    public void TheDateTimeConstructor_SetsBothParts()
    {
        var input = new DateTimeInput(SomeDateTime);

        Assert.Equal(SomeDate, input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
        Assert.Equal(SomeDateTime, input.DateAndTime);
        Assert.True(input.IsComplete);
    }

    [Fact]
    public void DateAndTime_WithADateAlone_IsThatDateAtMidnight()
    {
        var input = new DateTimeInput { DateAsDateOnly = SomeDate };

        Assert.Equal(new DateTime(2026, 11, 8), input.DateAndTime);
    }

    [Fact]
    public void DateAndTime_WithATimeAlone_IsThatTimeOnTheFirstDay()
    {
        var input = new DateTimeInput { TimeAsTimeOnly = SomeTime };

        Assert.Equal(new DateTime(1, 1, 1, 10, 30, 15), input.DateAndTime);
    }

    [Fact]
    public void WritingDateAndTime_SetsBothParts()
    {
        var input = new DateTimeInput { DateAndTime = SomeDateTime };

        Assert.Equal(SomeDate, input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void WritingDateAndTime_KeepsEveryTick()
    {
        var precise = SomeDateTime.AddTicks(1234567);

        var input = new DateTimeInput { DateAndTime = precise };

        Assert.Equal(precise, input.DateAndTime);
    }

    [Fact]
    public void WritingNullToDateAndTime_EmptiesBothParts()
    {
        var input = new DateTimeInput(SomeDateTime) { DateAndTime = null };

        Assert.Null(input.DateAsDateOnly);
        Assert.Null(input.TimeAsTimeOnly);
        Assert.Null(input.DateAndTime);
    }

    [Fact]
    public void WritingTheDateBackOverAnEmptyTime_FillsTheTime()
    {
        var input = new DateTimeInput { DateAsDateOnly = SomeDate };

        input.DateAndTime = input.DateAndTime;

        Assert.Equal(TimeOnly.MinValue, input.TimeAsTimeOnly);
    }

    [Fact]
    public void WritingTheDate_KeepsTheTime()
    {
        var input = new DateTimeInput(SomeDateTime) { DateAsDateOnly = new DateOnly(2027, 1, 2) };

        Assert.Equal(new DateTime(2027, 1, 2, 10, 30, 15), input.DateAndTime);
    }

    [Fact]
    public void WritingTheTime_KeepsTheDate()
    {
        var input = new DateTimeInput(SomeDateTime) { TimeAsTimeOnly = new TimeOnly(1, 2, 3) };

        Assert.Equal(new DateTime(2026, 11, 8, 1, 2, 3), input.DateAndTime);
    }

    [Fact]
    public void EmptyingTheDate_KeepsTheTime()
    {
        var input = new DateTimeInput(SomeDateTime) { DateAsDateOnly = null };

        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
        Assert.Equal(new DateTime(1, 1, 1, 10, 30, 15), input.DateAndTime);
    }

    [Fact]
    public void DateAsDateTime_ReadsMidnight()
    {
        var input = new DateTimeInput(SomeDateTime);

        Assert.Equal(new DateTime(2026, 11, 8), input.DateAsDateTime);
    }

    [Fact]
    public void WritingDateAsDateTime_DiscardsItsTime()
    {
        var input = new DateTimeInput { DateAsDateTime = SomeDateTime };

        Assert.Equal(SomeDate, input.DateAsDateOnly);
        Assert.Null(input.TimeAsTimeOnly);
    }

    [Fact]
    public void WritingNullToDateAsDateTime_EmptiesTheDate()
    {
        var input = new DateTimeInput(SomeDateTime) { DateAsDateTime = null };

        Assert.Null(input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void TimeAsTimeSpan_ReadsAndWritesTheTime()
    {
        var input = new DateTimeInput { TimeAsTimeSpan = new TimeSpan(10, 30, 15) };

        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
        Assert.Equal(new TimeSpan(10, 30, 15), input.TimeAsTimeSpan);
    }

    [Fact]
    public void WritingNullToTimeAsTimeSpan_EmptiesTheTime()
    {
        var input = new DateTimeInput(SomeDateTime) { TimeAsTimeSpan = null };

        Assert.Null(input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(TimeSpan.TicksPerDay)]
    public void WritingATimeSpanThatIsNotATimeOfDay_ChangesNothing(long ticks)
    {
        var input = new DateTimeInput(SomeDateTime);

        input.TimeAsTimeSpan = TimeSpan.FromTicks(ticks);

        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(TimeSpan.TicksPerDay - 1)]
    public void TheEndsOfADay_AreTimesOfDay(long ticks)
    {
        var input = new DateTimeInput { TimeAsTimeSpan = TimeSpan.FromTicks(ticks) };

        Assert.Equal(TimeSpan.FromTicks(ticks), input.TimeAsTimeSpan);
    }

    [Fact]
    public void TimeAsDateTime_ReadsTheTimeOnTheFirstDay()
    {
        var input = new DateTimeInput(SomeDateTime);

        Assert.Equal(new DateTime(1, 1, 1, 10, 30, 15), input.TimeAsDateTime);
    }

    [Fact]
    public void WritingTimeAsDateTime_DiscardsItsDate()
    {
        var input = new DateTimeInput { TimeAsDateTime = SomeDateTime };

        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
        Assert.Null(input.DateAsDateOnly);
    }

    [Fact]
    public void WritingNullToTimeAsDateTime_EmptiesTheTime()
    {
        var input = new DateTimeInput(SomeDateTime) { TimeAsDateTime = null };

        Assert.Null(input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    [Fact]
    public void EveryDateTimeRead_HasAnUnspecifiedKind()
    {
        var input = new DateTimeInput(new DateTime(2026, 11, 8, 10, 30, 15, DateTimeKind.Utc));

        Assert.Equal(DateTimeKind.Unspecified, input.DateAndTime!.Value.Kind);
        Assert.Equal(DateTimeKind.Unspecified, input.DateAsDateTime!.Value.Kind);
        Assert.Equal(DateTimeKind.Unspecified, input.TimeAsDateTime!.Value.Kind);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public void IsComplete_NeedsBothParts(bool hasDate, bool hasTime, bool expected)
    {
        var input = new DateTimeInput
        {
            DateAsDateOnly = hasDate ? SomeDate : null,
            TimeAsTimeOnly = hasTime ? SomeTime : null,
        };

        Assert.Equal(expected, input.IsComplete);
    }

    [Fact]
    public async Task AReaderNeverSeesHalfOfAWrite()
    {
        var first = new DateTime(2026, 1, 1, 1, 1, 1);
        var second = new DateTime(2027, 2, 2, 2, 2, 2);
        var input = new DateTimeInput(first);
        var cancellationToken = TestContext.Current.CancellationToken;

        var writer = Task.Run(
            () =>
            {
                for (var i = 0; i < 500_000; i++)
                {
                    input.DateAndTime = i % 2 == 0 ? second : first;
                }
            },
            cancellationToken);

        var reader = Task.Run(
            () =>
            {
                var halves = 0;
                while (!writer.IsCompleted)
                {
                    var read = input.DateAndTime;
                    if (read != first && read != second)
                    {
                        halves++;
                    }
                }

                return halves;
            },
            cancellationToken);

        await writer;
        Assert.Equal(0, await reader);
    }

    [Fact]
    public async Task AReaderOfTheTime_NeverSeesAValueThatWasNotWritten()
    {
        // Between null and a time, a half-finished write reads as midnight.
        var input = new DateTimeInput();

        var notWritten = await CountWhileWriting(
            () =>
            {
                input.TimeAsTimeOnly = SomeTime;
                input.TimeAsTimeOnly = null;
            },
            () => input.TimeAsTimeOnly == TimeOnly.MinValue);

        Assert.Equal(0, notWritten);
    }

    private static async Task<int> CountWhileWriting(Action write, Func<bool> isWrong)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var writer = Task.Run(
            () =>
            {
                for (var i = 0; i < 2_000_000; i++)
                {
                    write();
                }
            },
            cancellationToken);

        var reader = Task.Run(
            () =>
            {
                var wrong = 0;
                while (!writer.IsCompleted)
                {
                    if (isWrong())
                    {
                        wrong++;
                    }
                }

                return wrong;
            },
            cancellationToken);

        await writer;
        return await reader;
    }

    [Fact]
    public void TheImplicitConversion_IsDateAndTime()
    {
        DateTime? converted = new DateTimeInput(SomeDateTime);

        Assert.Equal(SomeDateTime, converted);
    }

    [Fact]
    public void TheImplicitConversion_OfAnEmptyInput_IsNull()
    {
        DateTime? converted = new DateTimeInput();

        Assert.Null(converted);
    }

    [Fact]
    public void TheImplicitConversion_OfNull_Throws()
    {
        DateTimeInput input = null!;

        Assert.Throws<NullReferenceException>(() => (DateTime?)input);
    }

    [Fact]
    public void AnInput_ComparesWithADateTimeByValue()
    {
        var input = new DateTimeInput(SomeDateTime);

        var equal = input == SomeDateTime;
        var earlier = input < SomeDateTime.AddDays(1);
        var later = SomeDateTime.AddDays(1) > input;

        Assert.True(equal);
        Assert.True(earlier);
        Assert.True(later);
    }

    [Fact]
    public void AnEmptyInput_IsNeitherEarlierNorLaterThanADateTime()
    {
        var input = new DateTimeInput();

        var earlier = input < SomeDateTime;
        var notEarlier = input >= SomeDateTime;

        Assert.False(earlier);
        Assert.False(notEarlier);
    }

    [Fact]
    public void ToString_IsDateAndTimes()
    {
        var input = new DateTimeInput(SomeDateTime);

        Assert.Equal(SomeDateTime.ToString(), input.ToString());
    }

    [Fact]
    public void ToString_OfAnEmptyInput_IsEmpty()
    {
        Assert.Equal(string.Empty, new DateTimeInput().ToString());
    }
}
