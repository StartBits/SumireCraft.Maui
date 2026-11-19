using SumireCraft.DateTimes;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Inputs;

// SmrDatePicker's DateTimeInput is SmrDateTimeInput.Date written shorter: one value, joined the same
// way, whose behaviour SmrDateTimeInputTests pins down. What it adds is a picker that starts empty.
[Collection(XamlLoadingCollection.Name)]
public class SmrDatePickerTests
{
    private static readonly DateTime SomeDateTime = new(2026, 11, 13, 10, 30, 15);

    [Fact]
    public void ANewPicker_ShowsNoDate()
    {
        var picker = new SmrDatePicker();

        Assert.Null(picker.Date);
        Assert.Null(picker.DateTimeInput);
    }

    [Fact]
    public void TheInput_IsTheOneTheAttachedPropertyHolds()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new SmrDatePicker { DateTimeInput = input };

        Assert.Same(SmrDateTimeInput.DateProperty, SmrDatePicker.DateTimeInputProperty);
        Assert.Same(input, SmrDateTimeInput.GetDate(picker));
        Assert.Equal(SomeDateTime.Date, picker.Date);
    }

    [Fact]
    public void PickingADate_WritesItIntoTheInput()
    {
        var input = new DateTimeInput(SomeDateTime);
        var picker = new SmrDatePicker { DateTimeInput = input };

        picker.Date = new DateTime(2027, 1, 2);

        Assert.Equal(new DateOnly(2027, 1, 2), input.DateAsDateOnly);
        Assert.Equal(TimeOnly.FromDateTime(SomeDateTime), input.TimeAsTimeOnly);
    }

    [Fact]
    public void InXaml_TheInputIsBoundByItsShortName()
    {
        const string Xaml = """
            <ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                         xmlns:smr="https://github.com/startbits/SumireCraft.Maui">
              <smr:SmrDatePicker DateTimeInput="{Binding Start}" />
            </ContentView>
            """;
        var model = new Model();
        var view = new ContentView().LoadFromXaml(Xaml);

        view.BindingContext = model;

        var picker = Assert.IsType<SmrDatePicker>(view.Content);
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
