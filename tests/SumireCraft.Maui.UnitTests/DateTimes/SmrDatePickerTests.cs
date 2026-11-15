using SumireCraft.DateTimes;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.DateTimes;

// What a date picker shows of its input, what it writes back, and what it does about an input
// that is missing. What the platform's picker then draws for an empty date is the UI tests'.
public class SmrDatePickerTests
{
    private static readonly DateOnly SomeDate = new(2026, 11, 13);
    private static readonly TimeOnly SomeTime = new(10, 30, 15);
    private static readonly DateTime SomeDateTime = SomeDate.ToDateTime(SomeTime);
    private static readonly DateTime SomeMidnight = SomeDate.ToDateTime(TimeOnly.MinValue);

    // The input is always there to be written into, whatever the application gave.

    [Fact]
    public void ANewPicker_HasAnEmptyInputAndShowsNoDate()
    {
        var picker = new SmrDatePicker();

        Assert.NotNull(picker.DateTimeInput);
        Assert.Null(picker.DateTimeInput.DateAndTime);
        Assert.Null(picker.Date);
    }

    [Fact]
    public void TwoPickers_DoNotShareTheirFirstInput()
    {
        Assert.NotSame(new SmrDatePicker().DateTimeInput, new SmrDatePicker().DateTimeInput);
    }

    [Fact]
    public void WritingNull_GivesANewEmptyInput()
    {
        var given = new DateTimeInput(SomeDateTime);
        var picker = new SmrDatePicker { DateTimeInput = given };

        picker.DateTimeInput = null!;

        Assert.NotNull(picker.DateTimeInput);
        Assert.NotSame(given, picker.DateTimeInput);
        Assert.Null(picker.DateTimeInput.DateAndTime);
        Assert.Null(picker.Date);
        Assert.Equal(SomeDateTime, given.DateAndTime);
    }

    [Fact]
    public void TheInputGiven_IsTheOneKept()
    {
        var given = new DateTimeInput(SomeDateTime);

        var picker = new SmrDatePicker { DateTimeInput = given };

        Assert.Same(given, picker.DateTimeInput);
    }

    // A binding writes back by default, so that the input a picker made for a view model that had
    // none reaches the view model, which could not read what was entered otherwise.

    [Fact]
    public void TheInput_BindsBothWaysByDefault()
    {
        Assert.Equal(BindingMode.TwoWay, SmrDatePicker.DateTimeInputProperty.DefaultBindingMode);
    }

    [Fact]
    public void BoundToNothing_ThePickerGivesItsOwnInputToTheSource()
    {
        var model = new Model { Start = null };
        var picker = new SmrDatePicker { BindingContext = model };

        picker.SetBinding(SmrDatePicker.DateTimeInputProperty, static (Model m) => m.Start);

        Assert.NotNull(model.Start);
        Assert.Same(model.Start, picker.DateTimeInput);
    }

    [Fact]
    public void BoundToAnInput_ThePickerLeavesTheSourceAlone()
    {
        var given = new DateTimeInput(SomeDateTime);
        var model = new Model { Start = given };
        var picker = new SmrDatePicker { BindingContext = model };

        picker.SetBinding(SmrDatePicker.DateTimeInputProperty, static (Model m) => m.Start);

        Assert.Same(given, model.Start);
        Assert.Same(given, picker.DateTimeInput);
        Assert.Equal(SomeDateTime, given.DateAndTime);
    }

    // What is shown is the input's date, empty included.

    [Fact]
    public void TheInputsDate_IsShown()
    {
        var picker = new SmrDatePicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        Assert.Equal(SomeMidnight, picker.Date);
    }

    [Fact]
    public void AnInputWithoutADate_IsShownEmpty()
    {
        var picker = new SmrDatePicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        picker.DateTimeInput = new DateTimeInput { TimeAsTimeOnly = SomeTime };

        Assert.Null(picker.Date);
    }

