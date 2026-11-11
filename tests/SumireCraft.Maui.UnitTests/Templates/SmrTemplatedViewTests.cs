using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Templates;

// A templated view shows the template it brings until an application's own takes its place, by any
// of the routes .NET MAUI gives a template to a view. Each test names the template it expects to
// see by the text of the label at the top of it.
public class SmrTemplatedViewTests
{
    private static readonly ControlTemplate AppTemplate = Template("app");

    [Fact]
    public void AViewOnItsOwn_ShowsItsDefaultTemplate()
    {
        var view = new PlainView();

        Assert.Equal("plain", Shown(view));
    }

    [Fact]
    public void AViewOnAPage_ShowsItsDefaultTemplate()
    {
        var view = new PlainView();
        _ = new Grid { view };

        Assert.Equal("plain", Shown(view));
    }

    [Fact]
    public void AnImplicitStyle_ReplacesTheDefaultTemplate()
    {
        var view = new PlainView();
        _ = new Grid { Resources = new ResourceDictionary { TemplateStyle<PlainView>() }, Children = { view } };

        Assert.Equal("app", Shown(view));
    }

    [Fact]
    public void AnImplicitStyleAddedLater_ReplacesTheDefaultTemplate()
    {
        var view = new PlainView();
        var page = new Grid { view };

        page.Resources = new ResourceDictionary { TemplateStyle<PlainView>() };

        Assert.Equal("app", Shown(view));
    }

    [Fact]
    public void AnExplicitStyle_ReplacesTheDefaultTemplate()
    {
        var view = new PlainView { Style = TemplateStyle<PlainView>() };
        _ = new Grid { view };

        Assert.Equal("app", Shown(view));
    }

    [Fact]
    public void AnExplicitStyleSetLater_ReplacesTheDefaultTemplate()
    {
        var view = new PlainView();
        _ = new Grid { view };

        view.Style = TemplateStyle<PlainView>();

        Assert.Equal("app", Shown(view));
    }

    [Fact]
    public void ATemplateSetOnTheView_ReplacesTheDefaultTemplate()
    {
        var view = new PlainView { ControlTemplate = AppTemplate };
        _ = new Grid { view };

        Assert.Equal("app", Shown(view));
    }

    [Fact]
    public void ClearingTheTemplateSetOnTheView_BringsTheDefaultBack()
    {
        var view = new PlainView { ControlTemplate = AppTemplate };
        _ = new Grid { view };

        view.ClearValue(TemplatedView.ControlTemplateProperty);

        Assert.Equal("plain", Shown(view));
    }

    // .NET MAUI applies one implicit style for a view's own type, and an application's style for a
    // templated view usually says nothing about its template. Neither kind of style may take the
    // template away just by existing.

    [Fact]
    public void AnImplicitStyleWithoutATemplate_KeepsTheDefaultTemplate()
    {
        var view = new PlainView();
        _ = new Grid { Resources = new ResourceDictionary { MarginStyle<PlainView>() }, Children = { view } };

        Assert.Equal("plain", Shown(view));
        Assert.Equal(new Thickness(3), view.Margin);
    }

    [Fact]
    public void AnExplicitStyleWithoutATemplate_KeepsTheDefaultTemplate()
    {
        var view = new PlainView { Style = MarginStyle<PlainView>() };
        _ = new Grid { view };

        Assert.Equal("plain", Shown(view));
        Assert.Equal(new Thickness(3), view.Margin);
    }

    // An application can style every templated view at once, the way it styles every button.

    [Fact]
    public void AStyleForEveryTemplatedView_ReplacesTheDefaultTemplate()
    {
        var style = TemplateStyle<SmrTemplatedView>();
        style.ApplyToDerivedTypes = true;
        var view = new PlainView();
        _ = new Grid { Resources = new ResourceDictionary { style }, Children = { view } };

        Assert.Equal("app", Shown(view));
    }

    [Fact]
    public void AStyleForEveryTemplatedViewWithoutATemplate_StillApplies()
    {
        var style = MarginStyle<SmrTemplatedView>();
        style.ApplyToDerivedTypes = true;
        var view = new PlainView();
        _ = new Grid { Resources = new ResourceDictionary { style }, Children = { view } };

        Assert.Equal("plain", Shown(view));
        Assert.Equal(new Thickness(3), view.Margin);
    }

    // What a view brings is its own: it reaches neither the views put inside it nor another
    // templated view nested in it.

    [Fact]
    public void TheDefaultTemplate_DoesNotReachAContentViewInside()
    {
        var inner = new ContentView();
        var view = new PlainView { Content = inner };
        _ = new Grid { view };

        Assert.Null(inner.ControlTemplate);
    }

    [Fact]
    public void NestedTemplatedViews_EachShowTheirOwnDefaultTemplate()
    {
        var inner = new OtherView();
        var outer = new PlainView { Content = inner };
        _ = new Grid { outer };

        Assert.Equal("plain", Shown(outer));
        Assert.Equal("other", Shown(inner));
    }

    [Fact]
    public void ResourcesAddedToTheView_KeepTheDefaultTemplate()
    {
        var view = new PlainView();
        view.Resources.Add(new Style(typeof(Label)));
        _ = new Grid { view };

        Assert.Equal("plain", Shown(view));
    }

