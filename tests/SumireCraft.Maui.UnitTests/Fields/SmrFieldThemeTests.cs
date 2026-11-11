using System.Runtime.CompilerServices;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Fields;

// The error's colour follows the application's theme. Choosing one takes an Application, which
// makes itself current for the whole process, hence the collection that runs on its own.
[Collection(ApplicationCollection.Name)]
public sealed class SmrFieldThemeTests : IDisposable
{
    public void Dispose() => Application.Current = null;

    [Fact]
    public void TheErrorInTheDarkTheme_IsMaterialsDarkErrorRed()
    {
        var application = new Application { UserAppTheme = AppTheme.Dark };
        var field = new SmrField { ErrorText = "Required" };
        var page = new ContentPage { Content = field };
        page.Parent = application;

        Assert.Equal(Color.FromArgb("#F2B8B5"), Error(field).TextColor);
    }

    [Fact]
    public void TheErrorInTheLightTheme_IsMaterialsErrorRed()
    {
        var application = new Application { UserAppTheme = AppTheme.Light };
        var field = new SmrField { ErrorText = "Required" };
        var page = new ContentPage { Content = field };
        page.Parent = application;

        Assert.Equal(Color.FromArgb("#B3261E"), Error(field).TextColor);
    }

    [Fact]
    public void TheThemeChanging_ChangesTheErrorColour()
    {
        var application = new Application { UserAppTheme = AppTheme.Light };
        var field = new SmrField { ErrorText = "Required" };
        var page = new ContentPage { Content = field };
        page.Parent = application;

        application.UserAppTheme = AppTheme.Dark;

        Assert.Equal(Color.FromArgb("#F2B8B5"), Error(field).TextColor);
    }

    [Fact]
    public void AStyleForTheErrorClassInTheDarkTheme_ReplacesTheDefaultColor()
    {
        var application = new Application { UserAppTheme = AppTheme.Dark };
        application.Resources.Add(new Style(typeof(Label))
        {
            Class = "SmrFieldError",
            Setters = { new Setter { Property = Label.TextColorProperty, Value = Colors.Teal } },
        });
        var field = new SmrField { ErrorText = "Required" };
        var page = new ContentPage { Content = field };
        page.Parent = application;

        Assert.Equal(Colors.Teal, Error(field).TextColor);
    }

    // Written in XAML, the way an application writes a colour for each theme.
    [Fact]
    public void AnErrorStyleThatFollowsTheTheme_ReplacesTheDefaultInBoth()
    {
        const string Xaml = """
            <Grid xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
              <Grid.Resources>
                <Style TargetType="Label" Class="SmrFieldError">
                  <Setter Property="TextColor" Value="{AppThemeBinding Light=Teal, Dark=Purple}" />
                </Style>
              </Grid.Resources>
            </Grid>
            """;
        var application = new Application { UserAppTheme = AppTheme.Light };
        var grid = new Grid().LoadFromXaml(Xaml);
        var field = new SmrField { ErrorText = "Required" };
        grid.Add(field);
        var page = new ContentPage { Content = grid };
        page.Parent = application;

        Assert.Equal(Colors.Teal, Error(field).TextColor);

        application.UserAppTheme = AppTheme.Dark;

        Assert.Equal(Colors.Purple, Error(field).TextColor);
    }

    // What listens for the theme changing is the default template's root, for as long as the
    // application lives. Listening must keep neither it nor the field alive once the page holding
    // them has gone. The root is watched as well as the field: a child holds its parent weakly in
    // .NET MAUI, so a root kept alive would not keep the field alive with it.
    [Fact]
    public void ListeningForTheTheme_LetsAFieldAndItsTemplateGo()
    {
        var application = new Application { UserAppTheme = AppTheme.Light };

        var (field, root) = FieldOnAPageThatWentAway(application);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(field.IsAlive, "The field was kept alive after its page went away.");
        Assert.False(root.IsAlive, "The field's template was kept alive after its page went away.");
        GC.KeepAlive(application);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Field, WeakReference Root) FieldOnAPageThatWentAway(Application application)
    {
        var field = new SmrField { ErrorText = "Required" };
        var page = new ContentPage { Content = field };
        page.Parent = application;
        page.Parent = null;

        return (new WeakReference(field), new WeakReference(Assert.Single(((IVisualTreeElement)field).GetVisualChildren())));
    }

    private static Label Error(SmrField field)
    {
        var root = Assert.IsType<Grid>(Assert.Single(((IVisualTreeElement)field).GetVisualChildren()), exactMatch: false);
        return (Label)root.Children[2];
    }
}
