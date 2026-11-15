using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Fields;

// The names of the style classes an application styles the title and the error through are the
// application's to change, so that a type of its own derived from SmrField leaves no Smr name in its
// styles.
public class SmrFieldStyleClassTests
{
    private static readonly Color LightError = Color.FromArgb("#B3261E");

    [Fact]
    public void TheClassNames_AreSmrFieldTitleAndSmrFieldErrorByDefault()
    {
        var field = new SmrField();

        Assert.Equal("SmrFieldTitle", field.TitleStyleClass);
        Assert.Equal("SmrFieldError", field.ErrorStyleClass);
    }

    [Fact]
    public void AClassNameOfTheApplicationsOwn_TakesThePlaceOfTheDefaultOne()
    {
        var field = new SmrField { TitleStyleClass = "AppFieldTitle", ErrorStyleClass = "AppFieldError" };

        Assert.Contains("AppFieldTitle", Title(field).StyleClass);
        Assert.DoesNotContain("SmrFieldTitle", Title(field).StyleClass);
        Assert.Contains("AppFieldError", Error(field).StyleClass);
        Assert.DoesNotContain("SmrFieldError", Error(field).StyleClass);
    }

    [Fact]
    public void AStyleForTheApplicationsTitleClass_ReachesTheTitle()
    {
        var field = new SmrField { Title = "Name", TitleStyleClass = "AppFieldTitle" };
        _ = new Grid { Resources = new ResourceDictionary { Style("AppFieldTitle", Label.FontSizeProperty, 20d) }, Children = { field } };

        Assert.Equal(20d, Title(field).FontSize);
    }

    [Fact]
    public void AStyleForTheDefaultTitleClass_NoLongerReachesATitleWithAClassOfItsOwn()
    {
        var field = new SmrField { Title = "Name", TitleStyleClass = "AppFieldTitle" };
        _ = new Grid { Resources = new ResourceDictionary { Style("SmrFieldTitle", Label.FontSizeProperty, 20d) }, Children = { field } };

        Assert.Equal(14d, Title(field).FontSize);
    }

    // The application's class still comes after the field's own defaults, so it wins where they meet.
    [Fact]
    public void AStyleForTheApplicationsErrorClass_ReplacesTheDefaultColor()
    {
        var field = new SmrField { ErrorText = "Required", ErrorStyleClass = "AppFieldError" };
        _ = new Grid { Resources = new ResourceDictionary { Style("AppFieldError", Label.TextColorProperty, Colors.Teal) }, Children = { field } };

        Assert.Equal(Colors.Teal, Error(field).TextColor);
    }

    [Fact]
    public void AClassNameChangedOnThePage_IsTakenUp()
    {
        var field = new SmrField { Title = "Name", ErrorText = "Required" };
        _ = new Grid
        {
            Resources = new ResourceDictionary { Style("AppFieldTitle", Label.FontSizeProperty, 20d), Style("AppFieldError", Label.TextColorProperty, Colors.Teal) },
            Children = { field },
        };

        field.TitleStyleClass = "AppFieldTitle";
        field.ErrorStyleClass = "AppFieldError";

        Assert.Equal(20d, Title(field).FontSize);
        Assert.Equal(Colors.Teal, Error(field).TextColor);
    }

    // With no class name the labels carry the field's own defaults alone, and look as they do by
    // default.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NoClassName_LeavesTheDefaultsAlone(string? name)
    {
        var field = new SmrField { Title = "Name", ErrorText = "Required", TitleStyleClass = name, ErrorStyleClass = name };
        _ = new Grid { field };

        Assert.DoesNotContain(Title(field).StyleClass, string.IsNullOrEmpty);
        Assert.DoesNotContain(Error(field).StyleClass, string.IsNullOrEmpty);
        Assert.DoesNotContain("SmrFieldTitle", Title(field).StyleClass);
        Assert.DoesNotContain("SmrFieldError", Error(field).StyleClass);
        Assert.Equal(14d, Title(field).FontSize);
        Assert.Equal(LightError, Error(field).TextColor);
    }

    // An error that comes and goes does not change which classes the labels carry.
    [Fact]
    public void AnErrorComingAndGoing_KeepsTheApplicationsClass()
    {
        var field = new SmrField { ErrorStyleClass = "AppFieldError" };

        field.ErrorText = "Required";
        field.ErrorText = null;

        Assert.Contains("AppFieldError", Error(field).StyleClass);
    }

    // The way an application names its classes once for every field: a type of its own that sets
    // them, or a style for the field.

    [Fact]
    public void ATypeDerivedFromTheField_CanNameTheClasses()
    {
        var field = new AppField { Title = "Name" };
        _ = new Grid { Resources = new ResourceDictionary { Style("AppFieldTitle", Label.FontSizeProperty, 20d) }, Children = { field } };

        Assert.Equal(20d, Title(field).FontSize);
    }

    [Fact]
    public void AStyleForTheField_CanNameTheClasses()
    {
        var fieldStyle = new Style(typeof(SmrField)) { Setters = { new Setter { Property = SmrField.TitleStyleClassProperty, Value = "AppFieldTitle" } } };
        var field = new SmrField { Title = "Name" };
        _ = new Grid { Resources = new ResourceDictionary { fieldStyle, Style("AppFieldTitle", Label.FontSizeProperty, 20d) }, Children = { field } };

        Assert.Equal(20d, Title(field).FontSize);
    }

    private static Style Style(string className, BindableProperty property, object value) => new(typeof(Label))
    {
        Class = className,
        Setters = { new Setter { Property = property, Value = value } },
    };

    private static Grid Root(SmrField field) =>
        Assert.IsType<Grid>(Assert.Single(((IVisualTreeElement)field).GetVisualChildren()), exactMatch: false);

    private static Label Title(SmrField field) => (Label)Root(field).Children[0];

    private static Label Error(SmrField field) => (Label)Root(field).Children[2];

    private sealed class AppField : SmrField
    {
        public AppField()
        {
            TitleStyleClass = "AppFieldTitle";
            ErrorStyleClass = "AppFieldError";
        }
    }
}
