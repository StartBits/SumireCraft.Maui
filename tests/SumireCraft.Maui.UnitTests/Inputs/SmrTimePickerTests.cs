using SumireCraft.DateTimes;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Inputs;

// SmrTimePicker's DateTimeInput is SmrDateTimeInput.Time written shorter: one value, joined the same
// way, whose behaviour SmrDateTimeInputTests pins down. What it adds is a picker that starts empty.
[Collection(XamlLoadingCollection.Name)]
public class SmrTimePickerTests
{
    private static readonly DateTime SomeDateTime = new(2026, 11, 13, 10, 30, 15);

    [Fact]
    public void ANewPicker_ShowsNoTime()
    {
        var picker = new SmrTimePicker();

        Assert.Null(picker.Time);
        Assert.Null(picker.DateTimeInput);
    }

    [Fact]
    public void TheInput_IsTheOneTheAttachedPropertyHolds()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new SmrTimePicker { DateTimeInput = input };

        Assert.Same(SmrDateTimeInput.TimeProperty, SmrTimePicker.DateTimeInputProperty);
        Assert.Same(input, SmrDateTimeInput.GetTime(picker));
        Assert.Equal(SomeDateTime.TimeOfDay, picker.Time);
    }

    [Fact]
    public void PickingATime_WritesItIntoTheInput()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new SmrTimePicker { DateTimeInput = input };

        picker.Time = new TimeSpan(8, 15, 0);

        Assert.Equal(new TimeOnly(8, 15), input.TimeAsTimeOnly);
        Assert.Equal(DateOnly.FromDateTime(SomeDateTime), input.DateAsDateOnly);
    }

    [Fact]
    public void InXaml_TheInputIsBoundByItsShortName()
    {
        const string Xaml = """
            <ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                         xmlns:smr="https://github.com/startbits/SumireCraft.Maui">
              <smr:SmrTimePicker DateTimeInput="{Binding Start}" />
            </ContentView>
            """;
        var model = new Model();
        var view = new ContentView().LoadFromXaml(Xaml);

        view.BindingContext = model;

        var picker = Assert.IsType<SmrTimePicker>(view.Content);
        Assert.NotNull(model.Start);
        Assert.Same(model.Start, picker.DateTimeInput);
    }

    // Internal, not private: a binding written as a lambda is compiled by a generator, which cannot
    // reach a private type.
    internal sealed class Model
    {
        public DateTimeInput? Start { get; set; }
    }
}
