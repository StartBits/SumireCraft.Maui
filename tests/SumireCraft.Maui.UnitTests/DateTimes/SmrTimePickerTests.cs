using SumireCraft.DateTimes;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.DateTimes;

// What a time picker shows of its input, what it writes back, and what it does about an input
// that is missing. What the platform's picker then draws for an empty time is the UI tests'.
public class SmrTimePickerTests
{
    private static readonly DateOnly SomeDate = new(2026, 11, 13);
    private static readonly TimeOnly SomeTime = new(10, 30, 15);
    private static readonly DateTime SomeDateTime = SomeDate.ToDateTime(SomeTime);

    // The input is always there to be written into, whatever the application gave.

    [Fact]
    public void ANewPicker_HasAnEmptyInputAndShowsNoTime()
    {
        var picker = new SmrTimePicker();

        Assert.NotNull(picker.DateTimeInput);
        Assert.Null(picker.DateTimeInput.DateAndTime);
        Assert.Null(picker.Time);
    }

    [Fact]
    public void TwoPickers_DoNotShareTheirFirstInput()
    {
        Assert.NotSame(new SmrTimePicker().DateTimeInput, new SmrTimePicker().DateTimeInput);
    }

    [Fact]
    public void WritingNull_GivesANewEmptyInput()
    {
        var given = new DateTimeInput(SomeDateTime);
        var picker = new SmrTimePicker { DateTimeInput = given };

        picker.DateTimeInput = null!;

        Assert.NotNull(picker.DateTimeInput);
        Assert.NotSame(given, picker.DateTimeInput);
        Assert.Null(picker.DateTimeInput.DateAndTime);
        Assert.Null(picker.Time);
        Assert.Equal(SomeDateTime, given.DateAndTime);
    }

    [Fact]
    public void TheInputGiven_IsTheOneKept()
    {
        var given = new DateTimeInput(SomeDateTime);

        var picker = new SmrTimePicker { DateTimeInput = given };

        Assert.Same(given, picker.DateTimeInput);
    }

    [Fact]
    public void TheInput_BindsBothWaysByDefault()
    {
        Assert.Equal(BindingMode.TwoWay, SmrTimePicker.DateTimeInputProperty.DefaultBindingMode);
    }

    [Fact]
    public void BoundToNothing_ThePickerGivesItsOwnInputToTheSource()
    {
        var model = new Model { Start = null };
        var picker = new SmrTimePicker { BindingContext = model };

        picker.SetBinding(SmrTimePicker.DateTimeInputProperty, static (Model m) => m.Start);

        Assert.NotNull(model.Start);
        Assert.Same(model.Start, picker.DateTimeInput);
    }

    [Fact]
    public void BoundToAnInput_ThePickerLeavesTheSourceAlone()
    {
        var given = new DateTimeInput(SomeDateTime);
        var model = new Model { Start = given };
        var picker = new SmrTimePicker { BindingContext = model };

        picker.SetBinding(SmrTimePicker.DateTimeInputProperty, static (Model m) => m.Start);

        Assert.Same(given, model.Start);
        Assert.Equal(SomeDateTime, given.DateAndTime);
    }

    // What is shown is the input's time, empty included.

    [Fact]
    public void TheInputsTime_IsShown()
    {
        var picker = new SmrTimePicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        Assert.Equal(SomeTime.ToTimeSpan(), picker.Time);
    }

    [Fact]
    public void AnInputWithoutATime_IsShownEmpty()
    {
        var picker = new SmrTimePicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        picker.DateTimeInput = new DateTimeInput { DateAsDateOnly = SomeDate };

        Assert.Null(picker.Time);
    }

    [Fact]
    public void AnotherInput_IsShownInPlaceOfTheFirst()
    {
        var picker = new SmrTimePicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        picker.DateTimeInput = new DateTimeInput(new DateTime(2027, 1, 2, 8, 15, 0));

        Assert.Equal(new TimeSpan(8, 15, 0), picker.Time);
    }

    // What is picked goes into the input's time, and the date is left alone.