    // A style an application meant for every content view still reaches a templated view, but its
    // template does not replace the one the templated view brings: that template is the more
    // particular of the two.

    [Fact]
    public void AStyleForEveryContentView_LeavesTheDefaultTemplateButStillApplies()
    {
        var style = TemplateStyle<ContentView>();
        style.ApplyToDerivedTypes = true;
        style.Setters.Add(new Setter { Property = View.MarginProperty, Value = new Thickness(3) });
        var view = new PlainView();
        _ = new Grid { Resources = new ResourceDictionary { style }, Children = { view } };

        Assert.Equal("plain", Shown(view));
        Assert.Equal(new Thickness(3), view.Margin);
    }

    // An application may give a view a dictionary of its own rather than adding to the one it has.
    // The default has to survive that, without the view writing into a dictionary it was handed.

    [Fact]
    public void ReplacingTheResources_KeepsTheDefaultTemplate()
    {
        var view = new PlainView();
        _ = new Grid { view };

        view.Resources = new ResourceDictionary { new Style(typeof(Label)) };

        Assert.Equal("plain", Shown(view));
    }

    [Fact]
    public void ReplacingTheResourcesBeforeThePage_KeepsTheDefaultTemplate()
    {
        var view = new PlainView { Resources = new ResourceDictionary { new Style(typeof(Label)) } };
        _ = new Grid { view };

        Assert.Equal("plain", Shown(view));
    }

    [Fact]
    public void ReplacedResources_StillServeWhatTheyHold()
    {
        var label = new Label();
        var given = new ResourceDictionary
        {
            new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextColorProperty, Value = Colors.Teal } } },
        };
        given["Accent"] = Colors.Teal;
        var view = new PlainView { Content = label };
        _ = new Grid { view };

        view.Resources = given;

        Assert.True(view.Resources.TryGetValue("Accent", out var accent));
        Assert.Equal(Colors.Teal, accent);
        Assert.Equal(Colors.Teal, label.TextColor);
    }

    [Fact]
    public void ADictionaryHandedToTheView_IsLeftAsItWas()
    {
        var given = new ResourceDictionary { ["Accent"] = Colors.Teal };
        var view = new PlainView();

        view.Resources = given;

        Assert.Equal(["Accent"], given.Keys);
        Assert.Empty(given.MergedDictionaries);
    }

    [Fact]
    public void OneDictionaryHandedToTwoViews_LetsEachShowItsOwnDefault()
    {
        var shared = new ResourceDictionary { ["Accent"] = Colors.Teal };
        var plain = new PlainView { Resources = shared };
        var other = new OtherView { Resources = shared };
        _ = new Grid { plain, other };

        Assert.Equal("plain", Shown(plain));
        Assert.Equal("other", Shown(other));
    }

    [Fact]
    public void ANestedViewWithReplacedResources_ShowsItsOwnDefault()
    {
        var inner = new OtherView();
        var outer = new PlainView { Content = inner };
        _ = new Grid { outer };

        inner.Resources = [];

        Assert.Equal("other", Shown(inner));
    }

    [Fact]
    public void AViewWithReplacedResourcesPutInsideAnother_ShowsItsOwnDefault()
    {
        var inner = new OtherView { Resources = [] };
        var outer = new PlainView { Content = inner };
        _ = new Grid { outer };

        Assert.Equal("other", Shown(inner));
    }

    [Fact]
    public void AViewWithReplacedResourcesMovedIntoAnother_ShowsItsOwnDefault()
    {
        var inner = new OtherView();
        var first = new Grid { inner };
        inner.Resources = [];
        first.Remove(inner);

        var outer = new PlainView { Content = inner };
        _ = new Grid { outer };

        Assert.Equal("other", Shown(inner));
    }

    [Fact]
    public void TheResourcesOfAnotherTemplatedView_LeaveTheViewItsOwnDefault()
    {
        var plain = new PlainView();
        var other = new OtherView { Resources = plain.Resources };
        _ = new Grid { plain, other };

        Assert.Equal("plain", Shown(plain));
        Assert.Equal("other", Shown(other));
    }

    [Fact]
    public void AMissingDefaultTemplate_IsRefused()
    {
        Assert.Throws<ArgumentNullException>(() => new NoTemplateView());
    }

    internal static ControlTemplate Template(string text) =>
        new(() => new VerticalStackLayout { new Label { Text = text }, new ContentPresenter() });

    private static Style TemplateStyle<T>() => TemplateStyle<T>(AppTemplate);

    private static Style MarginStyle<T>() => new(typeof(T))
    {
        Setters = { new Setter { Property = View.MarginProperty, Value = new Thickness(3) } },
    };

    internal static Style TemplateStyle<T>(ControlTemplate template) => new(typeof(T))
    {
        Setters = { new Setter { Property = TemplatedView.ControlTemplateProperty, Value = template } },
    };

    // The text of the label the shown template starts with, or null when no template is shown.
    internal static string? Shown(ContentView view) =>
        ((IVisualTreeElement)view).GetVisualChildren() is [Layout { Count: > 0 } root, ..] && root[0] is Label label
            ? label.Text
            : null;

    private sealed class PlainView() : SmrTemplatedView(Template("plain"));

    private sealed class OtherView() : SmrTemplatedView(Template("other"));

    private sealed class NoTemplateView() : SmrTemplatedView(null!);
}
