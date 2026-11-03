using System.Globalization;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// What the sample application should do when it is tapped.
/// </summary>
public abstract class TapTests : SamplePageTests
{
    /// <summary>
    /// The interval these tests dial the page up to, in milliseconds.
    /// </summary>
    /// <remarks>
    /// Far longer than the 300ms default, because a tap here is a round trip to the driver and
    /// back and several of those do not fit inside 300ms. Sending a whole burst in one command
    /// would avoid that, but nothing that does so reaches a .NET MAUI gesture recognizer: not the
    /// Windows driver's own click command, and not a touch pointer action. What the interval is
    /// worth in milliseconds is settled by TapSequenceTests against a fake clock. What these tests
    /// are here for is that a burst of a given length means what it should.
    /// </remarks>
    public const int SampleInterval = 2000;

    private const string PageTitle = "SmrTapBehavior";
    private const string BurstTarget = "BurstTarget";
    private const string DoubleTapTarget = "DoubleTapTarget";
    private const string WrappedDoubleTapTarget = "WrappedDoubleTapTarget";
    private const string Nothing = "nothing yet";

    private protected TapTests(SampleAppFixture app)
        : base(app)
    {
    }

    /// <remarks>
    /// A label waits out a whole burst before it changes, so it is given the interval on top.
    /// </remarks>
    protected override TimeSpan LabelPatience => TimeSpan.FromMilliseconds(SampleInterval + 5000);

    [Fact]
    public void ASingleTap_OpensTheInbox()
    {
        Open();

        Tap(BurstTarget, 1);

        Expect("LastAction", "inbox");
        Assert.Equal("1", Text("InboxCount"));
        Assert.Equal("0", Text("DebugRoomCount"));
    }

    [Fact]
    public void FiveTaps_OpenTheDebugRoomWithoutOpeningTheInbox()
    {
        Open();

        Tap(BurstTarget, 5);

        // The whole point of the behavior. Five taps must not navigate to the inbox on the way.
        Expect("LastAction", "debug room");
        Assert.Equal("0", Text("InboxCount"));
        Assert.Equal("1", Text("DebugRoomCount"));
    }

    [Fact]
    public void ABurstThatMatchesNeitherLine_AnswersNothing()
    {
        Open();

        Tap(BurstTarget, 3);
        Settle();

        // The inbox asks for exactly one tap, so three are not it, and three are not the five the
        // debug room wants either.
        Assert.Equal(Nothing, Text("LastAction"));
        Assert.Equal("0", Text("InboxCount"));
        Assert.Equal("0", Text("DebugRoomCount"));
    }

    [Fact]
    public void TwoTaps_AnswerALineThatAsksForExactlyTwo()
    {
        Open();

        Tap(DoubleTapTarget, 2);

        Expect("DoubleTapCount", "1");
        Assert.Equal("double tap", Text("LastAction"));
    }

    [Fact]
    public void ThreeTaps_AnswerNothingWhenTheOnlyLineAsksForExactlyTwo()
    {
        Open();

        Tap(DoubleTapTarget, 3);
        Settle();

        Assert.Equal("0", Text("DoubleTapCount"));
        Assert.Equal(Nothing, Text("LastAction"));
    }

    // The same two lines again, through types the sample derived from the library's own. The
    // counting is not what these are about; that a wrapped behavior and its lines still work is.

    [Fact]
    public void TwoTaps_AnswerAWrappedLineThatAsksForExactlyTwo()
    {
        Open();

        Tap(WrappedDoubleTapTarget, 2);

        Expect("WrappedDoubleTapCount", "1");
        Assert.Equal("wrapped double tap", Text("LastAction"));
    }

    [Fact]
    public void ThreeTaps_AnswerNothingWhenTheOnlyWrappedLineAsksForExactlyTwo()
    {
        Open();

        Tap(WrappedDoubleTapTarget, 3);
        Settle();

        Assert.Equal("0", Text("WrappedDoubleTapCount"));
        Assert.Equal(Nothing, Text("LastAction"));
    }

    /// <summary>
    /// Long enough for a burst to have settled, for the tests that assert nothing happened.
    /// </summary>
    private static void Settle() => Thread.Sleep(TimeSpan.FromMilliseconds(SampleInterval + 1500));

    /// <summary>
    /// Taps a target, one round trip per tap. See <see cref="SampleInterval"/> for why the page is
    /// dialled up far enough for that to work.
    /// </summary>
    private void Tap(string automationId, int times)
    {
        var target = Find(automationId);

        for (var i = 0; i < times; i++)
        {
            target.Click();
        }
    }

    /// <summary>
    /// Opens the page these tests are about, sets the interval they need, and puts the counters
    /// back to how they start.
    /// </summary>
    private void Open()
    {
        GoToPage();
        SetInterval(SampleInterval);

        Find("ResetButton").Click();
        Expect("LastAction", Nothing);
    }

    /// <summary>
    /// Picks the page out of the menu, unless it is already showing.
    /// </summary>
    private void GoToPage()
    {
        if (IsShowing(Ui.ByAutomationId(BurstTarget)))
        {
            return;
        }

        Ui.OpenMenu();
        Ui.Driver.FindElement(Ui.ByText(PageTitle)).Click();

        // The implicit wait covers the transition.
        Find(BurstTarget);
    }

    /// <summary>
    /// Types an interval into the page. The page uses its default when the box is empty, which is
    /// no use to a test that has to land several taps inside one.
    /// </summary>
    private void SetInterval(int milliseconds)
    {
        var wanted = milliseconds.ToString(CultureInfo.InvariantCulture);

        if (Text("EffectiveInterval") == wanted)
        {
            return;
        }

        var entry = Find("IntervalEntry");
        entry.Clear();
        entry.SendKeys(wanted);

        Ui.DismissKeyboard();
        Expect("EffectiveInterval", wanted);
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsTapTests(WindowsSampleApp app) : TapTests(app);

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidTapTests(AndroidSampleApp app) : TapTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosTapTests(IosSampleApp app) : TapTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystTapTests(MacCatalystSampleApp app) : TapTests(app);