    [Fact]
    public void PickingATime_WritesTheTimeAndKeepsTheDate()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new SmrTimePicker { DateTimeInput = input };

        picker.Time = new TimeSpan(8, 15, 0);

        Assert.Equal(new TimeOnly(8, 15), input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    [Fact]
    public void PickingATime_LeavesAnEmptyDateEmpty()
    {
        var picker = new SmrTimePicker();

        picker.Time = SomeTime.ToTimeSpan();

        Assert.Equal(SomeTime, picker.DateTimeInput.TimeAsTimeOnly);
        Assert.Null(picker.DateTimeInput.DateAsDateOnly);
    }

    [Fact]
    public void ClearingTheTime_EmptiesTheInputsTimeAndKeepsTheDate()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new SmrTimePicker { DateTimeInput = input };

        picker.Time = null;

        Assert.Null(input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    [Fact]
    public void WhatIsPicked_GoesIntoTheInputOfTheMoment()
    {
        var first = new DateTimeInput(SomeDateTime);
        var second = new DateTimeInput();
        var picker = new SmrTimePicker { DateTimeInput = first };
        picker.DateTimeInput = second;

        picker.Time = new TimeSpan(8, 15, 0);

        Assert.Equal(SomeTime, first.TimeAsTimeOnly);
        Assert.Equal(new TimeOnly(8, 15), second.TimeAsTimeOnly);
    }

    // A time picker shows no date, so it moves none: a date outside a date picker's range stays.

    [Fact]
    public void ADateOutsideADatePickersRange_StaysAsItWas()
    {
        var input = new DateTimeInput(new DateTime(1066, 10, 14, 9, 0, 0));

        _ = new SmrTimePicker { DateTimeInput = input };

        Assert.Equal(new DateTime(1066, 10, 14, 9, 0, 0), input.DateAndTime);
    }

    [Fact]
    public void AStyleForEveryTimePicker_ReachesThePicker()
    {
        var page = new Grid();
        page.Resources.Add(new Style(typeof(TimePicker))
        {
            Setters = { new Setter { Property = TimePicker.TextColorProperty, Value = Colors.Teal } },
        });
        var plain = new TimePicker();
        var picker = new SmrTimePicker();

        page.Add(plain);
        page.Add(picker);

        Assert.Equal(Colors.Teal, plain.TextColor);
        Assert.Equal(Colors.Teal, picker.TextColor);
    }

    [Fact]
    public void AnImplicitStyleOfItsOwn_WinsOverTheOneForEveryTimePicker()
    {
        var page = new Grid();
        page.Resources.Add(Colored(typeof(TimePicker), Colors.Teal));
        page.Resources.Add(Colored(typeof(SmrTimePicker), Colors.Orange));
        var picker = new SmrTimePicker();

        page.Add(picker);

        Assert.Equal(Colors.Orange, picker.TextColor);
    }

    // As on a plain TimePicker: a Style set on the picker goes on top of the implicit one, and what
    // it leaves unset still comes from there.
    [Fact]
    public void AStyleOfItsOwn_LeavesWhatItDoesNotSetToTheOneForEveryTimePicker()
    {
        var page = new Grid();
        page.Resources.Add(new Style(typeof(TimePicker))
        {
            Setters = { new Setter { Property = TimePicker.FontSizeProperty, Value = 22d } },
        });
        var plain = new TimePicker { Style = Colored(typeof(TimePicker), Colors.Orange) };
        var picker = new SmrTimePicker { Style = Colored(typeof(SmrTimePicker), Colors.Orange) };

        page.Add(plain);
        page.Add(picker);

        Assert.Equal(22d, plain.FontSize);
        Assert.Equal(22d, picker.FontSize);
        Assert.Equal(Colors.Orange, picker.TextColor);
    }

    private static Style Colored(Type type, Color color) => new(type)
    {
        Setters = { new Setter { Property = TimePicker.TextColorProperty, Value = color } },
    };

    // Internal, not private: a binding written as a lambda is compiled by a generator, which cannot
    // reach a private type.
    internal sealed class Model
    {
        public DateTimeInput? Start { get; set; }
    }
}
