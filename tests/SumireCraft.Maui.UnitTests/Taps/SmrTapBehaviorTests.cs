using System.Runtime.CompilerServices;
using Microsoft.Extensions.Time.Testing;
using SumireCraft.Maui.Controls;
using SumireCraft.Maui.Taps;

namespace SumireCraft.Maui.UnitTests.Taps;

// The wiring this behavior adds on top of TapSequence, whose tests cover the counting rules. A tap
// is the recognizer's command running, which is what a platform does with a real one; whether a
// real tap reaches the recognizer is for the UI tests to show.
public class SmrTapBehaviorTests
{
    private const int Interval = SmrTapBehavior.DefaultInterval;

    [Fact]
    public void ATapLine_AsksForOneExactTapUntilToldOtherwise()
    {
        var tap = new SmrTap();

        Assert.Equal(1, tap.Count);
        Assert.Equal(TapMatch.Exactly, tap.Match);
        Assert.Null(tap.Command);
        Assert.Null(tap.CommandParameter);
    }

    [Fact]
    public void TheDefaultInterval_IsThreeHundredMilliseconds()
    {
        Assert.Equal(300, SmrTapBehavior.DefaultInterval);
        Assert.Equal(300, new SmrTapBehavior().Interval);
    }

    [Fact]
    public void ABehavior_StartsWithNoTapLines()
    {
        Assert.Empty(new SmrTapBehavior().Taps);
    }

    [Fact]
    public void Attaching_GivesTheViewOneTapRecognizer()
    {
        var (view, _, _) = Attach();

        Assert.Single(view.GestureRecognizers.OfType<TapGestureRecognizer>());
    }

    [Fact]
    public void Detaching_TakesTheRecognizerAway()
    {
        var (view, behavior, _) = Attach();

        view.Behaviors.Remove(behavior);

        Assert.Empty(view.GestureRecognizers);
    }

    [Fact]
    public void ABurst_RunsTheLineItMatchesAndNoOther()
    {
        var inbox = new Counter();
        var debugRoom = new Counter();
        var (view, _, time) = Attach(
            new SmrTap { Count = 1, Command = inbox.Command },
            new SmrTap { Count = 5, Match = TapMatch.AtLeast, Command = debugRoom.Command });

        Tap(view, 5);
        Settle(time);

        Assert.Equal(0, inbox.Runs);
        Assert.Equal(1, debugRoom.Runs);
    }

    [Fact]
    public void NothingRuns_UntilTheBurstHasSettled()
    {
        var counter = new Counter();
        var (view, _, time) = Attach(new SmrTap { Count = 2, Command = counter.Command });

        Tap(view, 2);
        time.Advance(TimeSpan.FromMilliseconds(Interval - 1));

        Assert.Equal(0, counter.Runs);
    }

    [Fact]
    public void TheBurst_WaitsAsLongAsTheIntervalSays()
    {
        var counter = new Counter();
        var (view, behavior, time) = Attach(new SmrTap { Count = 2, Command = counter.Command });
        behavior.Interval = 1000;

        Tap(view, 2);
        time.Advance(TimeSpan.FromMilliseconds(999));
        Assert.Equal(0, counter.Runs);

        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.Equal(1, counter.Runs);
    }

