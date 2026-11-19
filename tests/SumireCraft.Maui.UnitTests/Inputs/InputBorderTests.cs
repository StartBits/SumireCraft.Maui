using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Inputs;

// What border each of the five inputs is drawn with, worked out from its properties: the platform's
// own box until one of them is given, then the library's. How a platform draws it is the UI tests'.
[Collection(XamlLoadingCollection.Name)]
public class InputBorderTests
{
    public static TheoryData<string> Inputs =>
        [nameof(SmrEntry), nameof(SmrEditor), nameof(SmrPicker), nameof(SmrDatePicker), nameof(SmrTimePicker)];

    // An input given nothing keeps the platform's box.

    [Theory]
    [MemberData(nameof(Inputs))]
    public void AnInputGivenNothing_KeepsThePlatformsBox(string input)
    {
        var view = Create(input);
        var bordered = (IBorderedInput)view;

        Assert.Null(bordered.BorderThickness);
        Assert.Null(bordered.CornerRadius);
        Assert.Null(bordered.BorderColor);
        Assert.Null(InputBorder.LookOf(view));
    }

    // XAML looks for what turns "0,0,0,1" into a thickness on the type that declares the property, so
    // each input declares its three on itself.
    [Theory]
    [MemberData(nameof(Inputs))]
    public void EveryInput_DeclaresTheThreeOnItself(string input)
    {
        var view = Create(input);

        Assert.Equal(view.GetType(), Property(view, nameof(SmrEntry.BorderThickness)).DeclaringType);
        Assert.Equal(view.GetType(), Property(view, nameof(SmrEntry.CornerRadius)).DeclaringType);
        Assert.Equal(view.GetType(), Property(view, nameof(SmrEntry.BorderColor)).DeclaringType);
    }

    // A thickness or a corner radius takes the box over; the other then has a default of its own.

    [Theory]
    [MemberData(nameof(Inputs))]
    public void AThickness_DrawsTheBorderWithSquareCorners(string input)
    {
        var view = Create(input);

        view.SetValue(Property(view, nameof(SmrEntry.BorderThickness)), new Thickness(0, 0, 0, 1));

        var look = Assert.NotNull(InputBorder.LookOf(view));
        Assert.Equal(new Thickness(0, 0, 0, 1), look.Thickness);
        Assert.Equal(new CornerRadius(0), look.CornerRadius);
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void ACornerRadius_DrawsTheBorderOneThickAllRound(string input)
    {
        var view = Create(input);

        view.SetValue(Property(view, nameof(SmrEntry.CornerRadius)), new CornerRadius(4, 4, 0, 0));

        var look = Assert.NotNull(InputBorder.LookOf(view));
        Assert.Equal(new Thickness(1), look.Thickness);
        Assert.Equal(new CornerRadius(4, 4, 0, 0), look.CornerRadius);
    }

    [Fact]
    public void AThicknessOfNothing_StillTakesTheBoxOver()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(0) };

