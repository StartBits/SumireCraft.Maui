using OpenQA.Selenium.Appium;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// What the sample application should show of an underlined input on a real screen.
/// </summary>
/// <remarks>
/// The unit tests show what line each input is handed; only a screen shows that the platform draws
/// it. Each test reads the pixels of a picture of the input itself, along a column through its
/// middle, from the bottom up: the line is a band of its colour at the bottom of the input.
/// </remarks>
public abstract class UnderlineTests : SamplePageTests
{
    private const string PageTitle = "SmrUnderline";
    private const string PageScroll = "UnderlinePageScroll";
    private const string ColouredEntry = "ColouredEntry";
    private const string ToggledEntry = "ToggledEntry";

    private protected UnderlineTests(SampleAppFixture app)
        : base(app)
    {
    }

    /// <remarks>
    /// The entry's line is purple and its text teal, so a band of purple is the line and nothing else.
    /// </remarks>
    [Fact]
    public void AnUnderlinedEntry_StandsOnALineOfItsColour()
    {
        Open();

        Assert.True(Band(Find(ColouredEntry), IsPurple) > 0, "The entry has no purple line at its bottom.");
    }

    /// <remarks>
    /// Each platform draws these on a control of their own, with keys and states of their own: each
    /// is asked for its line, in a colour none of the platforms' boxes are.
    /// </remarks>
    [Theory]
    [InlineData("UnderlinedEditor", 255, 140, 0)]
    [InlineData("UnderlinedPicker", 0, 128, 0)]
    public void AnUnderlinedInput_StandsOnALineOfItsColour(string input, int red, int green, int blue)
    {
        Open();

        Assert.True(BandOnScreen(input, Near(red, green, blue)) > 0, $"{input} has no line of ({red}, {green}, {blue}) at its bottom.");
    }

    [Fact]
    public void TheLine_ThickensWhileTheEntryHasTheFocus()
    {
        Open();
        var entry = Find(ColouredEntry);
        var before = Band(entry, IsPurple);

        entry.Click();

        var after = Settled(() => Band(Find(ColouredEntry), IsPurple), changedFrom: before);
        Assert.True(after > before, $"The line was {before} pixels thick before the entry had the focus, and {after} with it.");
    }

    /// <remarks>
    /// The entry's line is red, which none of the platforms' own boxes are: no red at its bottom is
    /// the box back.
    /// </remarks>
    [Fact]
    public void AnUnderlineTurnedOff_GivesTheBoxBackAndTurnedOnAgain_TheLine()
    {
        Open();
        SetUnderlined(true);
        Assert.True(Band(Find(ToggledEntry), IsRed) > 0, "The entry has no red line before the underline is turned off.");

        try
        {
            var underlined = Band(Find(ToggledEntry), IsRed);
            SetUnderlined(false);
            Assert.Equal(0, Settled(() => Band(Find(ToggledEntry), IsRed), changedFrom: underlined));

            SetUnderlined(true);
            Assert.True(Settled(() => Band(Find(ToggledEntry), IsRed), changedFrom: 0) > 0, "The red line did not come back.");
        }
        finally
        {
            SetUnderlined(true);
        }
    }

    /// <remarks>
    /// On Android, .NET MAUI draws an input's background itself once it is given one, over its own
    /// line; the underline has to be drawn again after that.
    /// </remarks>
    [Fact]
    public void ABackgroundGivenLater_KeepsTheLine()
    {
        Open();
        SetUnderlined(true);

        try
        {
            SetBackground(true);

            Assert.True(Settled(() => Band(Find(ToggledEntry), IsRed), changedFrom: 0) > 0, "The red line went with the background.");
        }
        finally
        {
            SetBackground(false);
        }
    }

    private static bool IsPurple((byte Red, byte Green, byte Blue) c) => c.Red - c.Green > 60 && c.Blue - c.Green > 60;

