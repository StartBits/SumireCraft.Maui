using OpenQA.Selenium.Appium;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// What the sample application should show of the library's inputs drawn with a border of their
/// own, on a real screen.
/// </summary>
/// <remarks>
/// The unit tests show what border each input is handed; only a screen shows that the platform draws
/// it. Each test reads the pixels of a picture of the input itself, along a column through its
/// middle: a line under the text is a band of its colour at the bottom of the input.
/// </remarks>
public abstract class InputBorderTests : SamplePageTests
{
    private const string PageTitle = "Inputs";
    private const string PageScroll = "InputsPageScroll";
    private const string ColouredEntry = "ColouredEntry";
    private const string AbcEntry = "AbcEntry";
    private const string ToggledEntry = "ToggledEntry";
    private const string BoxedEntry = "BoxedEntry";
    private const string RightToLeftBoxedEntry = "RightToLeftBoxedEntry";
    private const string PlainEntry = "PlainEntry";
    private const string Editor = "UnderlinedEditor";

    // The least room, in points, kept between a border down a side and the text: less than any
    // platform's own box keeps, more than a text run onto the border would leave. Android keeps the
    // least, 3.8 points to the edge of the text's room, which a letter's own edge reads as 2.7.
    private const double LeastGap = 2;

    private protected InputBorderTests(SampleAppFixture app)
        : base(app)
    {
    }

    /// <remarks>
    /// The entry's line is purple and its text teal, so a band of purple is the line and nothing else.
    /// </remarks>
    [Fact]
    public void AnEntryWithALineUnderItsText_StandsOnALineOfItsColour()
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
    public void AnInputWithALineUnderItsText_StandsOnALineOfItsColour(string input, int red, int green, int blue)
    {
        Open();

        Assert.True(BandOnScreen(input, Line(red, green, blue)) > 0, $"{input} has no line of ({red}, {green}, {blue}) at its bottom.");
    }

    /// <remarks>
    /// An editor holding more lines than it shows scrolls to its last; on iOS and Mac Catalyst a text
    /// view scrolls its layers with its text, the border's included, unless the border is put back.
    /// </remarks>
    [Fact]
    public void AnEditorScrolledThroughItsText_KeepsItsLineAtTheBottom()
    {
        Open();
        var scroller = Ui.ByAutomationId(PageScroll);
        var orange = Line(255, 140, 0);

        try
        {
            var editor = Ui.BringIntoView(Ui.ByAutomationId(Editor), scroller);
            editor.SendKeys(string.Join("\n", Enumerable.Range(1, 12)));
            Ui.DismissKeyboard();

            Assert.True(Band(Find(Editor), orange) > 0, "The editor's line went with its text as it scrolled.");
        }
        finally
        {
            Find(Editor).Clear();
            Ui.DismissKeyboard();
            Ui.ScrollToTop(scroller);
        }
    }

    /// <remarks>
    /// AbcEntry's style in the sample gives the focus a visual state of twice the thickness.
    /// </remarks>
    [Fact]
    public void AVisualStateForTheFocus_ThickensTheLine()
    {
        Open();
        var entry = Find(AbcEntry);
        var before = Band(entry, IsViolet);

        try
        {
            entry.Click();

            var after = Settled(() => Band(Find(AbcEntry), IsViolet), changedFrom: before);
            Assert.True(after > before, $"The line was {before} pixels thick before the entry had the focus, and {after} with it.");
        }
        finally
        {
            // The keyboard the focus brought up covers the bottom of the page, where Android leaves
            // what it covers out of the tree.
            Ui.DismissKeyboard();
        }
    }

    /// <remarks>
    /// The entry's line is red, which none of the platforms' own boxes are: no red at its bottom is
    /// the box back.
    /// </remarks>
    [Fact]
    public void ABorderTakenAway_GivesTheBoxBackAndGivenAgain_TheLine()
    {
        Open();
        SetBordered(true);
        Assert.True(Band(Find(ToggledEntry), IsRed) > 0, "The entry has no red line before its border is taken away.");

        try
        {
            var bordered = Band(Find(ToggledEntry), IsRed);
            SetBordered(false);
            Assert.Equal(0, Settled(() => Band(Find(ToggledEntry), IsRed), changedFrom: bordered));

            SetBordered(true);
            Assert.True(Settled(() => Band(Find(ToggledEntry), IsRed), changedFrom: 0) > 0, "The red line did not come back.");
        }
        finally
        {
            SetBordered(true);
        }
    }

