using SumireCraft.DateTimes;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.DateTimes;

// What a picker joined to an input shows of it, what it writes back, and what it does about an
// input that is missing: .NET MAUI's own pickers, and one from another library, which VendorPicker
// below stands in for. What the platform's picker then draws for an empty date or time is the UI
// tests'.
public class SmrDateTimeInputTests
{
    private static readonly DateOnly SomeDate = new(2026, 11, 13);
    private static readonly TimeOnly SomeTime = new(10, 30, 15);
    private static readonly DateTime SomeDateTime = SomeDate.ToDateTime(SomeTime);
    private static readonly DateTime SomeMidnight = SomeDate.ToDateTime(TimeOnly.MinValue);

    // A picker is joined to an input only when given one; until then it is a picker like any other.

    [Fact]
    public void APickerGivenNoInput_IsLeftAlone()
    {
        var picker = new DatePicker();
        var today = picker.Date;

        Assert.Null(SmrDateTimeInput.GetDate(picker));
        Assert.Equal(today, picker.Date);
    }

    [Fact]
    public void TheInputs_BindBothWaysByDefault()
    {
        Assert.Equal(BindingMode.TwoWay, SmrDateTimeInput.DateProperty.DefaultBindingMode);
        Assert.Equal(BindingMode.TwoWay, SmrDateTimeInput.TimeProperty.DefaultBindingMode);
    }

    [Fact]
    public void TheInputGiven_IsTheOneKept()
    {
        var given = new DateTimeInput(SomeDateTime);
        var picker = new DatePicker();

        SmrDateTimeInput.SetDate(picker, given);

        Assert.Same(given, SmrDateTimeInput.GetDate(picker));
    }

    // The input is always there to be written into, whatever the application gave: null is an empty
    // input of the picker's own, and a binding carries it back to a view model that had none.

    [Fact]
    public void WritingNull_GivesANewEmptyInputAndEmptiesThePicker()
    {
        var given = new DateTimeInput(SomeDateTime);
        var picker = Joined(new DatePicker(), given);

        SmrDateTimeInput.SetDate(picker, null);

        var input = SmrDateTimeInput.GetDate(picker);
        Assert.NotNull(input);
        Assert.NotSame(given, input);
        Assert.Null(input.DateAndTime);
        Assert.Null(picker.Date);
        Assert.Equal(SomeDateTime, given.DateAndTime);
    }

    [Fact]
    public void BoundToNothing_ThePickerGivesItsOwnInputToTheSource()
    {
        var model = new Model { Start = null };
        var picker = new DatePicker { BindingContext = model };

        picker.SetBinding(SmrDateTimeInput.DateProperty, static (Model m) => m.Start);

        Assert.NotNull(model.Start);
        Assert.Same(model.Start, SmrDateTimeInput.GetDate(picker));
        Assert.Null(picker.Date);
    }

    [Fact]
    public void BoundToAnInput_ThePickerLeavesTheSourceAlone()
    {
        var given = new DateTimeInput(SomeDateTime);
        var model = new Model { Start = given };
        var picker = new DatePicker { BindingContext = model };

        picker.SetBinding(SmrDateTimeInput.DateProperty, static (Model m) => m.Start);

        Assert.Same(given, model.Start);
        Assert.Equal(SomeDateTime, given.DateAndTime);
    }

    [Fact]
    public void TwoPickersBoundToNothing_DoNotShareAnInput()
    {
        var first = Joined(new DatePicker(), null);
        var second = Joined(new DatePicker(), null);

        Assert.NotSame(SmrDateTimeInput.GetDate(first), SmrDateTimeInput.GetDate(second));
    }

    // A date picker shows the input's date, empty included, and writes what is picked into it,
    // leaving the time alone.

    [Fact]
    public void ADatePicker_ShowsTheInputsDate()
    {
        var picker = Joined(new DatePicker(), new DateTimeInput(SomeDateTime));

        Assert.Equal(SomeMidnight, picker.Date);
    }

