using System.Globalization;
using OpenQA.Selenium.Appium;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// What the sample application should show when a view is badged.
/// </summary>
public abstract class BadgeTests : SamplePageTests
{
    private protected const string BadgeText = "7";
    private protected const string DotBadge = "DotBadge";
    private protected const string DotBadgedButton = "DotBadgedButton";
    private const string PageTitle = "SmrBadgeView";
    private protected const string BadgedButton = "BadgedButton";
    private protected const string Overflowed = "99+";
    private const string StyledBadgedButton = "StyledBadgedButton";
    private const string StyledBadgeText = "NEW";
    private protected const string PageScroll = "BadgePageScroll";
    private const string InboxPresses = "InboxPresses";
    private const string PressesCaption = "Inbox pressed: ";

    private protected BadgeTests(SampleAppFixture app)
        : base(app)
    {
    }

    [Fact]
    public void ABadge_ShowsTheCount()
    {
        Open();

        SetCount(BadgeText);

        Assert.True(IsShowing(Ui.ByText(BadgeText)), "The badge was not drawn.");
    }

    [Fact]
    public void ABadgeAtZero_IsNotDrawn()
    {
        Open();
        SetCount(BadgeText);

        SetCount("0");

        Assert.False(IsShowing(Ui.ByText(BadgeText)), "The badge was still drawn at zero.");
        Assert.False(IsShowing(Ui.ByText("0")), "The badge was drawn showing zero.");
    }

    /// <remarks>
    /// Nothing on the page may read 99+ before the count gets there, or finding it afterwards
    /// would prove nothing about where it came from.
    /// </remarks>
    [Fact]
    public void ACountAboveTheMaximum_ReadsAsTheMaximumPlus()
    {
        Open();

        SetCount(BadgeText);
        Assert.False(IsShowing(Ui.ByText(Overflowed)), "Something read 99+ before the count was over 99.");

        SetCount("100");
        Assert.True(IsShowing(Ui.ByText(Overflowed)), "A count of 100 did not read 99+.");
    }

    /// <remarks>
    /// The dot has no text, so it is only ever hidden by <c>IsBadgeVisible</c>: this is what shows
    /// that a badge drawn from a template takes the counter's word for whether to appear.
    /// </remarks>
    [Fact]
    public void ABadgeDrawnFromATemplate_FollowsTheCount()
    {
        Open();

        SetCount(BadgeText);
        Assert.True(IsShowing(Ui.ByAutomationId(DotBadge)), "The dot was not drawn.");

        SetCount("0");
        Assert.False(IsShowing(Ui.ByAutomationId(DotBadge)), "The dot was still drawn at zero.");
    }

    /// <remarks>
    /// The styled badge sits straight in the page's column around a button of a set width, which
    /// is the plain way to write one. The control itself stretches across the column there, and the
    /// badge has to follow the button rather than the edge of the page.
    /// </remarks>
    [Fact]
    public void ABadge_FollowsWhatItMarksRatherThanTheSpaceAroundIt()
    {
        Open();

        try
        {
            var content = Ui.BringIntoView(Ui.ByAutomationId(StyledBadgedButton), Ui.ByAutomationId(PageScroll));
            var badge = Ui.Driver.FindElement(Ui.ByText(StyledBadgeText));

            var contentMiddleX = content.Location.X + (content.Size.Width / 2);
            var contentRight = content.Location.X + content.Size.Width;
            var badgeMiddleX = badge.Location.X + (badge.Size.Width / 2);

            Assert.True(
                badgeMiddleX > contentMiddleX && badgeMiddleX < contentRight + badge.Size.Width,
                $"The badge is at x={badgeMiddleX}, and what it marks spans {content.Location.X} to {contentRight}.");
        }
        finally
        {
            Ui.ScrollToTop(Ui.ByAutomationId(PageScroll));
        }
    }