    private static bool IsRed((byte Red, byte Green, byte Blue) c) => c.Red - c.Green > 80 && c.Red - c.Blue > 80;

    // A colour close to the one given: the line's colour drawn, blended a little at its edges.
    private protected static Func<(byte, byte, byte), bool> Near(int red, int green, int blue) =>
        c => Math.Abs(c.Item1 - red) < 70 && Math.Abs(c.Item2 - green) < 70 && Math.Abs(c.Item3 - blue) < 70;

    // How many rows of the line's colour make the lowest band of it in the bottom third of the
    // element, down a column through its middle. Some platforms give a control more room below its
    // box than the box takes -- a WinUI date picker does -- so the line need not be the last row.
    private protected static int Band(AppiumElement element, Func<(byte, byte, byte), bool> isLine)
    {
        var picture = Png.Read(element.GetScreenshot().AsByteArray);
        var x = picture.Width / 2;
        var y = picture.Height - 1;
        var top = picture.Height * 2 / 3;

        while (y > top && !isLine(picture.ColourAt(x, y)))
        {
            y--;
        }

        var rows = 0;
        while (y >= 0 && isLine(picture.ColourAt(x, y)))
        {
            rows++;
            y--;
        }

        return rows;
    }

    // The band of an input that may be below the fold, as a window's short height leaves it on
    // Windows: a picture of an element off the screen cannot be taken there. Scrolled back after.
    private protected int BandOnScreen(string input, Func<(byte, byte, byte), bool> isLine)
    {
        var scroller = Ui.ByAutomationId(PageScroll);
        try
        {
            return Band(Ui.BringIntoView(Ui.ByAutomationId(input), scroller), isLine);
        }
        finally
        {
            Ui.ScrollToTop(scroller);
        }
    }

    // Measures until it has changed from what it was, or two seconds have passed: a platform draws a
    // change on its next frame, not at once.
    private static int Settled(Func<int> measure, int changedFrom)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
        var value = measure();

        while (value == changedFrom && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(100);
            value = measure();
        }

        return value;
    }

    private protected void Open()
    {
        if (IsShowing(Ui.ByAutomationId(ColouredEntry)))
        {
            return;
        }

        Ui.OpenMenu();
        Ui.Driver.FindElement(Ui.ByText(PageTitle)).Click();

        Find(ColouredEntry);
    }

    private void SetUnderlined(bool on) => Set("UnderlineSwitch", "UnderlineState", on ? "Underlined" : "Not underlined");

    private void SetBackground(bool on) => Set("BackgroundSwitch", "BackgroundState", on ? "On a background" : "On the page");

    private void Set(string toggle, string state, string wanted)
    {
        if (Text(state) != wanted)
        {
            Find(toggle).Click();
        }

        Expect(state, wanted);
    }
}

/// <summary>
/// The date and time pickers' lines, where they are drawn: not on Mac Catalyst, where .NET MAUI
/// shows the system's date picker, a pill the library leaves as it is.
/// </summary>
public abstract class UnderlinedPickerTests : UnderlineTests
{
    private protected UnderlinedPickerTests(SampleAppFixture app)
        : base(app)
    {
    }

    [Theory]
    [InlineData("UnderlinedDatePicker", 0, 0, 255)]
    [InlineData("UnderlinedTimePicker", 0, 128, 128)]
    public void AnUnderlinedPicker_StandsOnALineOfItsColour(string picker, int red, int green, int blue)
    {
        Open();

        Assert.True(BandOnScreen(picker, Near(red, green, blue)) > 0, $"{picker} has no line of ({red}, {green}, {blue}) at its bottom.");
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsUnderlineTests(WindowsSampleApp app) : UnderlinedPickerTests(app);

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidUnderlineTests(AndroidSampleApp app) : UnderlinedPickerTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosUnderlineTests(IosSampleApp app) : UnderlinedPickerTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystUnderlineTests(MacCatalystSampleApp app) : UnderlineTests(app);
