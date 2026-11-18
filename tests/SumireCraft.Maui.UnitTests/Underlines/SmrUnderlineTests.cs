using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Underlines;

// What an underlined input draws: the line's colour and thickness, worked out from the attached
// properties, the text colour and the focus. How each platform draws it is the UI tests'; here no
// platform is loaded, so the line is read as the look the platform would be handed.
public class SmrUnderlineTests
{
    // Nothing is underlined until asked, and the defaults are a thin line that thickens on focus.

    [Fact]
    public void TheDefaults_AreOffWithAThinLineThatThickensOnFocus()
    {
        var entry = new Entry();

        Assert.False(SmrUnderline.GetIsEnabled(entry));
        Assert.Null(SmrUnderline.GetColor(entry));
        Assert.Equal(1d, SmrUnderline.GetThickness(entry));
        Assert.Equal(2d, SmrUnderline.GetFocusedThickness(entry));
    }

    [Fact]
    public void AnInputNotUnderlined_HasNoLine()
    {
        Assert.Null(SmrUnderline.LookOf(new Entry()));
    }

    public static TheoryData<View> Inputs => [new Entry(), new Editor(), new Picker(), new DatePicker(), new TimePicker()];

    [Theory]
    [MemberData(nameof(Inputs))]
    public void EachInput_CanBeUnderlined(View input)
    {
        SmrUnderline.SetIsEnabled(input, true);

        Assert.Equal(1d, SmrUnderline.LookOf(input)?.Thickness);
    }

    [Fact]
    public void AViewThatTakesNoInput_IsLeftAlone()
    {
        var label = new Label();

        SmrUnderline.SetIsEnabled(label, true);

        Assert.Null(SmrUnderline.LookOf(label));
    }

    [Fact]
    public void AnInputNoLongerUnderlined_HasNoLine()
    {
        var entry = new Entry();
        SmrUnderline.SetIsEnabled(entry, true);

        SmrUnderline.SetIsEnabled(entry, false);

        Assert.Null(SmrUnderline.LookOf(entry));
    }

    // The line takes the text's colour unless it is given one of its own. With neither, it is the
    // platform's own text colour, which only the platform knows.

    [Fact]
    public void TheLine_TakesTheTextColour()
    {
        var entry = Underlined(new Entry { TextColor = Colors.Teal });

        Assert.Equal(Colors.Teal, SmrUnderline.LookOf(entry)?.Color);
    }

    [Fact]
    public void TheLine_FollowsTheTextColourWhenItChanges()
    {
        var entry = Underlined(new Entry { TextColor = Colors.Teal });

        entry.TextColor = Colors.Purple;

        Assert.Equal(Colors.Purple, SmrUnderline.LookOf(entry)?.Color);
    }

    [Fact]
    public void AColourOfItsOwn_WinsOverTheTextColour()
    {
        var entry = Underlined(new Entry { TextColor = Colors.Red });

        SmrUnderline.SetColor(entry, Colors.Green);

        Assert.Equal(Colors.Green, SmrUnderline.LookOf(entry)?.Color);
    }

    [Fact]
    public void WithNoColourAtAll_TheLineIsThePlatformsTextColour()
    {
        var entry = Underlined(new Entry());

        var look = SmrUnderline.LookOf(entry);

        Assert.NotNull(look);
        Assert.Null(look.Value.Color);
    }

    // The line is as thick as Thickness, and as FocusedThickness while the input has the focus.

    [Fact]
    public void TheLine_ThickensWhileTheInputHasTheFocus()
    {
        var entry = Underlined(new Entry());

        ((IView)entry).IsFocused = true;

        Assert.Equal(2d, SmrUnderline.LookOf(entry)?.Thickness);
    }

    [Fact]
    public void TheLine_ThinsAgainWhenTheFocusLeaves()
    {
        var entry = Underlined(new Entry());
        ((IView)entry).IsFocused = true;

        ((IView)entry).IsFocused = false;

        Assert.Equal(1d, SmrUnderline.LookOf(entry)?.Thickness);
    }