    /// <remarks>
    /// <para>
    /// The page is turned right to left, as it would be in Arabic or Hebrew. The styled badge's
    /// top-end corner is then the top-left one, and the offset, which moves it back towards the
    /// start, has to move it towards the right there: the same XAML means the same thing whichever
    /// way the page runs. So the middle of the badge is as far in from the left edge of what it
    /// marks as it was from the right edge before. An offset that did not turn with the corner
    /// would leave the two apart by twice its size.
    /// </para>
    /// <para>
    /// The text, which is what can be found, lies inside the button at that offset. That matters
    /// on Android, which reports only the part of an element inside its parent.
    /// </para>
    /// </remarks>
    [Fact]
    public void InARightToLeftPage_TheOffsetTurnsWithTheCorner()
    {
        const int Tolerance = 2;

        Open();

        try
        {
            var (content, badge) = StyledBadge();
            var inFromTheRight = content.Location.X + content.Size.Width - Middle(badge);

            Ui.ScrollToTop(Ui.ByAutomationId(PageScroll));
            SetFlowDirection("RightToLeft");

            (content, badge) = StyledBadge();
            var inFromTheLeft = Middle(badge) - content.Location.X;

            Assert.True(
                Math.Abs(inFromTheLeft - inFromTheRight) <= Tolerance,
                $"The middle of the badge is {inFromTheLeft} in from the left edge right to left, and was {inFromTheRight} in from the right edge left to right.");
        }
        finally
        {
            Ui.ScrollToTop(Ui.ByAutomationId(PageScroll));
            SetFlowDirection("LeftToRight");
        }

        (AppiumElement Content, AppiumElement Badge) StyledBadge() =>
            (Ui.BringIntoView(Ui.ByAutomationId(StyledBadgedButton), Ui.ByAutomationId(PageScroll)),
             Ui.Driver.FindElement(Ui.ByText(StyledBadgeText)));

        static double Middle(AppiumElement element) => element.Location.X + (element.Size.Width / 2.0);
    }

    /// <remarks>
    /// The badge covers the corner of what it marks, which is where a person presses a badged
    /// button as often as anywhere. The middle of the badge, where the press lands, is a quarter of
    /// it in from the corner and so over the button. The middle of the button is pressed first, so
    /// that a press that does not count is the badge's doing rather than the counter's.
    /// </remarks>
    [Fact]
    public void PressingTheBadge_PressesWhatItMarks()
    {
        Open();
        Find("PlacementTopEnd").Click();
        Expect("CurrentPlacement", "TopEnd");
        SetCount(BadgeText);
        var before = int.Parse(Text(InboxPresses)[PressesCaption.Length..], CultureInfo.InvariantCulture);

        Find(BadgedButton).Click();
        Expect(InboxPresses, Presses(before + 1));

        Ui.Driver.FindElement(Ui.ByText(BadgeText)).Click();
        Expect(InboxPresses, Presses(before + 2));

        static string Presses(int count) => PressesCaption + count.ToString(CultureInfo.InvariantCulture);
    }

    [Fact]
    public void EachPlacement_PutsTheBadgeInThatCorner()
    {
        Open();
        SetCount(BadgeText);

        ExpectCorner("TopStart");
        ExpectCorner("TopEnd");
        ExpectCorner("BottomEnd");
        ExpectCorner("BottomStart");
    }