    [Fact]
    public void ADatePicker_ShowsAnInputWithoutADateEmpty()
    {
        var picker = Joined(new DatePicker(), new DateTimeInput { TimeAsTimeOnly = SomeTime });

        Assert.Null(picker.Date);
    }

    [Fact]
    public void ADatePicker_ShowsAnotherInputInPlaceOfTheFirst()
    {
        var picker = Joined(new DatePicker(), new DateTimeInput(SomeDateTime));

        SmrDateTimeInput.SetDate(picker, new DateTimeInput(new DateTime(2027, 1, 2)));

        Assert.Equal(new DateTime(2027, 1, 2), picker.Date);
    }

    [Fact]
    public void PickingADate_WritesTheDateAndKeepsTheTime()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = Joined(new DatePicker(), input);

        picker.Date = new DateTime(2027, 1, 2);

        Assert.Equal(new DateOnly(2027, 1, 2), input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void ClearingTheDate_EmptiesTheInputsDateAndKeepsTheTime()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = Joined(new DatePicker(), input);

        picker.Date = null;

        Assert.Null(input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void WhatIsPicked_GoesIntoTheInputOfTheMoment()
    {
        var first = new DateTimeInput(SomeDateTime);
        var second = new DateTimeInput();
        var picker = Joined(new DatePicker(), first);
        SmrDateTimeInput.SetDate(picker, second);

        picker.Date = new DateTime(2027, 1, 2);

        Assert.Equal(SomeDate, first.DateAsDateOnly);
        Assert.Equal(new DateOnly(2027, 1, 2), second.DateAsDateOnly);
    }

    [Fact]
    public void APickerNoLongerJoined_WritesNothing()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = Joined(new DatePicker(), input);
        picker.ClearValue(SmrDateTimeInput.DateProperty);

        picker.Date = new DateTime(2027, 1, 2);

        Assert.Null(SmrDateTimeInput.GetDate(picker));
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

        var picker = Joined(new DatePicker(), input);

        var expected = DateOnly.ParseExact(shown, "yyyy-MM-dd");
        Assert.Equal(expected.ToDateTime(TimeOnly.MinValue), picker.Date);
        Assert.Equal(expected, input.DateAsDateOnly);
        Assert.Equal(new TimeOnly(9, 0), input.TimeAsTimeOnly);
    }

    [Fact]
    public void ADateMovedToWhereThePickerAlreadyIs_IsMovedInTheInputToo()
    {
        var first = new DateTimeInput(new DateTime(1066, 10, 14, 9, 0, 0));
        var second = new DateTimeInput(new DateTime(1500, 1, 1, 9, 0, 0));
        var picker = Joined(new DatePicker(), first);

        SmrDateTimeInput.SetDate(picker, second);

        Assert.Equal(new DateOnly(1900, 1, 1), second.DateAsDateOnly);
        Assert.Equal(new TimeOnly(9, 0), second.TimeAsTimeOnly);
    }

    [Fact]
    public void InAWiderRange_TheDateStaysAsItWas()
    {
        var input = new DateTimeInput(new DateTime(1066, 10, 14, 9, 0, 0));

        var picker = Joined(new DatePicker { MinimumDate = new DateTime(1000, 1, 1) }, input);

        Assert.Equal(new DateTime(1066, 10, 14), picker.Date);
        Assert.Equal(new DateTime(1066, 10, 14, 9, 0, 0), input.DateAndTime);
    }

    // A time picker shows the input's time, and writes what is picked into it, leaving the date
    // alone; a date outside a date picker's range stays, as it shows none.

    [Fact]
    public void ATimePicker_ShowsTheInputsTime()
    {
        var picker = new TimePicker();
        SmrDateTimeInput.SetTime(picker, new DateTimeInput(SomeDateTime));

        Assert.Equal(SomeTime.ToTimeSpan(), picker.Time);
    }

    [Fact]
    public void ATimePicker_ShowsAnInputWithoutATimeEmpty()
    {
        var picker = new TimePicker();
        SmrDateTimeInput.SetTime(picker, new DateTimeInput { DateAsDateOnly = SomeDate });

        Assert.Null(picker.Time);
    }

    [Fact]
    public void PickingATime_WritesTheTimeAndKeepsTheDate()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new TimePicker();
        SmrDateTimeInput.SetTime(picker, input);

        picker.Time = new TimeSpan(8, 15, 0);

        Assert.Equal(new TimeOnly(8, 15), input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    [Fact]
    public void ATimePicker_LeavesADateOutsideADatePickersRangeAlone()
    {
        var input = new DateTimeInput(new DateTime(1066, 10, 14, 9, 0, 0));
        var picker = new TimePicker();

        SmrDateTimeInput.SetTime(picker, input);

        Assert.Equal(new DateTime(1066, 10, 14, 9, 0, 0), input.DateAndTime);
    }

    [Fact]
    public void ADatePickerAndATimePickerOnOneInput_FillItBetweenThem()
    {
        var input = new DateTimeInput();
        var date = Joined(new DatePicker(), input);
        var time = new TimePicker();
        SmrDateTimeInput.SetTime(time, input);

        date.Date = SomeMidnight;
        time.Time = SomeTime.ToTimeSpan();

        Assert.True(input.IsComplete);
        Assert.Equal(SomeDateTime, input.DateAndTime);
    }

    // A picker of another library is joined through the bindable property it holds its date or time
    // in, handed over as it is rather than named. Its type picks the part of the input it is joined
    // to; a type that cannot hold an empty date or time leaves the picker alone.

    [Fact]
    public void AVendorPicker_IsJoinedThroughTheDatePropertyItIsGiven()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new VendorPicker();

        SmrDateTimeInput.SetDateTarget(picker, VendorPicker.SelectedDateOnlyProperty);
        SmrDateTimeInput.SetDate(picker, input);

        Assert.Equal(SomeDate, picker.SelectedDateOnly);
        picker.SelectedDateOnly = new DateOnly(2027, 1, 2);
        Assert.Equal(new DateOnly(2027, 1, 2), input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void AVendorPicker_WithNoInputYet_StartsEmptyOnceGivenOne()
    {
        var picker = new VendorPicker();

        SmrDateTimeInput.SetDate(picker, new DateTimeInput());
        SmrDateTimeInput.SetDateTarget(picker, VendorPicker.SelectedDateOnlyProperty);

        Assert.Null(picker.SelectedDateOnly);
    }

    [Theory]
    [InlineData(nameof(VendorPicker.SelectedDateOnly), true)]
    [InlineData(nameof(VendorPicker.SelectedDateTime), true)]
    [InlineData(nameof(VendorPicker.SelectedTimeSpan), false)]
    [InlineData(nameof(VendorPicker.SelectedTimeOnly), false)]
    public void EachTypeOfProperty_TakesTheInputsPartInThatType(string property, bool isDate)
    {
        var (target, shown) = property switch
        {
            nameof(VendorPicker.SelectedDateOnly) => (VendorPicker.SelectedDateOnlyProperty, (object)SomeDate),
            nameof(VendorPicker.SelectedDateTime) => (VendorPicker.SelectedDateTimeProperty, SomeMidnight),
            nameof(VendorPicker.SelectedTimeSpan) => (VendorPicker.SelectedTimeSpanProperty, SomeTime.ToTimeSpan()),
            _ => (VendorPicker.SelectedTimeOnlyProperty, SomeTime),
        };
        var picker = new VendorPicker();
        var input = new DateTimeInput(SomeDateTime);

        if (isDate)
        {
            SmrDateTimeInput.SetDateTarget(picker, target);
            SmrDateTimeInput.SetDate(picker, input);
        }
        else
        {
            SmrDateTimeInput.SetTimeTarget(picker, target);
            SmrDateTimeInput.SetTime(picker, input);
        }

        Assert.Equal(shown, picker.GetValue(target));
    }

    [Fact]
    public void ATimeHeldAsADateTime_TakesTheTime()
    {
        var picker = new VendorPicker();
        var input = new DateTimeInput(SomeDateTime);
        SmrDateTimeInput.SetTimeTarget(picker, VendorPicker.SelectedDateTimeProperty);
        SmrDateTimeInput.SetTime(picker, input);

        Assert.Equal(SomeTime, TimeOnly.FromDateTime(picker.SelectedDateTime!.Value));

        picker.SelectedDateTime = new DateTime(1999, 12, 31, 8, 15, 0);
        Assert.Equal(new TimeOnly(8, 15), input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    // A property holds one part. One that holds a date and a time both is joined to the date: as the
    // time, it would carry a date of its own into the input's date.
    [Fact]
    public void OnePropertyGivenForTheDateAndTheTime_IsJoinedToTheDateAlone()
    {
        var picker = new VendorPicker();
        var input = new DateTimeInput(SomeDateTime);
        SmrDateTimeInput.SetDateTarget(picker, VendorPicker.SelectedDateTimeProperty);
        SmrDateTimeInput.SetTimeTarget(picker, VendorPicker.SelectedDateTimeProperty);

        SmrDateTimeInput.SetDate(picker, input);
        SmrDateTimeInput.SetTime(picker, input);

        Assert.Equal(SomeMidnight, picker.SelectedDateTime);
        Assert.Equal(SomeDateTime, input.DateAndTime);
        picker.SelectedDateTime = new DateTime(2027, 1, 2, 8, 15, 0);
        Assert.Equal(new DateOnly(2027, 1, 2), input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void APropertyThatCannotBeEmpty_LeavesThePickerAlone()
    {
        var picker = new VendorPicker();
        var input = new DateTimeInput(SomeDateTime);
        SmrDateTimeInput.SetDateTarget(picker, VendorPicker.PlainDateProperty);

        SmrDateTimeInput.SetDate(picker, input);

        Assert.Equal(new DateTime(2000, 1, 1), picker.PlainDate);
        picker.PlainDate = new DateTime(2027, 1, 2);
        Assert.Equal(SomeDateTime, input.DateAndTime);
    }

    [Fact]
    public void AnotherPropertyOfThePicker_LeavesTheInputAlone()
    {
        var picker = new VendorPicker();
        var input = new DateTimeInput(SomeDateTime);
        SmrDateTimeInput.SetDateTarget(picker, VendorPicker.SelectedDateOnlyProperty);
        SmrDateTimeInput.SetDate(picker, input);

        picker.SelectedDateTime = new DateTime(1999, 12, 31);

        Assert.Equal(SomeDateTime, input.DateAndTime);
    }

    // Where the date goes is said once, in a style for the picker's type, and every page joins the
    // picker by its input alone; in XAML, the property is handed over with x:Static.

    [Fact]
    public void ADateTargetFromAStyle_JoinsEveryPickerItReaches()
    {
        var style = new Style(typeof(VendorPicker))
        {
            Setters = { new Setter { Property = SmrDateTimeInput.DateTargetProperty, Value = VendorPicker.SelectedDateOnlyProperty } },
        };
        var picker = new VendorPicker { Style = style };

        SmrDateTimeInput.SetDate(picker, new DateTimeInput(SomeDateTime));

        Assert.Equal(SomeDate, picker.SelectedDateOnly);
    }

    [Fact]
    public void InXaml_TheTargetIsHandedOverWithXStatic()
    {
        const string Xaml = """
            <ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                         xmlns:smr="https://github.com/startbits/SumireCraft.Maui"
                         xmlns:t="clr-namespace:SumireCraft.Maui.UnitTests.DateTimes;assembly=SumireCraft.Maui.UnitTests">
              <t:VendorPicker smr:SmrDateTimeInput.DateTarget="{x:Static t:VendorPicker.SelectedDateOnlyProperty}"
                              smr:SmrDateTimeInput.Date="{Binding Start}" />
            </ContentView>
            """;
        var given = new DateTimeInput(SomeDateTime);
        var view = new ContentView().LoadFromXaml(Xaml);

        view.BindingContext = new Model { Start = given };

        var picker = Assert.IsType<VendorPicker>(view.Content);
        Assert.Same(given, SmrDateTimeInput.GetDate(picker));
        Assert.Equal(SomeDate, picker.SelectedDateOnly);
    }

    // A joined picker is let go of with its page: the library keeps nothing that holds it.

    [Fact]
    public async Task AJoinedPicker_IsLetGoOf()
    {
        var reference = JoinedAndForgotten();

        for (var i = 0; i < 3 && reference.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Yield();
        }

        Assert.False(reference.IsAlive);
    }

    // Not inlined, so that no local of the caller's keeps the picker reachable.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference JoinedAndForgotten() => new(Joined(new DatePicker(), new DateTimeInput(SomeDateTime)));

    private static DatePicker Joined(DatePicker picker, DateTimeInput? input)
    {
        SmrDateTimeInput.SetDate(picker, input);
        return picker;
    }

    // Internal, not private: a binding written as a lambda is compiled by a generator, which cannot
    // reach a private type.
    internal sealed class Model
    {
        public DateTimeInput? Start { get; set; }
    }
}

// Stands in for a picker from another library: a view with a date or a time property of each type
// a part can be held in, raising a change as any bindable property does. Each starts at a value of
// its own, as a picker's date starts at today.
public class VendorPicker : ContentView
{
    public static readonly BindableProperty SelectedDateOnlyProperty =
        BindableProperty.Create(nameof(SelectedDateOnly), typeof(DateOnly?), typeof(VendorPicker), (DateOnly?)new DateOnly(2000, 1, 1));

    public static readonly BindableProperty SelectedDateTimeProperty =
        BindableProperty.Create(nameof(SelectedDateTime), typeof(DateTime?), typeof(VendorPicker), (DateTime?)new DateTime(2000, 1, 1, 12, 0, 0));

    public static readonly BindableProperty SelectedTimeSpanProperty =
        BindableProperty.Create(nameof(SelectedTimeSpan), typeof(TimeSpan?), typeof(VendorPicker), (TimeSpan?)TimeSpan.FromHours(12));

    public static readonly BindableProperty SelectedTimeOnlyProperty =
        BindableProperty.Create(nameof(SelectedTimeOnly), typeof(TimeOnly?), typeof(VendorPicker), (TimeOnly?)new TimeOnly(12, 0));

    public static readonly BindableProperty PlainDateProperty =
        BindableProperty.Create(nameof(PlainDate), typeof(DateTime), typeof(VendorPicker), new DateTime(2000, 1, 1));

    public DateOnly? SelectedDateOnly
    {
        get => (DateOnly?)GetValue(SelectedDateOnlyProperty);
        set => SetValue(SelectedDateOnlyProperty, value);
    }

    public DateTime? SelectedDateTime
    {
        get => (DateTime?)GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public TimeSpan? SelectedTimeSpan
    {
        get => (TimeSpan?)GetValue(SelectedTimeSpanProperty);
        set => SetValue(SelectedTimeSpanProperty, value);
    }

    public TimeOnly? SelectedTimeOnly
    {
        get => (TimeOnly?)GetValue(SelectedTimeOnlyProperty);
        set => SetValue(SelectedTimeOnlyProperty, value);
    }

    public DateTime PlainDate
    {
        get => (DateTime)GetValue(PlainDateProperty);
        set => SetValue(PlainDateProperty, value);
    }
}
