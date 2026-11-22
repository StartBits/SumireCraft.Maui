using SumireCraft.Maui.Controls;
using static SumireCraft.Maui.UnitTests.Templates.SmrTemplatedViewTests;

namespace SumireCraft.Maui.UnitTests.Templates;

// Where applications usually keep their styles: App.xaml, at the top of every view's parents.
[Collection(ApplicationCollection.Name)]
public sealed class SmrTemplatedViewApplicationTests : IDisposable
{
    public void Dispose() => Application.Current = null;

    [Fact]
    public void AnImplicitStyleInTheApplication_ReplacesTheDefaultTemplate()
    {
        var view = new AppView();
        InApplication(view, TemplateStyle<AppView>(Template("app")));

        Assert.Equal("app", Shown(view));
    }

    [Fact]
    public void AnImplicitStyleWithoutATemplateInTheApplication_KeepsTheDefaultTemplate()
    {
        var view = new AppView();
        InApplication(view, new Style(typeof(AppView)) { Setters = { new Setter { Property = View.MarginProperty, Value = new Thickness(3) } } });

        Assert.Equal("default", Shown(view));
        Assert.Equal(new Thickness(3), view.Margin);
    }

    // Puts the view on a page whose parent is an application holding the style, as in a running
    // application, where the page's window has the application for its parent.
    private static void InApplication(View view, Style style)
    {
        var application = new Application();
        application.Resources.Add(style);
        var page = new ContentPage { Content = view };
        page.Parent = application;
    }

    private sealed class AppView() : SmrTemplatedView(Template("default"));
}