    /// <summary>
    /// Asks for a corner and checks the badge is on the right side of the middle of what it marks,
    /// horizontally and vertically.
    /// </summary>
    /// <remarks>
    /// Comparing against the middle rather than against an expected position: where exactly a
    /// badge lands is a matter of size and rounding on each platform, but which half of the thing
    /// it is in is the whole of what placement means.
    /// </remarks>
    private void ExpectCorner(string name)
    {
        Find($"Placement{name}").Click();
        Expect("CurrentPlacement", name);

        var content = Find(BadgedButton);
        var badge = Ui.Driver.FindElement(Ui.ByText(BadgeText));

        var contentMiddleX = content.Location.X + (content.Size.Width / 2);
        var contentMiddleY = content.Location.Y + (content.Size.Height / 2);
        var badgeMiddleX = badge.Location.X + (badge.Size.Width / 2);
        var badgeMiddleY = badge.Location.Y + (badge.Size.Height / 2);

        var wantsStart = name.EndsWith("Start", StringComparison.Ordinal);
        var wantsTop = name.StartsWith("Top", StringComparison.Ordinal);

        Assert.True(
            wantsStart ? badgeMiddleX < contentMiddleX : badgeMiddleX > contentMiddleX,
            $"{name}: the badge is at x={badgeMiddleX} and the middle of what it marks is at {contentMiddleX}.");

        Assert.True(
            wantsTop ? badgeMiddleY < contentMiddleY : badgeMiddleY > contentMiddleY,
            $"{name}: the badge is at y={badgeMiddleY} and the middle of what it marks is at {contentMiddleY}.");
    }

    private protected void Open()
    {
        if (IsShowing(Ui.ByAutomationId(BadgedButton)))
        {
            return;
        }

        Ui.OpenMenu();
        Ui.Driver.FindElement(Ui.ByText(PageTitle)).Click();

        Find(BadgedButton);
    }

    /// <remarks>
    /// Clearing the entry on the way leaves the count where it was, since an empty entry is not a
    /// number to convert.
    /// </remarks>
    private protected void SetCount(string count)
    {
        var entry = Find("BadgeCountEntry");
        entry.Clear();
        entry.SendKeys(count);

        Ui.DismissKeyboard();
    }

    private protected void SetFlowDirection(string direction)
    {
        if (Text("CurrentFlowDirection") != direction)
        {
            Find("FlipFlowDirection").Click();
        }

        Expect("CurrentFlowDirection", direction);
    }
}

/// <summary>
/// The badge tests that measure the part of a badge past its corner, for the platforms that report
/// an element whole. Android reports only the part of an element inside its parent, so there the
/// badge's size, and a picture of it, cannot be had.
/// </summary>
public abstract class UnclippedBadgeTests : BadgeTests
{
    private protected UnclippedBadgeTests(SampleAppFixture app)
        : base(app)
    {
    }

    /// <remarks>
    /// <para>
    /// A quarter out leaves the middle of the badge a quarter of its size in from each edge. A few
    /// pixels either way are allowed for the rounding of a scaled screen.
    /// </para>
    /// <para>
    /// Measured on the dot, which is a bare label: the built-in badge is found by its text, and
    /// that is the label inside the pill, smaller than the badge by the pill's padding.
    /// </para>
    /// </remarks>
    [Fact]
    public void ABadge_HangsAQuarterOutPastItsCorner()
    {
        const int Tolerance = 2;

        Open();
        Find("PlacementTopEnd").Click();
        Expect("CurrentPlacement", "TopEnd");
        SetCount(BadgeText);

        var content = Find(DotBadgedButton);
        var badge = Find(DotBadge);

        var expectedX = content.Location.X + content.Size.Width - (badge.Size.Width / 4.0);
        var expectedY = content.Location.Y + (badge.Size.Height / 4.0);
        var badgeMiddleX = badge.Location.X + (badge.Size.Width / 2.0);
        var badgeMiddleY = badge.Location.Y + (badge.Size.Height / 2.0);

        Assert.True(
            Math.Abs(badgeMiddleX - expectedX) <= Tolerance,
            $"The middle of the badge is at x={badgeMiddleX}, and a quarter of it in from the right of what it marks is {expectedX}.");

        Assert.True(
            Math.Abs(badgeMiddleY - expectedY) <= Tolerance,
            $"The middle of the badge is at y={badgeMiddleY}, and a quarter of it down from the top of what it marks is {expectedY}.");
    }

