using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Fields;

// What a field shows of its title and its error, how an application restyles them, and what it
// tells a screen reader. Where the rows then land on a screen is the UI tests'.
public class SmrFieldTests
{
    private static readonly Color LightError = Color.FromArgb("#B3261E");

    // The error a field reports is whatever text it is given; it reads no validation of its own.

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("Required", true)]
    public void HasError_FollowsTheErrorText(string? error, bool expected)
    {
        var field = new SmrField { ErrorText = error };

        Assert.Equal(expected, field.HasError);
    }

    [Fact]
    public void AnErrorAppearing_SaysSoThroughHasError()
    {
        var field = new SmrField();
        var changed = new List<string?>();
        field.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        field.ErrorText = "Required";

        Assert.Contains(nameof(SmrField.HasError), changed);
    }

    [Fact]
    public void ReserveErrorSpace_IsOnByDefault()
    {
        Assert.True(new SmrField().ReserveErrorSpace);
    }

    // The default template: the title above, the content, the error below, and nothing else.

    [Fact]
    public void TheDefaultTemplate_StacksTitleContentAndError()
    {
        var entry = new Entry();
        var field = new SmrField { Title = "Name", Content = entry };

        var root = Root(field);

        Assert.Equal(3, root.Children.Count);
        Assert.IsType<Label>(root.Children[0]);
        Assert.IsType<ContentPresenter>(root.Children[1]);
        Assert.IsType<Label>(root.Children[2]);
        Assert.Equal([0, 1, 2], root.Children.Select(child => Grid.GetRow((BindableObject)child)));
    }

    // Title and error take the height of their text, and the content takes whatever is left, so a
    // field given a height of its own grows its content rather than its labels. Spacing comes from
    // the labels' margins: a grid's row spacing would stay behind a missing title.

    [Fact]
    public void TheRows_GiveWhatIsLeftToTheContent()
    {
        var root = Root(new SmrField());

        Assert.Equal(
            [GridLength.Auto, GridLength.Star, GridLength.Auto],
            root.RowDefinitions.Select(row => row.Height));
        Assert.Equal(0, root.RowSpacing);
    }

    [Fact]
    public void TheTitle_IsShown()
    {
        var field = new SmrField { Title = "Name" };

        Assert.True(Title(field).IsVisible);
        Assert.Equal("Name", Title(field).Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NoTitle_LeavesNoRowForIt(string? title)
    {
        var field = new SmrField { Title = title };

        Assert.False(Title(field).IsVisible);
    }

    // A field with no title can keep a line for one, so that it lines up with a titled field beside
    // it. Off by default: a form with no titles wants no space above its inputs.

    [Fact]
    public void ReserveTitleSpace_IsOffByDefault()
    {
        Assert.False(new SmrField().ReserveTitleSpace);
    }

    [Fact]
    public void NoTitleWithReservedSpace_KeepsALineForIt()
    {
        var field = new SmrField { ReserveTitleSpace = true };

        Assert.True(Title(field).IsVisible);
        Assert.Equal(" ", Title(field).Text);
    }

    [Fact]
    public void ATitleWithReservedSpace_IsShown()
    {
        var field = new SmrField { ReserveTitleSpace = true, Title = "Name" };

        Assert.True(Title(field).IsVisible);
        Assert.Equal("Name", Title(field).Text);
    }

    [Fact]
    public void ATitleCleared_GoesBackToTheReservedLine()
    {
        var field = new SmrField { ReserveTitleSpace = true, Title = "Name" };

        field.Title = null;

        Assert.Equal(" ", Title(field).Text);
    }

    [Fact]
    public void TheBlankLineKeptForATitle_IsNotReadOut()
    {
        var field = new SmrField { ReserveTitleSpace = true };

        Assert.False(AutomationProperties.GetIsInAccessibleTree(Title(field)));
    }

    [Fact]
    public void ATitle_IsReadOut()
    {
        var field = new SmrField { ReserveTitleSpace = true, Title = "Name" };

        Assert.True(AutomationProperties.GetIsInAccessibleTree(Title(field)));
    }

    [Fact]
    public void ATitleChanged_IsShown()
    {
        var field = new SmrField { Title = "Name" };

        field.Title = "Full name";

        Assert.Equal("Full name", Title(field).Text);
    }

    [Fact]
    public void TheError_IsShown()
    {
        var field = new SmrField { ErrorText = "Required" };

        Assert.True(Error(field).IsVisible);
        Assert.Equal("Required", Error(field).Text);
    }

    // With no error, the row stays one line high by default, so that an error coming and going does
    // not push what lies below the field up and down. A blank line, not an empty one: an empty label
    // measures no height at all.

    [Fact]
    public void NoError_KeepsALineForIt()
    {
        var field = new SmrField();

        Assert.True(Error(field).IsVisible);
        Assert.Equal(" ", Error(field).Text);
    }

    [Fact]
    public void NoErrorWithoutReservedSpace_LeavesNoRowForIt()
    {
        var field = new SmrField { ReserveErrorSpace = false };

        Assert.False(Error(field).IsVisible);
    }

    [Fact]
    public void AnErrorWithoutReservedSpace_IsShown()
    {
        var field = new SmrField { ReserveErrorSpace = false, ErrorText = "Required" };

        Assert.True(Error(field).IsVisible);
        Assert.Equal("Required", Error(field).Text);
    }

    [Fact]
    public void AnErrorCleared_GoesBackToTheReservedLine()
    {
        var field = new SmrField { ErrorText = "Required" };

        field.ErrorText = null;

        Assert.Equal(" ", Error(field).Text);
    }

    // The look of the title and the error is the application's to change through two style
    // classes, without writing a template: one style in App.xaml reaches every field, and one in a
    // page's resources reaches the fields on that page.

    [Fact]
    public void TheLabels_CarryTheStyleClassesAnApplicationStyles()
    {
        var field = new SmrField();

        Assert.Contains("SmrFieldTitle", Title(field).StyleClass);
        Assert.Contains("SmrFieldError", Error(field).StyleClass);
    }

    // What the field's own styles give the labels before an application says anything.

    [Fact]
    public void TheTitleByDefault_IsSetApartFromTheContent()
    {
        var field = new SmrField { Title = "Name" };
        _ = new Grid { field };

        Assert.Equal(14d, Title(field).FontSize);
        Assert.Equal(new Thickness(0, 0, 0, 4), Title(field).Margin);
    }

    [Fact]
    public void TheErrorByDefault_IsSmallerAndSetApartFromTheContent()
    {
        var field = new SmrField { ErrorText = "Required" };
        _ = new Grid { field };

        Assert.Equal(12d, Error(field).FontSize);
        Assert.Equal(new Thickness(0, 4, 0, 0), Error(field).Margin);
    }

    [Fact]
    public void TheErrorByDefault_IsMaterialsErrorRed()
    {
        var field = new SmrField { ErrorText = "Required" };
        _ = new Grid { field };

        Assert.Equal(LightError, Error(field).TextColor);
    }

    [Fact]
    public void AStyleForTheErrorClass_ReplacesTheDefaultColor()
    {
        var field = new SmrField { ErrorText = "Required" };
        _ = new Grid { Resources = new ResourceDictionary { ErrorStyle(Label.TextColorProperty, Colors.Teal) }, Children = { field } };

        Assert.Equal(Colors.Teal, Error(field).TextColor);
    }

    [Fact]
    public void AStyleForTheErrorClassOnThePage_WinsOverOneFurtherOut()
    {
        var field = new SmrField { ErrorText = "Required" };
        var page = new Grid { Resources = new ResourceDictionary { ErrorStyle(Label.TextColorProperty, Colors.Purple) }, Children = { field } };
        _ = new Grid { Resources = new ResourceDictionary { ErrorStyle(Label.TextColorProperty, Colors.Teal) }, Children = { page } };

        Assert.Equal(Colors.Purple, Error(field).TextColor);
    }

    // Styles in .NET MAUI do not combine by property: the nearer style for a class hides the further
    // one whole, so what only the further one set falls back to the field's default.
    [Fact]
    public void AStyleForTheErrorClassOnThePage_HidesTheOneFurtherOutWhole()
    {
        var field = new SmrField { ErrorText = "Required" };
        var page = new Grid { Resources = new ResourceDictionary { ErrorStyle(Label.TextColorProperty, Colors.Purple) }, Children = { field } };
        _ = new Grid { Resources = new ResourceDictionary { ErrorStyle(Label.FontSizeProperty, 20d) }, Children = { page } };

        Assert.Equal(Colors.Purple, Error(field).TextColor);
        Assert.Equal(12d, Error(field).FontSize);
    }

    [Fact]
    public void AStyleForTheErrorClassThatSaysNothingOfColor_KeepsTheDefaultColor()
    {
        var field = new SmrField { ErrorText = "Required" };
        _ = new Grid { Resources = new ResourceDictionary { ErrorStyle(Label.FontSizeProperty, 20d) }, Children = { field } };

        Assert.Equal(LightError, Error(field).TextColor);
        Assert.Equal(20d, Error(field).FontSize);
    }

    [Fact]
    public void AStyleForTheTitleClass_ReachesTheTitle()
    {
        var style = new Style(typeof(Label)) { Class = "SmrFieldTitle", Setters = { new Setter { Property = Label.FontSizeProperty, Value = 20d } } };
        var field = new SmrField { Title = "Name" };
        _ = new Grid { Resources = new ResourceDictionary { style }, Children = { field } };

        Assert.Equal(20d, Title(field).FontSize);
    }

    [Fact]
    public void AnImplicitStyleForEveryLabel_StillReachesTheTitleAndTheError()
    {
        var style = new Style(typeof(Label)) { Setters = { new Setter { Property = Label.FontFamilyProperty, Value = "AppFont" } } };
        var field = new SmrField { Title = "Name", ErrorText = "Required" };
        _ = new Grid { Resources = new ResourceDictionary { style }, Children = { field } };

        Assert.Equal("AppFont", Title(field).FontFamily);
        Assert.Equal("AppFont", Error(field).FontFamily);
        Assert.Equal(LightError, Error(field).TextColor);
    }

    [Fact]
    public void ReplacingTheResources_KeepsTheDefaultColor()
    {
        var field = new SmrField { ErrorText = "Required" };
        var first = new Grid { field };

        field.Resources = [];
        first.Remove(field);
        _ = new Grid { field };

        Assert.Equal(LightError, Error(field).TextColor);
    }

    // A screen reader reads the title, the content and the error in the order they are drawn. The
    // field writes nothing on the content: a name or a hint the application gave it, by binding or
    // otherwise, stays its own, and Android's Entry is not given a description, which TalkBack
    // does not handle well there.

    [Fact]
    public void TheContent_IsLeftAsTheApplicationMadeIt()
    {
        var entry = new Entry();
        _ = new SmrField { Title = "Name", ErrorText = "Required", Content = entry };

        Assert.False(entry.IsSet(SemanticProperties.DescriptionProperty));
        Assert.False(entry.IsSet(SemanticProperties.HintProperty));
    }

    // The blank line kept for an error is there for its height alone, and a screen reader is not to
    // stop on it.

    [Fact]
    public void TheBlankLineKeptForAnError_IsNotReadOut()
    {
        var field = new SmrField();

        Assert.False(AutomationProperties.GetIsInAccessibleTree(Error(field)));
    }

    [Fact]
    public void AnError_IsReadOut()
    {
        var field = new SmrField();

        field.ErrorText = "Required";

        Assert.True(AutomationProperties.GetIsInAccessibleTree(Error(field)));
    }

    [Fact]
    public void AnErrorCleared_IsNotReadOutAgain()
    {
        var field = new SmrField { ErrorText = "Required" };

        field.ErrorText = null;

        Assert.False(AutomationProperties.GetIsInAccessibleTree(Error(field)));
    }

    [Fact]
    public void ATemplateOfTheApplicationsOwn_IsLeftAlone()
    {
        var template = new ControlTemplate(() => new VerticalStackLayout { new ContentPresenter() });
        var field = new SmrField { ControlTemplate = template, Title = "Name", ErrorText = "Required" };

        var root = Assert.IsType<VerticalStackLayout>(Assert.Single(((IVisualTreeElement)field).GetVisualChildren()));
        Assert.IsType<ContentPresenter>(Assert.Single(root));
    }

    private static Style ErrorStyle(BindableProperty property, object value) => new(typeof(Label))
    {
        Class = "SmrFieldError",
        Setters = { new Setter { Property = property, Value = value } },
    };

    private static Grid Root(SmrField field) =>
        Assert.IsType<Grid>(Assert.Single(((IVisualTreeElement)field).GetVisualChildren()), exactMatch: false);

    private static Label Title(SmrField field) => (Label)Root(field).Children[0];

    private static Label Error(SmrField field) => (Label)Root(field).Children[2];
}
