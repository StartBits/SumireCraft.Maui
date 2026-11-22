using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Templates;

// The usual way an application gives a view resources of its own in XAML: a dictionary of its
// own as the value of the property, not entries added to the one the view already has.
[Collection(XamlLoadingCollection.Name)]
public class SmrTemplatedViewXamlTests
{
    [Fact]
    public void ADictionaryWrittenInXaml_KeepsTheDefaultTemplate()
    {
        const string Xaml = """
            <XamlTemplatedView
                xmlns="clr-namespace:SumireCraft.Maui.UnitTests.Templates;assembly=SumireCraft.Maui.UnitTests"
                xmlns:maui="http://schemas.microsoft.com/dotnet/2021/maui"
                xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
              <XamlTemplatedView.Resources>
                <maui:ResourceDictionary>
                  <maui:Color x:Key="Accent">Teal</maui:Color>
                </maui:ResourceDictionary>
              </XamlTemplatedView.Resources>
            </XamlTemplatedView>
            """;

        var view = new XamlTemplatedView().LoadFromXaml(Xaml);
        _ = new Grid { view };

        Assert.True(view.Resources.TryGetValue("Accent", out _));
        var root = Assert.IsType<VerticalStackLayout>(Assert.Single(((IVisualTreeElement)view).GetVisualChildren()));
        Assert.Equal("xaml", Assert.IsType<Label>(root[0]).Text);
    }
}

public class XamlTemplatedView : SmrTemplatedView
{
    private static readonly ControlTemplate DefaultTemplate =
        new(() => new VerticalStackLayout { new Label { Text = "xaml" }, new ContentPresenter() });

    public XamlTemplatedView()
        : base(DefaultTemplate)
    {
    }
}