        var look = Assert.NotNull(InputBorder.LookOf(view));
        Assert.Equal(new Thickness(0), look.Thickness);
    }

    [Fact]
    public void ClearingBoth_GivesThePlatformsBoxBack()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4) };

        view.ClearValue(SmrEntry.BorderThicknessProperty);
        Assert.NotNull(InputBorder.LookOf(view));
        view.ClearValue(SmrEntry.CornerRadiusProperty);

        Assert.Null(InputBorder.LookOf(view));
    }

    [Fact]
    public void ANewThickness_IsDrawn()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(0, 0, 0, 1) };

        view.BorderThickness = new Thickness(0, 0, 0, 2);

        Assert.Equal(new Thickness(0, 0, 0, 2), InputBorder.LookOf(view)?.Thickness);
    }

    [Theory]
    [InlineData(-1d, 0d, 0d, 0d)]
    [InlineData(0d, 0d, 0d, double.NaN)]
    [InlineData(0d, double.PositiveInfinity, 0d, 0d)]
    public void AThicknessThatIsNoThickness_IsPassedOver(double left, double top, double right, double bottom)
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1) };

        view.BorderThickness = new Thickness(left, top, right, bottom);

        Assert.Equal(new Thickness(1), view.BorderThickness);
    }

    [Fact]
    public void ACornerRadiusBelowNothing_IsPassedOver()
    {
        var view = new SmrEntry { CornerRadius = new CornerRadius(4) };

        view.CornerRadius = new CornerRadius(-1, 0, 0, 0);

        Assert.Equal(new CornerRadius(4), view.CornerRadius);
    }

    // The border is the text's colour unless given one of its own, and follows the text's.

    [Theory]
    [MemberData(nameof(Inputs))]
    public void TheBorder_IsTheTextsColour(string input)
    {
        var view = Create(input);
        SetTextColor(view, Colors.Teal);

        view.SetValue(Property(view, nameof(SmrEntry.BorderThickness)), new Thickness(1));

        Assert.Equal(Colors.Teal, InputBorder.LookOf(view)?.Color);
    }

    [Fact]
    public void TheBorder_FollowsTheTextsColour()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1), TextColor = Colors.Teal };

        view.TextColor = Colors.Orange;

        Assert.Equal(Colors.Orange, InputBorder.LookOf(view)?.Color);
    }

    [Fact]
    public void WithNoTextColour_TheBorderIsThePlatformsTextColour()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1) };

        Assert.Null(InputBorder.LookOf(view)?.Color);
    }

    [Fact]
    public void AColourOfItsOwn_TakesThePlaceOfTheTexts()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1), TextColor = Colors.Teal, BorderColor = Colors.Purple };

        view.TextColor = Colors.Orange;

        Assert.Equal(Colors.Purple, InputBorder.LookOf(view)?.Color);
    }

    // What draws the border is handed each change as it comes, and let go of with the border.

    [Fact]
    public void ABorderGiven_IsHandedToWhatDrawsIt()
    {
        var view = new SmrEntry { TextColor = Colors.Teal };

        view.BorderThickness = new Thickness(0, 0, 0, 1);

        Assert.Equal(new BorderLook(new Thickness(0, 0, 0, 1), new CornerRadius(0), Colors.Teal), InputBorder.DrawnLookOf(view));
    }

    [Fact]
    public void ANewThickness_IsHandedToWhatDrawsIt()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(0, 0, 0, 1) };

        view.BorderThickness = new Thickness(0, 0, 0, 2);

        Assert.Equal(new Thickness(0, 0, 0, 2), InputBorder.DrawnLookOf(view)?.Thickness);
    }

    [Fact]
    public void ANewCornerRadius_IsHandedToWhatDrawsIt()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1) };

        view.CornerRadius = new CornerRadius(4);

        Assert.Equal(new CornerRadius(4), InputBorder.DrawnLookOf(view)?.CornerRadius);
    }

    [Fact]
    public void ANewTextColour_IsHandedToWhatDrawsIt()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1), TextColor = Colors.Teal };

        view.TextColor = Colors.Orange;

        Assert.Equal(Colors.Orange, InputBorder.DrawnLookOf(view)?.Color);
    }

    [Fact]
    public void ANewBorderColour_IsHandedToWhatDrawsIt()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1) };

        view.BorderColor = Colors.Purple;

        Assert.Equal(Colors.Purple, InputBorder.DrawnLookOf(view)?.Color);
    }

    [Fact]
    public void ClearingBoth_LetsGoOfWhatDrawsTheBorder()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4) };

        view.ClearValue(SmrEntry.BorderThicknessProperty);
        view.ClearValue(SmrEntry.CornerRadiusProperty);

        Assert.Null(InputBorder.DrawnLookOf(view));
    }

    [Fact]
    public void ABorderGivenAgain_IsDrawnAgain()
    {
        var view = new SmrEntry { BorderThickness = new Thickness(1) };
        view.ClearValue(SmrEntry.BorderThicknessProperty);

        view.BorderThickness = new Thickness(0, 0, 0, 1);

        Assert.Equal(new Thickness(0, 0, 0, 1), InputBorder.DrawnLookOf(view)?.Thickness);
    }

    // What a style, a visual state or XAML sets, the input draws.

    [Fact]
    public void InXaml_TheThreeAreWrittenAsOnAnyView()
    {
        const string Xaml = """
            <ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                         xmlns:smr="https://github.com/startbits/SumireCraft.Maui">
              <smr:SmrEntry BorderThickness="0,0,0,1" CornerRadius="4,4,0,0" BorderColor="Purple" />
            </ContentView>
            """;

        var view = new ContentView().LoadFromXaml(Xaml);

        var entry = Assert.IsType<SmrEntry>(view.Content);
        Assert.Equal(new Thickness(0, 0, 0, 1), entry.BorderThickness);
        Assert.Equal(new CornerRadius(4, 4, 0, 0), entry.CornerRadius);
        Assert.Equal(Colors.Purple, entry.BorderColor);
    }

    [Fact]
    public void AStyle_SetsTheBorder()
    {
        const string Xaml = """
            <ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                         xmlns:smr="https://github.com/startbits/SumireCraft.Maui">
              <ContentView.Resources>
                <Style TargetType="smr:SmrEntry">
                  <Setter Property="BorderThickness" Value="0,0,0,1" />
                  <Setter Property="CornerRadius" Value="2" />
                </Style>
              </ContentView.Resources>
              <smr:SmrEntry />
            </ContentView>
            """;

        var view = new ContentView().LoadFromXaml(Xaml);

        var entry = Assert.IsType<SmrEntry>(view.Content);
        Assert.Equal(new Thickness(0, 0, 0, 1), InputBorder.LookOf(entry)?.Thickness);
        Assert.Equal(new CornerRadius(2), InputBorder.LookOf(entry)?.CornerRadius);
    }

    [Fact]
    public void AVisualState_ChangesTheThickness()
    {
        const string Xaml = """
            <ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                         xmlns:smr="https://github.com/startbits/SumireCraft.Maui">
              <smr:SmrEntry BorderThickness="0,0,0,1">
                <VisualStateManager.VisualStateGroups>
                  <VisualStateGroupList>
                    <VisualStateGroup x:Name="CommonStates">
                      <VisualState x:Name="Normal" />
                      <VisualState x:Name="Thick">
                        <VisualState.Setters>
                          <Setter Property="BorderThickness" Value="0,0,0,2" />
                        </VisualState.Setters>
                      </VisualState>
                    </VisualStateGroup>
                  </VisualStateGroupList>
                </VisualStateManager.VisualStateGroups>
              </smr:SmrEntry>
            </ContentView>
            """;
        var view = new ContentView().LoadFromXaml(Xaml);
        var entry = Assert.IsType<SmrEntry>(view.Content);

        VisualStateManager.GoToState(entry, "Thick");
        Assert.Equal(new Thickness(0, 0, 0, 2), InputBorder.DrawnLookOf(entry)?.Thickness);
        VisualStateManager.GoToState(entry, "Normal");
        Assert.Equal(new Thickness(0, 0, 0, 1), InputBorder.DrawnLookOf(entry)?.Thickness);
    }

    // A style is one instance for every input it reaches, and each input keeps a border of its own.
    [Fact]
    public void OneStyleForManyInputs_LeavesEachItsOwnBorder()
    {
        var style = new Style(typeof(SmrEntry))
        {
            Setters = { new Setter { Property = SmrEntry.BorderThicknessProperty, Value = new Thickness(0, 0, 0, 1) } },
        };
        var first = new SmrEntry { Style = style, TextColor = Colors.Teal };
        var second = new SmrEntry { Style = style, TextColor = Colors.Orange };

        Assert.Equal(Colors.Teal, InputBorder.LookOf(first)?.Color);
        Assert.Equal(Colors.Orange, InputBorder.LookOf(second)?.Color);
    }

    // An input with a border is let go of with its page: the library keeps nothing that holds it.
    [Fact]
    public async Task AnInputWithABorder_IsLetGoOf()
    {
        var reference = BorderedAndForgotten();

        for (var i = 0; i < 3 && reference.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Yield();
        }

        Assert.False(reference.IsAlive);
    }

    // Not inlined, so that no local of the caller's keeps the input reachable.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference BorderedAndForgotten() => new(new SmrEntry { BorderThickness = new Thickness(1) });

    internal static View Create(string input) => input switch
    {
        nameof(SmrEntry) => new SmrEntry(),
        nameof(SmrEditor) => new SmrEditor(),
        nameof(SmrPicker) => new SmrPicker(),
        nameof(SmrDatePicker) => new SmrDatePicker(),
        nameof(SmrTimePicker) => new SmrTimePicker(),
        _ => throw new ArgumentOutOfRangeException(nameof(input)),
    };

    // The input's own property of the name, as XAML finds it.
    private static BindableProperty Property(View view, string name) =>
        (BindableProperty)view.GetType().GetField(name + "Property")!.GetValue(null)!;

    private static void SetTextColor(View view, Color color)
    {
        switch (view)
        {
            case Entry entry:
                entry.TextColor = color;
                break;
            case Editor editor:
                editor.TextColor = color;
                break;
            case Picker picker:
                picker.TextColor = color;
                break;
            case DatePicker date:
                date.TextColor = color;
                break;
            case TimePicker time:
                time.TextColor = color;
                break;
        }
    }
}
