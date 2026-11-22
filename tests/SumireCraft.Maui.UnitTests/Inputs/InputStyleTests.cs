using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Inputs;

// A style an application wrote for every Entry, Editor, Picker, DatePicker or TimePicker -- the
// Styles.xaml of a new project has one for each -- reaches the library's input derived from it too,
// although .NET MAUI gives an implicit style to the exact type alone.
[Collection(XamlLoadingCollection.Name)]
public class InputStyleTests
{
    public static TheoryData<string> Inputs => InputBorderTests.Inputs;

    [Theory]
    [MemberData(nameof(Inputs))]
    public void AStyleForEveryInputOfTheKind_ReachesTheInput(string input)
    {
        var view = InputBorderTests.Create(input);
        var page = new Grid();
        page.Resources.Add(Colored(BaseOf(view), Colors.Teal));

        page.Add(view);

        Assert.Equal(Colors.Teal, TextColor(view));
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void AStyleOfItsOwn_TakesThePlaceOfTheOneForEveryInputOfTheKind(string input)
    {
        var view = InputBorderTests.Create(input);
        var page = new Grid();
        page.Resources.Add(Colored(BaseOf(view), Colors.Teal));
        view.Style = Colored(view.GetType(), Colors.Orange);

        page.Add(view);

        Assert.Equal(Colors.Orange, TextColor(view));
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void AnImplicitStyleOfItsOwn_WinsOverTheOneForEveryInputOfTheKind(string input)
    {
        var view = InputBorderTests.Create(input);
        var page = new Grid();
        page.Resources.Add(Colored(BaseOf(view), Colors.Teal));
        page.Resources.Add(Colored(view.GetType(), Colors.Orange));

        page.Add(view);

        Assert.Equal(Colors.Orange, TextColor(view));
    }

    // The style for every input of the kind is applied to derived types, so an implicit style for the
    // input's own type goes on top of it rather than in its place: what the one for the input's type
    // leaves unset, such as Styles.xaml's font and height, still comes from the one for every input.
    [Theory]
    [MemberData(nameof(Inputs))]
    public void AnImplicitStyleOfItsOwn_GoesOnTopOfTheOneForEveryInputOfTheKind(string input)
    {
        var view = InputBorderTests.Create(input);
        var page = new Grid();
        page.Resources.Add(new Style(BaseOf(view))
        {
            Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 0.5 } },
        });
        page.Resources.Add(Colored(view.GetType(), Colors.Orange));

        page.Add(view);

        Assert.Equal(0.5, view.Opacity);
        Assert.Equal(Colors.Orange, TextColor(view));
    }

    // As on .NET MAUI's own input: a Style set on the input goes on top of the implicit one, and what
    // it leaves unset still comes from there.
    [Theory]
    [MemberData(nameof(Inputs))]
    public void AStyleOfItsOwn_LeavesWhatItDoesNotSetToTheOneForEveryInputOfTheKind(string input)
    {
        var view = InputBorderTests.Create(input);
        var page = new Grid();
        page.Resources.Add(new Style(BaseOf(view))
        {
            Setters = { new Setter { Property = VisualElement.OpacityProperty, Value = 0.5 } },
        });
        view.Style = Colored(view.GetType(), Colors.Orange);

        page.Add(view);

        Assert.Equal(0.5, view.Opacity);
        Assert.Equal(Colors.Orange, TextColor(view));
    }

    [Fact]
    public void TheStyleForEveryInputOfTheKind_IsFollowedWhenItChanges()
    {
        var view = new SmrEntry();
        var page = new Grid();
        page.Resources.Add(Colored(typeof(Entry), Colors.Teal));
        page.Add(view);

        page.Resources[typeof(Entry).FullName] = Colored(typeof(Entry), Colors.Orange);

        Assert.Equal(Colors.Orange, view.TextColor);
    }

    // As on .NET MAUI's own input, which keeps a style taken out of its dictionary.
    [Fact]
    public void TheStyleForEveryInputOfTheKind_TakenAway_IsKeptAsByAnEntry()
    {
        var plain = new Entry();
        var view = new SmrEntry();
        var page = new Grid();
        page.Resources.Add(Colored(typeof(Entry), Colors.Teal));
        page.Add(plain);
        page.Add(view);

        page.Resources.Remove(typeof(Entry).FullName);

        Assert.Equal(plain.TextColor, view.TextColor);
    }

    [Fact]
    public void AStyleForEveryInputOfTheKind_CanSetTheBorder()
    {
        var view = new SmrEntry();
        var page = new Grid();
        page.Resources.Add(new Style(typeof(Entry))
        {
            Setters = { new Setter { Property = SmrEntry.BorderThicknessProperty, Value = new Thickness(0, 0, 0, 1) } },
        });

        page.Add(view);

        Assert.Equal(new Thickness(0, 0, 0, 1), view.BorderThickness);
    }

    // A dictionary the application gives the input may be shared, and is not written to: the input
    // goes without the style for every input of the kind instead.
    [Fact]
    public void ADictionaryGivenToTheInput_IsNotWrittenTo()
    {
        var page = new Grid();
        page.Resources.Add(Colored(typeof(Entry), Colors.Teal));
        var given = new ResourceDictionary();
        var view = new SmrEntry { Resources = given };
        page.Add(view);

        page.Resources[typeof(Entry).FullName] = Colored(typeof(Entry), Colors.Orange);

        Assert.Empty(given);
    }

    private static Type BaseOf(View view) => view switch
    {
        Entry => typeof(Entry),
        Editor => typeof(Editor),
        Picker => typeof(Picker),
        DatePicker => typeof(DatePicker),
        TimePicker => typeof(TimePicker),
        _ => throw new ArgumentOutOfRangeException(nameof(view)),
    };

    private static Color? TextColor(View view) => ((ITextStyle)view).TextColor;

    private static BindableProperty TextColorProperty(Type type) =>
        typeof(Entry).IsAssignableFrom(type) ? Entry.TextColorProperty
        : typeof(Editor).IsAssignableFrom(type) ? Editor.TextColorProperty
        : typeof(Picker).IsAssignableFrom(type) ? Picker.TextColorProperty
        : typeof(DatePicker).IsAssignableFrom(type) ? DatePicker.TextColorProperty
        : TimePicker.TextColorProperty;

    private static Style Colored(Type type, Color color) => new(type)
    {
        Setters = { new Setter { Property = TextColorProperty(type), Value = color } },
    };
}
