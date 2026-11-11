namespace SumireCraft.Maui.UITests;

/// <summary>
/// What the sample application should show of a field's title and error on a real screen.
/// </summary>
public abstract class FieldTests : SamplePageTests
{
    private const string PageTitle = "SmrField";
    private const string NameEntry = "NameEntry";
    private const string NoteEntry = "NoteEntry";
    private const string CityEntry = "CityEntry";
    private const string DistrictEntry = "DistrictEntry";
    private const string NameTitle = "Name";
    private const string NameError = "Enter a name.";
    private const string NoteError = "Keep it short.";
    private const string BelowReserved = "BelowReserved";
    private const string BelowClosed = "BelowClosed";

    private protected FieldTests(SampleAppFixture app)
        : base(app)
    {
    }

    /// <remarks>
    /// The unit tests show that the error label keeps a blank line; only a screen shows that a blank
    /// line is as high as a line of text, so that nothing below moves. Measured from the field's own
    /// entry, so that a field above it growing does not count.
    /// </remarks>
    [Fact]
    public void AnError_LeavesWhatIsBelowWhereItWas()
    {
        Open();
        SetErrors(shown: false);
        var before = Below(NameEntry, BelowReserved);

        SetErrors(shown: true);

        Assert.True(IsShowing(Ui.ByText(NameError)), "The error was not shown.");
        var after = Settled(() => Below(NameEntry, BelowReserved));
        Assert.True(Math.Abs(after - before) <= 1, $"What lies below the field moved from {before} to {after} below its entry.");
    }

    [Fact]
    public void AnErrorWithoutReservedSpace_PushesWhatIsBelowDown()
    {
        Open();
        SetErrors(shown: false);
        var before = Below(NoteEntry, BelowClosed);

        SetErrors(shown: true);

        Assert.True(IsShowing(Ui.ByText(NoteError)), "The error was not shown.");
        var after = Settled(() => Below(NoteEntry, BelowClosed));
        Assert.True(after > before, $"What lies below the field stayed {before} below its entry.");
    }

    /// <remarks>
    /// Two fields side by side, one titled and one keeping a line for a title it has not got. Only a
    /// screen shows that the blank line is as high as the title, so that the entries line up.
    /// </remarks>
    [Fact]
    public void AFieldKeepingALineForItsTitle_LinesUpWithATitledOne()
    {
        Open();

        var titled = Find(CityEntry).Location.Y;
        var untitled = Find(DistrictEntry).Location.Y;

        Assert.True(Math.Abs(titled - untitled) <= 1, $"The titled field's entry is at y={titled} and the other's at y={untitled}.");
    }

    /// <remarks>
    /// The sample's App.xaml indents every field's title through the <c>SmrFieldTitle</c> class.
    /// Without that style the title starts where the entry does; with it, a little after.
    /// </remarks>
    [Fact]
    public void AStyleInTheApplication_ReachesTheTitle()
    {
        Open();

        var entry = Find(NameEntry);
        var title = Ui.Driver.FindElement(Ui.ByText(NameTitle));

        Assert.True(
            title.Location.X > entry.Location.X,
            $"The title starts at x={title.Location.X} and the entry at x={entry.Location.X}: the application's style did not indent it.");
    }

    // How far below a field's entry the marker under that field sits.
    private int Below(string entry, string marker) => Find(marker).Location.Y - Find(entry).Location.Y;

    // A measurement once the layout has stopped moving: the same twice running, a moment apart, or
    // whatever it is after two seconds.
    private static int Settled(Func<int> measure)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
        var last = measure();

        while (DateTime.UtcNow < deadline)
        {
            Thread.Sleep(200);
            var next = measure();
            if (next == last)
            {
                return next;
            }

            last = next;
        }

        return last;
    }

    private void Open()
    {
        if (IsShowing(Ui.ByAutomationId(NameEntry)))
        {
            return;
        }

        Ui.OpenMenu();
        Ui.Driver.FindElement(Ui.ByText(PageTitle)).Click();

        Find(NameEntry);
    }

    private void SetErrors(bool shown)
    {
        var wanted = shown ? "Errors shown" : "No errors";
        if (Text("ErrorsState") != wanted)
        {
            Find("ToggleErrors").Click();
        }

        Expect("ErrorsState", wanted);
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsFieldTests(WindowsSampleApp app) : FieldTests(app);

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidFieldTests(AndroidSampleApp app) : FieldTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosFieldTests(IosSampleApp app) : FieldTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystFieldTests(MacCatalystSampleApp app) : FieldTests(app);
