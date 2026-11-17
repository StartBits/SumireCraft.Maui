using SumireCraft.DateTimes;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.DateTimes;

// A picker from another library, given a DateTimeInput by the source generator the way an
// application would give it one: an attribute on a partial class derived from the picker. Each type
// of date or time property is joined to the part of the input in that type. VendorPicker below
// stands in for the other library's picker.
public class GeneratedPickerTests
{
    private static readonly DateOnly SomeDate = new(2026, 11, 17);
    private static readonly TimeOnly SomeTime = new(10, 30, 15);
    private static readonly DateTime SomeDateTime = SomeDate.ToDateTime(SomeTime);

    private static readonly DateOnly OtherDate = new(1999, 12, 31);
    private static readonly TimeOnly OtherTime = new(23, 59, 0);

    // What every generated picker has, shown on one of them.

    [Fact]
    public void ANewPicker_HasAnEmptyInput()
    {
        var picker = new DateOnlyPicker();

        Assert.NotNull(picker.DateTimeInput);
        Assert.Null(picker.DateTimeInput.DateAndTime);
    }

    // A picker of its own starts at a value of its choosing, today say, which the input does not
    // hold; picked again unchanged, it would never reach the input. So every generated picker starts
    // as its input does, empty.

    [Fact]
    public void ANewPicker_ShowsItsEmptyInputRatherThanItsOwnStart()
    {
        Assert.Null(new DateOnlyPicker().SelectedDateOnly);
        Assert.Null(new DateTimeDatePicker().SelectedDateTime);
        Assert.Null(new TimeSpanPicker().SelectedTimeSpan);
        Assert.Null(new TimeOnlyPicker().SelectedTimeOnly);
        Assert.Null(new DateTimeTimePicker().SelectedDateTime);
    }

    [Fact]
    public void APickerWithAConstructorOfItsOwn_StartsEmptyByCallingThis()
    {
        var picker = new NamedDateOnlyPicker("start");

        Assert.Null(picker.SelectedDateOnly);
        Assert.Equal("start", picker.StyleId);
    }

    // What every picker of a kind starts with, an application's common settings, goes in OnCreated,
    // which the generated constructor calls once the picker is empty.

    [Fact]
    public void OnCreated_IsCalledByTheConstructorOnceThePickerIsEmpty()
    {
        var picker = new SetUpTimeOnlyPicker();

        Assert.Equal("set up", picker.StyleId);
        Assert.Null(picker.SeenAtCreation);
    }

    [Fact]
    public void TwoPickers_DoNotShareTheirFirstInput()
    {
        Assert.NotSame(new DateOnlyPicker().DateTimeInput, new DateOnlyPicker().DateTimeInput);
    }

    [Fact]
    public void WritingNull_GivesANewEmptyInputAndEmptiesThePicker()
    {
        var given = new DateTimeInput(SomeDateTime);
        var picker = new DateOnlyPicker { DateTimeInput = given };

        picker.DateTimeInput = null!;

        Assert.NotSame(given, picker.DateTimeInput);
        Assert.Null(picker.DateTimeInput.DateAndTime);
        Assert.Null(picker.SelectedDateOnly);
        Assert.Equal(SomeDateTime, given.DateAndTime);
    }

    [Fact]
    public void TheInputGiven_IsTheOneKept()
    {
        var given = new DateTimeInput(SomeDateTime);

        var picker = new DateOnlyPicker { DateTimeInput = given };

        Assert.Same(given, picker.DateTimeInput);
    }

    [Fact]
    public void TheInput_BindsBothWaysByDefault()
    {
        Assert.Equal(BindingMode.TwoWay, DateOnlyPicker.DateTimeInputProperty.DefaultBindingMode);
    }

    [Fact]
    public void BoundToNothing_ThePickerGivesItsOwnInputToTheSource()
    {
        var model = new Model { Start = null };
        var picker = new DateOnlyPicker { BindingContext = model };

        picker.SetBinding(DateOnlyPicker.DateTimeInputProperty, static (Model m) => m.Start);

        Assert.NotNull(model.Start);
        Assert.Same(model.Start, picker.DateTimeInput);
    }

    // XAML read at run time finds the generated property, as XamlC does when the application is
    // built. XAML generated as C# while the application is built does not: two source generators do
    // not see what the other writes, which the documentation says.

    [Fact]
    public void AGeneratedPicker_IsBoundInXaml()
    {
        const string Xaml = """
            <ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:t="clr-namespace:SumireCraft.Maui.UnitTests.DateTimes;assembly=SumireCraft.Maui.UnitTests">
              <t:DateOnlyPicker DateTimeInput="{Binding Start}" />
            </ContentView>
            """;
        var given = new DateTimeInput(SomeDateTime);
        var view = new ContentView().LoadFromXaml(Xaml);

        view.BindingContext = new Model { Start = given };

        var picker = Assert.IsType<DateOnlyPicker>(view.Content);
        Assert.Same(given, picker.DateTimeInput);
        Assert.Equal(SomeDate, picker.SelectedDateOnly);
    }

    [Fact]
    public void APropertyOtherThanTheDate_LeavesTheInputAlone()
    {
        var picker = new DateOnlyPicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        picker.SelectedTimeOnly = OtherTime;
        picker.SelectedDateTime = OtherDate.ToDateTime(OtherTime);

        Assert.Equal(SomeDateTime, picker.DateTimeInput.DateAndTime);
    }