    [Fact]
    public void ChangingTheInterval_DropsWhatWasHalfCounted()
    {
        var counter = new Counter();
        var (view, behavior, time) = Attach(new SmrTap { Count = 2, Command = counter.Command });

        Tap(view, 1);
        behavior.Interval = 500;
        Tap(view, 1);
        time.Advance(TimeSpan.FromMilliseconds(501));

        // One tap before the change and one after: neither burst is the two the line asks for.
        Assert.Equal(0, counter.Runs);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void AnIntervalOfZeroOrLess_FallsBackToTheDefault(int interval)
    {
        var behavior = new SmrTapBehavior { Interval = 500 };

        behavior.Interval = interval;

        Assert.Equal(SmrTapBehavior.DefaultInterval, behavior.Interval);
    }

    /// <remarks>
    /// A bound value arrives while the page is being put together, where an exception would take
    /// the page down with it.
    /// </remarks>
    [Fact]
    public void ABoundIntervalOfZero_StillCountsWithTheDefault()
    {
        var counter = new Counter();
        var (view, behavior, time) = Attach(new SmrTap { Count = 2, Command = counter.Command });
        behavior.SetBinding(SmrTapBehavior.IntervalProperty, nameof(Settings.Interval));

        view.BindingContext = new Settings(Interval: 0);
        Tap(view, 2);
        time.Advance(TimeSpan.FromMilliseconds(Interval - 1));
        Assert.Equal(0, counter.Runs);

        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.Equal(1, counter.Runs);
    }

    [Fact]
    public void TheParameter_IsPassedToTheCommand()
    {
        object? received = null;
        var (view, _, time) = Attach(new SmrTap
        {
            Command = new Command<object>(parameter => received = parameter),
            CommandParameter = "inbox",
        });

        Tap(view, 1);
        Settle(time);

        Assert.Equal("inbox", received);
    }

    [Fact]
    public void ALineThatCannotRun_RunsNothingRatherThanTheLineBelowIt()
    {
        var blocked = new Counter(canRun: false);
        var fallback = new Counter();
        var (view, _, time) = Attach(
            new SmrTap { Count = 3, Match = TapMatch.AtLeast, Command = blocked.Command },
            new SmrTap { Count = 1, Match = TapMatch.AtLeast, Command = fallback.Command });

        Tap(view, 3);
        Settle(time);

        Assert.Equal(0, blocked.Runs);
        Assert.Equal(0, fallback.Runs);
    }

    [Fact]
    public void ACommandBoundOnALine_ResolvesAgainstTheViewsContext()
    {
        var page = new Counter();
        var line = new SmrTap();
        line.SetBinding(SmrTap.CommandProperty, nameof(Counter.Command));
        var (view, _, time) = Attach(line);

        view.BindingContext = page;
        Tap(view, 1);
        Settle(time);

        Assert.Equal(1, page.Runs);
    }

    [Fact]
    public void ALineAddedAfterAttaching_GetsTheViewsContextToo()
    {
        var page = new Counter();
        var (view, behavior, time) = Attach();
        view.BindingContext = page;

        var line = new SmrTap();
        line.SetBinding(SmrTap.CommandProperty, nameof(Counter.Command));
        behavior.Taps.Add(line);
        Tap(view, 1);
        Settle(time);

        Assert.Equal(1, page.Runs);
    }

    [Fact]
    public void ABurstUnderWayWhenTheBehaviorIsDetached_StillRuns()
    {
        var counter = new Counter();
        var (view, behavior, time) = Attach(new SmrTap { Count = 2, Command = counter.Command });

        Tap(view, 2);
        view.Behaviors.Remove(behavior);
        Settle(time);

        Assert.Equal(1, counter.Runs);
    }

    [Fact]
    public void OneBehaviorOnTwoViews_GivesEachItsOwnRecognizer()
    {
        var (first, behavior, _) = Attach();
        var second = AlsoAttach(behavior);

        Assert.Single(first.GestureRecognizers.OfType<TapGestureRecognizer>());
        Assert.Single(second.GestureRecognizers.OfType<TapGestureRecognizer>());
    }

    [Fact]
    public void TwoViews_CountTheirTapsSeparately()
    {
        var counter = new Counter();
        var (first, behavior, time) = Attach(new SmrTap { Count = 2, Command = counter.Command });
        var second = AlsoAttach(behavior);

        Tap(first, 1);
        Tap(second, 1);
        Settle(time);

        // One tap on each is not two taps on either.
        Assert.Equal(0, counter.Runs);
    }

    [Fact]
    public void TakingTheBehaviorOffTheFirstView_LeavesTheSecondAnswering()
    {
        var counter = new Counter();
        var (first, behavior, time) = Attach(new SmrTap { Command = counter.Command });
        var second = AlsoAttach(behavior);

        first.Behaviors.Remove(behavior);
        Tap(second, 1);
        Settle(time);

        Assert.Equal(1, counter.Runs);
    }

    [Fact]
    public void TakingTheBehaviorOffTheSecondView_LeavesTheFirstAnswering()
    {
        var counter = new Counter();
        var (first, behavior, time) = Attach(new SmrTap { Command = counter.Command });
        var second = AlsoAttach(behavior);

        second.Behaviors.Remove(behavior);
        Tap(first, 1);
        Settle(time);

        Assert.Equal(1, counter.Runs);
    }

    [Fact]
    public void TakingTheBehaviorOffOneView_TakesThatViewsRecognizerAndNoOther()
    {
        var (first, behavior, _) = Attach();
        var second = AlsoAttach(behavior);

        first.Behaviors.Remove(behavior);

        Assert.Empty(first.GestureRecognizers);
        Assert.Single(second.GestureRecognizers.OfType<TapGestureRecognizer>());
    }

    [Fact]
    public void ALine_RunsAgainstTheContextOfTheViewThatWasTapped()
    {
        var firstPage = new Counter();
        var secondPage = new Counter();
        var line = new SmrTap();
        line.SetBinding(SmrTap.CommandProperty, nameof(Counter.Command));
        var (first, behavior, time) = Attach(line);
        var second = AlsoAttach(behavior);
        first.BindingContext = firstPage;
        second.BindingContext = secondPage;

        Tap(second, 1);
        Settle(time);

        Assert.Equal(0, firstPage.Runs);
        Assert.Equal(1, secondPage.Runs);

        Tap(first, 1);
        Settle(time);

        Assert.Equal(1, firstPage.Runs);
        Assert.Equal(1, secondPage.Runs);
    }

    [Fact]
    public void AParameterBoundOnALine_ResolvesAgainstTheViewThatWasTappedToo()
    {
        var received = new List<object?>();
        var line = new SmrTap { Command = new Command<object>(received.Add) };
        line.SetBinding(SmrTap.CommandParameterProperty, ".");
        var (first, behavior, time) = Attach(line);
        var second = AlsoAttach(behavior);
        first.BindingContext = "first item";
        second.BindingContext = "second item";

        Tap(second, 1);
        Settle(time);
        Tap(first, 1);
        Settle(time);

        Assert.Equal(["second item", "first item"], received);
    }

    [Fact]
    public void AContextSetOnTheBehavior_WinsOverTheViewThatWasTapped()
    {
        var page = new Counter();
        var item = new Counter();
        var line = new SmrTap();
        line.SetBinding(SmrTap.CommandProperty, nameof(Counter.Command));
        var (view, behavior, time) = Attach(line);
        behavior.BindingContext = page;
        view.BindingContext = item;

        Tap(view, 1);
        Settle(time);

        Assert.Equal(1, page.Runs);
        Assert.Equal(0, item.Runs);
    }

    /// <remarks>
    /// The way out of an item template to the page's view model, written in XAML as a binding with
    /// a source on the behavior's <c>BindingContext</c>.
    /// </remarks>
    [Fact]
    public void AContextBoundOnTheBehavior_WinsOverTheViewThatWasTapped()
    {
        var page = new Counter();
        var item = new Counter();
        var line = new SmrTap();
        line.SetBinding(SmrTap.CommandProperty, nameof(Counter.Command));
        var (view, behavior, time) = Attach(line);
        behavior.SetBinding(BindableObject.BindingContextProperty, new Binding(".", source: page));
        view.BindingContext = item;

        Tap(view, 1);
        Settle(time);

        Assert.Equal(1, page.Runs);
        Assert.Equal(0, item.Runs);
    }

    [Fact]
    public void ABurstUnderWayOnAViewThatIsLeft_StillRunsAgainstThatViewsContext()
    {
        var firstPage = new Counter();
        var secondPage = new Counter();
        var line = new SmrTap { Count = 2 };
        line.SetBinding(SmrTap.CommandProperty, nameof(Counter.Command));
        var (first, behavior, time) = Attach(line);
        var second = AlsoAttach(behavior);
        first.BindingContext = firstPage;
        second.BindingContext = secondPage;

        Tap(first, 2);
        first.Behaviors.Remove(behavior);
        Settle(time);

        Assert.Equal(1, firstPage.Runs);
        Assert.Equal(0, secondPage.Runs);
    }

    [Fact]
    public void ChangingTheInterval_ReachesEveryView()
    {
        var counter = new Counter();
        var (first, behavior, time) = Attach(new SmrTap { Count = 2, Command = counter.Command });
        var second = AlsoAttach(behavior);
        behavior.Interval = 1000;

        Tap(first, 2);
        Tap(second, 2);
        time.Advance(TimeSpan.FromMilliseconds(999));
        Assert.Equal(0, counter.Runs);

        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.Equal(2, counter.Runs);
    }

    [Fact]
    public void TheBehaviorsOwnContext_FollowsTheViewItWasAttachedToLast()
    {
        var (first, behavior, _) = Attach();
        first.BindingContext = "first";
        var second = AlsoAttach(behavior);
        second.BindingContext = "second";

        Assert.Equal("second", behavior.BindingContext);

        first.BindingContext = "first again";

        Assert.Equal("second", behavior.BindingContext);
    }

    [Fact]
    public void WhenTheViewItFollowsIsLeft_TheBehaviorFollowsTheLatestOfTheRest()
    {
        var (first, behavior, _) = Attach();
        first.BindingContext = "first";
        var second = AlsoAttach(behavior);
        second.BindingContext = "second";
        var third = AlsoAttach(behavior);
        third.BindingContext = "third";

        third.Behaviors.Remove(behavior);

        Assert.Equal("second", behavior.BindingContext);

        second.BindingContext = "second again";

        Assert.Equal("second again", behavior.BindingContext);
    }

    /// <remarks>
    /// A style is where sharing really happens: the one behavior in it is attached to every view the
    /// style is applied to, without passing through the view's own <c>Behaviors</c>.
    /// </remarks>
    [Fact]
    public void OneBehaviorInAStyle_AnswersEachViewWithItsOwnContext()
    {
        var firstItem = new Counter();
        var secondItem = new Counter();
        var time = new FakeTimeProvider();
        var line = new SmrTap();
        line.SetBinding(SmrTap.CommandProperty, nameof(Counter.Command));
        var behavior = new SmrTapBehavior { TimeProvider = time, Taps = { line } };
        var style = new Style(typeof(Label)) { Behaviors = { behavior } };
        var first = new Label { Style = style, BindingContext = firstItem };
        var second = new Label { Style = style, BindingContext = secondItem };

        Tap(first, 1);
        Settle(time);
        Tap(second, 1);
        Settle(time);

        Assert.Equal(1, firstItem.Runs);
        Assert.Equal(1, secondItem.Runs);

        first.Style = null;
        Tap(second, 1);
        Settle(time);

        Assert.Empty(first.GestureRecognizers);
        Assert.Equal(2, secondItem.Runs);
    }

    /// <remarks>
    /// A behavior shared through a resource can outlive every page it was put on, and .NET MAUI
    /// does not take a behavior off a view whose page is closed. Holding on to the views would keep
    /// every closed page alive for as long as the resource.
    /// </remarks>
    [Fact]
    public void ViewsThatGoAwayWithoutLettingGo_AreNotKeptAliveByTheBehavior()
    {
        var behavior = new SmrTapBehavior { TimeProvider = new FakeTimeProvider() };

        var views = ViewsLeftAttachedTo(behavior);
        ForceCollection();

        Assert.All(views, view => Assert.False(view.TryGetTarget(out _)));
        GC.KeepAlive(behavior);
    }

    /// <summary>
    /// Puts <paramref name="behavior"/> on a fresh view as well, beside whichever views it is on
    /// already.
    /// </summary>
    internal static View AlsoAttach(SmrTapBehavior behavior)
    {
        var view = new Label();
        view.Behaviors.Add(behavior);

        return view;
    }

    /// <summary>
    /// Puts <paramref name="behavior"/> on two views, taps both, and lets go of them without taking
    /// the behavior off. Kept in its own non-inlined method so no local in the caller keeps them
    /// alive.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<View>[] ViewsLeftAttachedTo(SmrTapBehavior behavior)
    {
        behavior.Taps.Add(new SmrTap());

        var first = AlsoAttach(behavior);
        var second = AlsoAttach(behavior);
        first.BindingContext = "first";
        second.BindingContext = "second";
        Tap(first, 1);
        Tap(second, 1);
        Settle((FakeTimeProvider)behavior.TimeProvider);

        return [new WeakReference<View>(first), new WeakReference<View>(second)];
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Puts a behavior holding <paramref name="taps"/> on a fresh view, measuring with a clock the
    /// test moves by hand.
    /// </summary>
    internal static (View View, SmrTapBehavior Behavior, FakeTimeProvider Time) Attach(params SmrTap[] taps) =>
        Attach(new SmrTapBehavior(), taps);

    /// <inheritdoc cref="Attach(SmrTap[])"/>
    internal static (View View, SmrTapBehavior Behavior, FakeTimeProvider Time) Attach(
        SmrTapBehavior behavior,
        params SmrTap[] taps)
    {
        var time = new FakeTimeProvider();
        behavior.TimeProvider = time;

        foreach (var tap in taps)
        {
            behavior.Taps.Add(tap);
        }

        var view = new Label();
        view.Behaviors.Add(behavior);

        return (view, behavior, time);
    }

    internal static void Tap(View view, int times)
    {
        var recognizer = view.GestureRecognizers.OfType<TapGestureRecognizer>().Single();

        for (var i = 0; i < times; i++)
        {
            recognizer.Command!.Execute(recognizer.CommandParameter);
        }
    }

    /// <summary>
    /// Moves the clock past the default interval, so whatever burst is under way is read.
    /// </summary>
    internal static void Settle(FakeTimeProvider time) => time.Advance(TimeSpan.FromMilliseconds(Interval + 1));

    private sealed record Settings(int Interval);

    internal sealed class Counter
    {
        /// <param name="canRun">
        /// What the command answers when asked whether it can run. Executing it counts a run
        /// either way, so a run that should have been refused still shows.
        /// </param>
        public Counter(bool canRun = true) => Command = new Command(() => Runs++, () => canRun);

        public Command Command { get; }

        public int Runs { get; private set; }
    }
}