    // The input raises no notification, so code that wants other values shown gives another input.

    [Fact]
    public void AnotherInput_IsShownInPlaceOfTheFirst()
    {
        var picker = new SmrDatePicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        picker.DateTimeInput = new DateTimeInput(new DateTime(2027, 1, 2));

        Assert.Equal(new DateTime(2027, 1, 2), picker.Date);
    }

    // What is picked goes into the input's date, and the time is left alone.

    [Fact]
    public void PickingADate_WritesTheDateAndKeepsTheTime()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new SmrDatePicker { DateTimeInput = input };

        picker.Date = new DateTime(2027, 1, 2);

        Assert.Equal(new DateOnly(2027, 1, 2), input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void PickingADate_LeavesAnEmptyTimeEmpty()
    {
        var picker = new SmrDatePicker();

        picker.Date = SomeMidnight;

        Assert.Equal(SomeDate, picker.DateTimeInput.DateAsDateOnly);
        Assert.Null(picker.DateTimeInput.TimeAsTimeOnly);
    }

    [Fact]
    public void ClearingTheDate_EmptiesTheInputsDateAndKeepsTheTime()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new SmrDatePicker { DateTimeInput = input };

        picker.Date = null;

        Assert.Null(input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void WhatIsPicked_GoesIntoTheInputThePickerMade()
    {
        var model = new Model { Start = null };
        var picker = new SmrDatePicker { BindingContext = model };
        picker.SetBinding(SmrDatePicker.DateTimeInputProperty, static (Model m) => m.Start);

        picker.Date = SomeMidnight;

        Assert.Equal(SomeDate, model.Start?.DateAsDateOnly);
    }

    [Fact]
    public void WhatIsPicked_GoesIntoTheInputOfTheMoment()
    {
        var first = new DateTimeInput(SomeDateTime);
        var second = new DateTimeInput();
        var picker = new SmrDatePicker { DateTimeInput = first };
        picker.DateTimeInput = second;

        picker.Date = new DateTime(2027, 1, 2);

        Assert.Equal(SomeDate, first.DateAsDateOnly);
        Assert.Equal(new DateOnly(2027, 1, 2), second.DateAsDateOnly);
    }

    // A date and a time are two pickers on one input.

    [Fact]
    public void ADatePickerAndATimePickerOnOneInput_FillItBetweenThem()
    {
        var input = new DateTimeInput();
        var date = new SmrDatePicker { DateTimeInput = input };
        var time = new SmrTimePicker { DateTimeInput = input };

        date.Date = SomeMidnight;
        time.Time = SomeTime.ToTimeSpan();

        Assert.True(input.IsComplete);
        Assert.Equal(SomeDateTime, input.DateAndTime);
    }

    // .NET MAUI's date picker moves a date outside its range to the nearest end of it, and writes
    // that back to whatever it is bound to. The input takes the date shown, so the two never
    // differ; the range is the application's to widen.

    [Theory]
    [InlineData(1066, "1900-01-01")]
    [InlineData(2500, "2100-12-31")]
    public void ADateOutsideThePickersRange_IsMovedInTheInputToo(int year, string shown)
    {
        var input = new DateTimeInput(new DateTime(year, 10, 14, 9, 0, 0));

        var picker = new SmrDatePicker { DateTimeInput = input };

        var expected = DateOnly.ParseExact(shown, "yyyy-MM-dd");
        Assert.Equal(expected.ToDateTime(TimeOnly.MinValue), picker.Date);
        Assert.Equal(expected, input.DateAsDateOnly);
        Assert.Equal(new TimeOnly(9, 0), input.TimeAsTimeOnly);
    }

    [Fact]
    public void InAWiderRange_TheDateStaysAsItWas()
    {
        var input = new DateTimeInput(new DateTime(1066, 10, 14, 9, 0, 0));

        var picker = new SmrDatePicker { MinimumDate = new DateTime(1000, 1, 1), DateTimeInput = input };

        Assert.Equal(new DateTime(1066, 10, 14), picker.Date);
        Assert.Equal(new DateTime(1066, 10, 14, 9, 0, 0), input.DateAndTime);
    }

    // A style an application wrote for every DatePicker -- the Styles.xaml of a new project has
    // one -- reaches this picker too, although .NET MAUI gives an implicit style to the exact type
    // alone.

    [Fact]
    public void AStyleForEveryDatePicker_ReachesThePicker()
    {
        var page = new Grid();
        page.Resources.Add(new Style(typeof(DatePicker))
        {
            Setters = { new Setter { Property = DatePicker.TextColorProperty, Value = Colors.Teal } },
        });
        var plain = new DatePicker();
        var picker = new SmrDatePicker();

        page.Add(plain);
        page.Add(picker);

        Assert.Equal(Colors.Teal, plain.TextColor);
        Assert.Equal(Colors.Teal, picker.TextColor);
    }

    [Fact]
    public void AStyleOfItsOwn_TakesThePlaceOfTheOneForEveryDatePicker()
    {
        var page = new Grid();
        page.Resources.Add(new Style(typeof(DatePicker))
        {
            Setters = { new Setter { Property = DatePicker.TextColorProperty, Value = Colors.Teal } },
        });
        var picker = new SmrDatePicker
        {
            Style = new Style(typeof(SmrDatePicker))
            {
                Setters = { new Setter { Property = DatePicker.TextColorProperty, Value = Colors.Orange } },
            },
        };

        page.Add(picker);

        Assert.Equal(Colors.Orange, picker.TextColor);
    }

    [Fact]
    public void AnImplicitStyleOfItsOwn_WinsOverTheOneForEveryDatePicker()
    {
        var page = new Grid();
        page.Resources.Add(Colored(typeof(DatePicker), Colors.Teal));
        page.Resources.Add(Colored(typeof(SmrDatePicker), Colors.Orange));
        var picker = new SmrDatePicker();

        page.Add(picker);

        Assert.Equal(Colors.Orange, picker.TextColor);
    }

    // As on a plain DatePicker: a Style set on the picker goes on top of the implicit one, and what
    // it leaves unset still comes from there.
    [Fact]
    public void AStyleOfItsOwn_LeavesWhatItDoesNotSetToTheOneForEveryDatePicker()
    {
        var page = new Grid();
        page.Resources.Add(new Style(typeof(DatePicker))
        {
            Setters = { new Setter { Property = DatePicker.FontSizeProperty, Value = 22d } },
        });
        var plain = new DatePicker { Style = Colored(typeof(DatePicker), Colors.Orange) };
        var picker = new SmrDatePicker { Style = Colored(typeof(SmrDatePicker), Colors.Orange) };

        page.Add(plain);
        page.Add(picker);

        Assert.Equal(22d, plain.FontSize);
        Assert.Equal(22d, picker.FontSize);
        Assert.Equal(Colors.Orange, picker.TextColor);
    }

    // A dictionary the application gives the picker may be shared, and is not written to: the
    // picker goes without the style for every DatePicker instead.
    [Fact]
    public void ADictionaryGivenToThePicker_IsNotWrittenTo()
    {
        var page = new Grid();
        page.Resources.Add(Colored(typeof(DatePicker), Colors.Teal));
        var given = new ResourceDictionary();
        var picker = new SmrDatePicker { Resources = given };
        page.Add(picker);

        page.Resources[typeof(DatePicker).FullName] = Colored(typeof(DatePicker), Colors.Orange);

        Assert.Empty(given);
    }

    private static Style Colored(Type type, Color color) => new(type)
    {
        Setters = { new Setter { Property = DatePicker.TextColorProperty, Value = color } },
    };

    // Internal, not private: a binding written as a lambda is compiled by a generator, which cannot
    // reach a private type.
    internal sealed class Model
    {
        public DateTimeInput? Start { get; set; }
    }
}