    /// <remarks>
    /// <para>
    /// Where a badge is placed says nothing about whether it was drawn there: a driver reports an
    /// element's frame whole even where its parent cuts it off. So this looks at a picture of the
    /// page just past the corner of what the badge marks: two points beyond its right edge and two
    /// above its top, level with the middle of the badge. Drawn, both are the red of the badge's
    /// pill; cut off, both are whatever is behind it.
    /// </para>
    /// <para>
    /// Measured on the built-in badge, whose pill is filled. The dot is a glyph in a box larger than
    /// itself, and the part of that box past the corner holds little or none of it. The badge's
    /// middle is found from its text, which sits in the middle of the pill; the text itself is
    /// white and well inside the corner.
    /// </para>
    /// <para>
    /// The picture is of the page's scroll view rather than of the screen, so that what a point is
    /// worth in pixels, and where the picture starts, come from the same element.
    /// </para>
    /// </remarks>
    [Fact]
    public void ABadge_IsDrawnPastItsCorner()
    {
        const int Past = 2;

        Open();
        Find("PlacementTopEnd").Click();
        Expect("CurrentPlacement", "TopEnd");
        SetCount(BadgeText);

        var content = Find(BadgedButton);
        var badge = Ui.Driver.FindElement(Ui.ByText(BadgeText));
        var scroller = Find(PageScroll);
        var picture = Png.Read(scroller.GetScreenshot().AsByteArray);
        var scale = picture.Width / (double)scroller.Size.Width;

        var badgeMiddleX = badge.Location.X + (badge.Size.Width / 2.0);
        var badgeMiddleY = badge.Location.Y + (badge.Size.Height / 2.0);

        ExpectRed("past its right edge", content.Location.X + content.Size.Width + Past, badgeMiddleY);
        ExpectRed("above its top edge", badgeMiddleX, content.Location.Y - Past);

        void ExpectRed(string where, double x, double y)
        {
            var colour = picture.ColourAt(
                (int)((x - scroller.Location.X) * scale),
                (int)((y - scroller.Location.Y) * scale));

            Assert.True(
                colour.Red - colour.Green > 80 && colour.Red - colour.Blue > 80,
                $"Just {where}, at ({x}, {y}), the page is {colour}, not the red of the badge.");
        }
    }

    /// <remarks>
    /// <para>
    /// The text says "99+" either way, so what is compared is what was drawn: where the white of the
    /// text has most of its ink, across a picture of the label found by that text -- the label
    /// inside the pill, as above. "99+" is heavier on the left, where the nines are; laid out right
    /// to left it would read "+99" and be heavier on the right.
    /// </para>
    /// </remarks>
    [Fact]
    public void InARightToLeftPage_TheTextStillReadsLeftToRight()
    {
        const double Tolerance = 0.08;

        Open();
        Find("PlacementTopEnd").Click();
        Expect("CurrentPlacement", "TopEnd");
        SetCount("100");

        try
        {
            var leftToRight = WhereTheInkIs();
            SetFlowDirection("RightToLeft");
            var rightToLeft = WhereTheInkIs();

            Assert.True(leftToRight < 0.5, $"Left to right, the ink of 99+ is centred at {leftToRight:F2} across the badge, not on its left.");
            Assert.True(
                Math.Abs(rightToLeft - leftToRight) <= Tolerance,
                $"The ink of the badge is centred at {rightToLeft:F2} across it right to left, and was at {leftToRight:F2} left to right.");
        }
        finally
        {
            SetFlowDirection("LeftToRight");
        }

        double WhereTheInkIs() =>
            Png.Read(Ui.Driver.FindElement(Ui.ByText(Overflowed)).GetScreenshot().AsByteArray).LightCentre();
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsBadgeTests(WindowsSampleApp app) : UnclippedBadgeTests(app);

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidBadgeTests(AndroidSampleApp app) : BadgeTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosBadgeTests(IosSampleApp app) : UnclippedBadgeTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystBadgeTests(MacCatalystSampleApp app) : UnclippedBadgeTests(app);