    /// <remarks>
    /// On Android, .NET MAUI draws an input's background itself once it is given one, over its own
    /// line; the border has to be drawn again after that.
    /// </remarks>
    [Fact]
    public void ABackgroundGivenLater_KeepsTheLine()
    {
        Open();
        SetBordered(true);

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

    /// <remarks>
    /// A border down the sides moves the text in, by its thickness on Android and by the room iOS
    /// keeps in place of its box. Taken away, the text goes back to where the platform keeps it: where
    /// it is in the entry given no border, the same width on the page. Both placeholders start with a
    /// round letter, read the same to a pixel.
    /// </remarks>
    [Fact]
    public void ABorderTakenAway_PutsTheTextBackWhereThePlatformKeepsIt()
    {
        Open();
        var scroller = Ui.ByAutomationId(PageScroll);

        try
        {
            SetBordered(false);
            var toggled = TextStart(Png.Read(Find(ToggledEntry).GetScreenshot().AsByteArray));
            var plain = TextStart(Png.Read(Ui.BringIntoView(Ui.ByAutomationId(PlainEntry), scroller).GetScreenshot().AsByteArray));

            Assert.True(Math.Abs(toggled - plain) <= 2, $"With its border taken away, the entry's text starts {toggled} pixels in, and the plain entry's {plain}.");
        }
        finally
        {
            Ui.ScrollToTop(scroller);
            SetBordered(true);
        }
    }

    // How many pixels in from the left the text starts, across the middle of an input: past the edge
    // of the platform's box, if it draws one there, and the light room inside it.
    private static int TextStart(Png picture)
    {
        static bool IsLight((byte Red, byte Green, byte Blue) c) => c.Red > 230 && c.Green > 230 && c.Blue > 230;

        var start = int.MaxValue;
        for (var y = picture.Height / 3; y < picture.Height * 2 / 3; y++)
        {
            var x = 0;
            while (x < picture.Width && !IsLight(picture.ColourAt(x, y)))
            {
                x++;
            }

            while (x < picture.Width && IsLight(picture.ColourAt(x, y)))
            {
                x++;
            }

            start = Math.Min(start, x);
        }

        return start;
    }

    /// <remarks>
    /// The entry's border is magenta all round, with corners rounded by more than its thickness: its
    /// top has the border too, and its very corner is outside it.
    /// </remarks>
    [Fact]
    public void ABorderAllRoundWithRoundedCorners_IsDrawnAtTheTopAndNotInTheCorner()
    {
        Open();
        var scroller = Ui.ByAutomationId(PageScroll);

        try
        {
            var picture = Png.Read(Ui.BringIntoView(Ui.ByAutomationId(BoxedEntry), scroller).GetScreenshot().AsByteArray);

            Assert.True(TopBand(picture, IsMagenta) > 0, "The entry has no magenta border at its top.");
            Assert.False(IsMagenta(picture.ColourAt(0, 0)), "The entry's top left corner is not rounded.");
        }
        finally
        {
            Ui.ScrollToTop(scroller);
        }
    }

    /// <remarks>
    /// The box iOS takes away from a text field is what held its text off the edge. Across the middle
    /// of each boxed entry, the entry's background lies between the text and the border down the side
    /// the text starts from, for at least <see cref="LeastGap"/> points: from the left of the boxed
    /// entry, and from the right of one laid out right to left, where its border is thicker. A
    /// picture's pixels to the point are read off the top border, two points thick in both.
    /// </remarks>
    [Theory]
    [InlineData(BoxedEntry, false)]
    [InlineData(RightToLeftBoxedEntry, true)]
    public void ABorderDownTheSideTheTextStartsFrom_KeepsTheTextOffIt(string entry, bool fromTheRight)
    {
        Open();
        var scroller = Ui.ByAutomationId(PageScroll);

        try
        {
            var picture = Png.Read(Ui.BringIntoView(Ui.ByAutomationId(entry), scroller).GetScreenshot().AsByteArray);
            var pixelsToThePoint = TopBand(picture, IsMagenta) / 2.0;
            Assert.True(pixelsToThePoint > 0, $"{entry} has no magenta border at its top.");
            var gap = double.MaxValue;

            for (var y = picture.Height / 3; y < picture.Height * 2 / 3; y++)
            {
                var (inside, width) = BorderFrom(picture, y, fromTheRight);
                Assert.True(width > 0, $"{entry} has no magenta border down the side its text starts from, {y} pixels down.");

                // The pixel next to the border is blended with it, and is counted without being read.
                var step = fromTheRight ? -1 : 1;
                var background = 1;
                for (var x = inside + step; x > 0 && x < picture.Width - 1 && IsBoxBackground(picture.ColourAt(x, y)); x += step)
                {
                    background++;
                }

                gap = Math.Min(gap, background / pixelsToThePoint);
            }

            Assert.True(gap >= LeastGap, $"{entry}'s text comes within {gap:F1} points of the border it starts from.");
        }
        finally
        {
            Ui.ScrollToTop(scroller);
        }
    }

    /// <remarks>
    /// As .NET MAUI lays out a Margin or a Padding, the thickness given for the left is the side the
    /// text starts from, and the right the side it ends at: laid out right to left, the entry given
    /// eight points at the left and two elsewhere is eight points thick at its right and two at its
    /// left. The two are compared, not measured: Windows scales a point to a fraction of pixels, and
    /// blends the edges of a thin border more than a thick one's, so the right is asked to be well over
    /// twice the left rather than four times.
    /// </remarks>
    [Fact]
    public void ABorderLaidOutRightToLeft_HasItsLeftThicknessAtTheRight()
    {
        Open();
        var scroller = Ui.ByAutomationId(PageScroll);

        try
        {
            var picture = Png.Read(Ui.BringIntoView(Ui.ByAutomationId(RightToLeftBoxedEntry), scroller).GetScreenshot().AsByteArray);
            var y = picture.Height / 2;

            var right = BorderFrom(picture, y, fromTheRight: true).Width;
            var left = BorderFrom(picture, y, fromTheRight: false).Width;

            Assert.True(left > 0 && right >= 2.5 * left, $"{RightToLeftBoxedEntry}'s border is {left} pixels thick at its left and {right} at its right.");
        }
        finally
        {
            Ui.ScrollToTop(scroller);
        }
    }

    // Where the magenta border met first from one end of a row ends, and how many pixels wide it is.
    private static (int Inside, int Width) BorderFrom(Png picture, int y, bool fromTheRight)
    {
        var step = fromTheRight ? -1 : 1;
        var x = fromTheRight ? picture.Width - 1 : 0;

        for (var moved = 0; moved < picture.Width / 3 && !IsMagenta(picture.ColourAt(x, y)); moved++)
        {
            x += step;
        }

        var width = 0;
        while (x >= 0 && x < picture.Width && IsMagenta(picture.ColourAt(x, y)))
        {
            width++;
            x += step;
        }

        return (x, width);
    }

    // The boxed entry's own background, #F3EDF7.
    private static bool IsBoxBackground((byte, byte, byte) c) => Near(243, 237, 247, 16)(c);

    // Mac Catalyst scales an iPad's points to 77% of the Mac's, so a line one point thick falls across
    // pixels and is blended with the white around it: purple there reads as (158, 108, 158).
    private static bool IsPurple((byte Red, byte Green, byte Blue) c) => c.Red - c.Green > 40 && c.Blue - c.Green > 40;

    private static bool IsRed((byte Red, byte Green, byte Blue) c) => c.Red - c.Green > 80 && c.Red - c.Blue > 80;

    private static bool IsMagenta((byte Red, byte Green, byte Blue) c) => c.Red > 180 && c.Blue > 180 && c.Green < 100;

    // The sample's violet, #8467E0.
    private static bool IsViolet((byte, byte, byte) c) => Near(132, 103, 224)(c);

    // A colour close to the one given: the line's colour drawn, blended a little at its edges.
    private protected static Func<(byte, byte, byte), bool> Near(int red, int green, int blue, int within = 70) =>
        c => Math.Abs(c.Item1 - red) < within && Math.Abs(c.Item2 - green) < within && Math.Abs(c.Item3 - blue) < within;

    // A line of the colour given, on a white page, however much of it a pixel holds. Mac Catalyst
    // scales an iPad's points to 77% of the Mac's, so a line one point thick falls across two rows of
    // pixels, each the colour blended with white: green there reads as (93, 151, 93) and
    // (123, 166, 123). What each pixel takes away from white points the same way as what the colour
    // does, and is at least a third as much; black text and a grey placeholder point elsewhere.
    private protected static Func<(byte, byte, byte), bool> Line(int red, int green, int blue) => c =>
    {
        double[] colour = [255 - red, 255 - green, 255 - blue];
        double[] pixel = [255 - c.Item1, 255 - c.Item2, 255 - c.Item3];
        var dot = (colour[0] * pixel[0]) + (colour[1] * pixel[1]) + (colour[2] * pixel[2]);
        var colourLength = Math.Sqrt((colour[0] * colour[0]) + (colour[1] * colour[1]) + (colour[2] * colour[2]));
        var pixelLength = Math.Sqrt((pixel[0] * pixel[0]) + (pixel[1] * pixel[1]) + (pixel[2] * pixel[2]));

        return pixelLength >= colourLength / 3 && dot / (colourLength * pixelLength) >= 0.98;
    };

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

    // The same from the top down, in the top third.
    private static int TopBand(Png picture, Func<(byte, byte, byte), bool> isLine)
    {
        var x = picture.Width / 2;
        var y = 0;
        var bottom = picture.Height / 3;

        while (y < bottom && !isLine(picture.ColourAt(x, y)))
        {
            y++;
        }

        var rows = 0;
        while (y < picture.Height && isLine(picture.ColourAt(x, y)))
        {
            rows++;
            y++;
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

    private void SetBordered(bool on) => Set("BorderSwitch", "BorderState", on ? "Bordered" : "Not bordered");

    private void SetBackground(bool on) => Set("BackgroundSwitch", "BackgroundState", on ? "On a background" : "On the page");

    private void Set(string toggle, string state, string wanted)
    {
        if (Text(state) != wanted)
        {
            Find(toggle).Click();
            Expect(state, wanted);

            // The label changes at once, but the switch slides for a moment longer, and on iOS a tap
            // sent while it slides was seen to go nowhere, now and then: the next press found it as
            // it was.
            Thread.Sleep(500);
        }

        Expect(state, wanted);
    }
}

/// <summary>
/// The date and time pickers' lines, where they are drawn: not on Mac Catalyst, where .NET MAUI
/// shows the system's date picker, a pill the library leaves as it is.
/// </summary>
public abstract class PickerBorderTests : InputBorderTests
{
    private protected PickerBorderTests(SampleAppFixture app)
        : base(app)
    {
    }

    [Theory]
    [InlineData("UnderlinedDatePicker", 0, 0, 255)]
    [InlineData("UnderlinedTimePicker", 0, 128, 128)]
    public void APickerWithALineUnderItsText_StandsOnALineOfItsColour(string picker, int red, int green, int blue)
    {
        Open();

        Assert.True(BandOnScreen(picker, Line(red, green, blue)) > 0, $"{picker} has no line of ({red}, {green}, {blue}) at its bottom.");
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsInputBorderTests(WindowsSampleApp app) : PickerBorderTests(app);

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidInputBorderTests(AndroidSampleApp app) : PickerBorderTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosInputBorderTests(IosSampleApp app) : PickerBorderTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystInputBorderTests(MacCatalystSampleApp app) : InputBorderTests(app);