    // A date as DateOnly?.

    [Fact]
    public void ADateOnlyPicker_ShowsTheInputsDate()
    {
        var picker = new DateOnlyPicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        Assert.Equal(SomeDate, picker.SelectedDateOnly);
    }

    [Fact]
    public void ADateOnlyPicker_WritesWhatIsPickedAndLeavesTheTime()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new DateOnlyPicker { DateTimeInput = input };

        picker.SelectedDateOnly = OtherDate;

        Assert.Equal(OtherDate, input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    [Fact]
    public void ADateOnlyPicker_WritesAnEmptyDate()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new DateOnlyPicker { DateTimeInput = input };

        picker.SelectedDateOnly = null;

        Assert.Null(input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    // A date as DateTime?.

    [Fact]
    public void ADateTimeDatePicker_ShowsTheInputsDate()
    {
        var picker = new DateTimeDatePicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        Assert.Equal(SomeDate.ToDateTime(TimeOnly.MinValue), picker.SelectedDateTime);
    }

    [Fact]
    public void ADateTimeDatePicker_WritesWhatIsPickedAndLeavesTheTime()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new DateTimeDatePicker { DateTimeInput = input };

        picker.SelectedDateTime = OtherDate.ToDateTime(OtherTime);

        Assert.Equal(OtherDate, input.DateAsDateOnly);
        Assert.Equal(SomeTime, input.TimeAsTimeOnly);
    }

    // A time as TimeSpan?.

    [Fact]
    public void ATimeSpanPicker_ShowsTheInputsTime()
    {
        var picker = new TimeSpanPicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        Assert.Equal(SomeTime.ToTimeSpan(), picker.SelectedTimeSpan);
    }

    [Fact]
    public void ATimeSpanPicker_WritesWhatIsPickedAndLeavesTheDate()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new TimeSpanPicker { DateTimeInput = input };

        picker.SelectedTimeSpan = OtherTime.ToTimeSpan();

        Assert.Equal(OtherTime, input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    // A time as TimeOnly?.

    [Fact]
    public void ATimeOnlyPicker_ShowsTheInputsTime()
    {
        var picker = new TimeOnlyPicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        Assert.Equal(SomeTime, picker.SelectedTimeOnly);
    }

    [Fact]
    public void ATimeOnlyPicker_WritesWhatIsPickedAndLeavesTheDate()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new TimeOnlyPicker { DateTimeInput = input };

        picker.SelectedTimeOnly = OtherTime;

        Assert.Equal(OtherTime, input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    [Fact]
    public void ATimeOnlyPicker_WritesAnEmptyTime()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new TimeOnlyPicker { DateTimeInput = input };

        picker.SelectedTimeOnly = null;

        Assert.Null(input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    // A time as DateTime?, whose date means nothing.

    [Fact]
    public void ADateTimeTimePicker_ShowsTheInputsTime()
    {
        var picker = new DateTimeTimePicker { DateTimeInput = new DateTimeInput(SomeDateTime) };

        Assert.Equal(SomeTime, TimeOnly.FromDateTime(picker.SelectedDateTime!.Value));
    }

    [Fact]
    public void ADateTimeTimePicker_WritesWhatIsPickedAndLeavesTheDate()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new DateTimeTimePicker { DateTimeInput = input };

        picker.SelectedDateTime = OtherDate.ToDateTime(OtherTime);

        Assert.Equal(OtherTime, input.TimeAsTimeOnly);
        Assert.Equal(SomeDate, input.DateAsDateOnly);
    }

    internal sealed class Model
    {
        public DateTimeInput? Start { get; set; }
    }
}

// Stands in for a picker from another library: a view with a date or a time property of each type
// the generator accepts, raising a change as any bindable property does. Each starts at a value of
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
}

[DateTimeInputDate(nameof(SelectedDateOnly))]
public partial class DateOnlyPicker : VendorPicker;

[DateTimeInputDate(nameof(SelectedDateTime))]
public partial class DateTimeDatePicker : VendorPicker;

[DateTimeInputTime(nameof(SelectedTimeSpan))]
public partial class TimeSpanPicker : VendorPicker;

[DateTimeInputTime(nameof(SelectedTimeOnly))]
public partial class TimeOnlyPicker : VendorPicker;

[DateTimeInputTime(nameof(SelectedDateTime))]
public partial class DateTimeTimePicker : VendorPicker;

// Settings of its own in OnCreated, and what the picker held when it ran.
[DateTimeInputTime(nameof(SelectedTimeOnly))]
public partial class SetUpTimeOnlyPicker : VendorPicker
{
    public TimeOnly? SeenAtCreation { get; private set; } = new TimeOnly(1, 2);

    partial void OnCreated()
    {
        StyleId = "set up";
        SeenAtCreation = SelectedTimeOnly;
    }
}

// A constructor of its own, which starts from the generated one.
[DateTimeInputDate(nameof(SelectedDateOnly))]
public partial class NamedDateOnlyPicker : VendorPicker
{
    public NamedDateOnlyPicker(string name)
        : this()
    {
        StyleId = name;
    }
}
