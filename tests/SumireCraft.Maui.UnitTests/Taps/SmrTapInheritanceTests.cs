using SumireCraft.Maui.Controls;
using SumireCraft.Maui.Taps;

namespace SumireCraft.Maui.UnitTests.Taps;

// What an application gets when it wraps the behavior in types of its own, so that the day this
// library goes away there is one place to change rather than every page. "Abc" stands for that
// application.
[Collection(XamlLoadingCollection.Name)]
public class SmrTapInheritanceTests
{
    [Fact]
    public void ADerivedBehavior_TakesDerivedLines()
    {
        var behavior = new AbcTapBehavior
        {
            Taps = { new AbcTap { Count = 2, Match = TapMatch.Exactly } },
        };

        var tap = Assert.IsType<AbcTap>(Assert.Single(behavior.Taps));
        Assert.Equal(2, tap.Count);
        Assert.Equal(SmrTapBehavior.DefaultInterval, behavior.Interval);
    }

    [Fact]
    public void ADerivedBehaviorOnAView_RunsItsDerivedLines()
    {
        var page = new SmrTapBehaviorTests.Counter();
        var line = new AbcTap { Count = 2 };
        line.SetBinding(SmrTap.CommandProperty, nameof(SmrTapBehaviorTests.Counter.Command));
        var (view, _, time) = SmrTapBehaviorTests.Attach(new AbcTapBehavior(), line);

        view.BindingContext = page;
        SmrTapBehaviorTests.Tap(view, 2);
        SmrTapBehaviorTests.Settle(time);

        Assert.Equal(1, page.Runs);
    }

    [Fact]
    public void ADerivedBehavior_TakesItsLinesStraightInsideItInXaml()
    {
        const string Xaml = """
            <AbcTapBehavior
                xmlns="clr-namespace:SumireCraft.Maui.UnitTests.Taps;assembly=SumireCraft.Maui.UnitTests"
                Interval="500">
                <AbcTap Count="2" Match="Exactly" />
                <AbcTap Count="5" Match="AtLeast" />
            </AbcTapBehavior>
            """;

        var behavior = new AbcTapBehavior().LoadFromXaml(Xaml);

        Assert.Equal(500, behavior.Interval);
        Assert.Collection(
            behavior.Taps,
            first =>
            {
                Assert.IsType<AbcTap>(first);
                Assert.Equal(2, first.Count);
                Assert.Equal(TapMatch.Exactly, first.Match);
            },
            second =>
            {
                Assert.IsType<AbcTap>(second);
                Assert.Equal(5, second.Count);
                Assert.Equal(TapMatch.AtLeast, second.Match);
            });
    }
}

public class AbcTapBehavior : SmrTapBehavior;

public class AbcTap : SmrTap;