    [Fact]
    public void ThicknessesOfItsOwn_AreUsed()
    {
        var entry = Underlined(new Entry());
        SmrUnderline.SetThickness(entry, 0.5);
        SmrUnderline.SetFocusedThickness(entry, 3);

        Assert.Equal(0.5, SmrUnderline.LookOf(entry)?.Thickness);
        ((IView)entry).IsFocused = true;
        Assert.Equal(3d, SmrUnderline.LookOf(entry)?.Thickness);
    }

    // .NET MAUI passes over a value its property refuses, with a warning in the log.
    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void AThicknessThatIsNoThickness_IsPassedOver(double thickness)
    {
        var entry = Underlined(new Entry());

        SmrUnderline.SetThickness(entry, thickness);
        SmrUnderline.SetFocusedThickness(entry, thickness);

        Assert.Equal(1d, SmrUnderline.GetThickness(entry));
        Assert.Equal(2d, SmrUnderline.GetFocusedThickness(entry));
        Assert.Equal(1d, SmrUnderline.LookOf(entry)?.Thickness);
    }

    [Fact]
    public void AnInputAlreadyFocused_StartsThick()
    {
        var entry = new Entry();
        ((IView)entry).IsFocused = true;

        SmrUnderline.SetIsEnabled(entry, true);

        Assert.Equal(2d, SmrUnderline.LookOf(entry)?.Thickness);
    }

    // The properties are the view's own, so a style, a visual state and a binding all set them, and
    // a style shared by many inputs gives each a line of its own.

    [Fact]
    public void AStyle_UnderlinesEveryInputItReaches()
    {
        var style = new Style(typeof(Entry))
        {
            Setters =
            {
                new Setter { Property = SmrUnderline.IsEnabledProperty, Value = true },
                new Setter { Property = SmrUnderline.ColorProperty, Value = Colors.Teal },
            },
        };
        var first = new Entry { Style = style };
        var second = new Entry { Style = style, TextColor = Colors.Red };

        second.TextColor = Colors.Blue;
        ((IView)second).IsFocused = true;

        Assert.Equal(new UnderlineLook(Colors.Teal, 1), SmrUnderline.LookOf(first));
        Assert.Equal(new UnderlineLook(Colors.Teal, 2), SmrUnderline.LookOf(second));
    }

    [Fact]
    public void AVisualStateForFocus_ChangesTheColourWhileFocused()
    {
        var entry = Underlined(new Entry { TextColor = Colors.Black });
        VisualStateManager.SetVisualStateGroups(entry,
        [
            new VisualStateGroup
            {
                Name = "CommonStates",
                States =
                {
                    new VisualState { Name = "Normal" },
                    new VisualState { Name = "Focused", Setters = { new Setter { Property = SmrUnderline.ColorProperty, Value = Colors.Orange } } },
                },
            },
        ]);

        ((IView)entry).IsFocused = true;

        Assert.Equal(new UnderlineLook(Colors.Orange, 2), SmrUnderline.LookOf(entry));
    }

    [Fact]
    public void ABinding_SetsTheColour()
    {
        var entry = Underlined(new Entry { BindingContext = new Model { LineColor = Colors.Teal } });

        entry.SetBinding(SmrUnderline.ColorProperty, static (Model m) => m.LineColor);

        Assert.Equal(Colors.Teal, SmrUnderline.LookOf(entry)?.Color);
    }

    // An underlined input is let go of with its page: the library keeps nothing that holds it.

    [Fact]
    public async Task AnUnderlinedInput_IsLetGoOf()
    {
        var reference = UnderlinedAndForgotten();

        for (var i = 0; i < 3 && reference.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Yield();
        }

        Assert.False(reference.IsAlive);
    }

    // Not inlined, so that no local of the caller's keeps the entry reachable.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference UnderlinedAndForgotten() => new(Underlined(new Entry()));

    private static T Underlined<T>(T view)
        where T : View
    {
        SmrUnderline.SetIsEnabled(view, true);
        return view;
    }

    internal sealed class Model
    {
        public Color? LineColor { get; set; }
    }
}
