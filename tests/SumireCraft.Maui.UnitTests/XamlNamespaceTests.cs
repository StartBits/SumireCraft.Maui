using System.Reflection;
using SumireCraft.Maui.Controls;
using XmlnsPrefixAttribute = Microsoft.Maui.Controls.XmlnsPrefixAttribute;

namespace SumireCraft.Maui.UnitTests;

// The URI an application writes for the widgets in XAML. Once published it cannot change without
// breaking every page that uses it, so these tests pin it down character for character.
[Collection(XamlLoadingCollection.Name)]
public class XamlNamespaceTests
{
    private const string Uri = "https://github.com/startbits/SumireCraft.Maui";

    [Fact]
    public void TheUri_ReachesTheWidgetsInXaml()
    {
        const string Xaml = $"""
            <ContentView
                xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                xmlns:smr="{Uri}">
                <smr:SmrBadgeView Text="3" Placement="TopStart">
                    <Label Text="Inbox">
                        <Label.Behaviors>
                            <smr:SmrTapBehavior>
                                <smr:SmrTap Count="2" />
                            </smr:SmrTapBehavior>
                        </Label.Behaviors>
                    </Label>
                </smr:SmrBadgeView>
            </ContentView>
            """;

        var view = new ContentView().LoadFromXaml(Xaml);

        var badge = Assert.IsType<SmrBadgeView>(view.Content);
        Assert.Equal("3", badge.Text);
        Assert.Equal(BadgePlacement.TopStart, badge.Placement);
        var label = Assert.IsType<Label>(badge.Content);
        var behavior = Assert.IsType<SmrTapBehavior>(Assert.Single(label.Behaviors));
        Assert.Equal(2, Assert.Single(behavior.Taps).Count);
    }

    [Fact]
    public void TheUri_SuggestsSmrAsItsPrefix()
    {
        // Not the only prefix on the assembly: .NET MAUI adds its own for the global namespace.
        var prefix = Assert.Single(
            typeof(SmrBadgeView).Assembly.GetCustomAttributes<XmlnsPrefixAttribute>(),
            attribute => attribute.XmlNamespace == Uri);

        Assert.Equal("smr", prefix.Prefix);
    }
}
