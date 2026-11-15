namespace SumireCraft.Maui.UITests;

/// <summary>
/// What a platform's own pickers do to an input they are shown, which no unit test can see: the
/// native control behind a <c>DatePicker</c> or a <c>TimePicker</c> may write back a value of its
/// own.
/// </summary>
/// <remarks>
/// Nothing here picks a date or a time through a platform's dialog. Each platform's is different,
/// and what is picked reaches the input by the same path as these values come back by.
/// </remarks>
public abstract class DateTimeInputTests : SamplePageTests
{
    private const string PageTitle = "DateTimeInput";
    private const string Read = "ReadInput";
    private const string State = "InputState";
    private const string Fill = "FillInput";
    private const string Early = "EarlyInput";
    private const string Empty = "EmptyInput";
    private const string Null = "NullInput";

    private protected DateTimeInputTests(SampleAppFixture app)
        : base(app)
    {
    }

    /// <summary>
    /// What an empty time reads as once a picker has shown it: still empty, where the platform's
    /// time picker can show no time at all.
    /// </summary>
    protected virtual string EmptyTime => "none";

    [Fact]
    public void AnEmptyInput_IsShownWithoutItsDateBeingFilled()
    {
        Open();

        Find(Empty).Click();

        ExpectInput($"Date: none, time: {EmptyTime}");
    }

    [Fact]
    public void AnInputShown_ComesBackAsItWas()
    {
        Open();

        Find(Fill).Click();

        ExpectInput("Date: 2026-11-13, time: 10:30");
    }

    /// <remarks>
    /// .NET MAUI's date picker shows nothing before 1900, and the input takes the date shown. The
    /// time is another picker's and stays.
    /// </remarks>
    [Fact]
    public void ADateBeforeThePickersRange_IsMovedToItsStart()
    {
        Open();

        Find(Early).Click();

        ExpectInput("Date: 1900-01-01, time: 09:00");
    }

    [Fact]
    public void AViewModelWithoutAnInput_IsGivenAnEmptyOne()
    {
        Open();

        Find(Null).Click();

        ExpectInput($"Date: none, time: {EmptyTime}");
    }

    // The input says nothing when it changes, and a platform may write to it a moment after it is
    // shown, so the page is asked again until it says what it should or time runs out.
    private void ExpectInput(string expected)
    {
        var deadline = DateTime.UtcNow + LabelPatience;
        string actual;

        do
        {
            Find(Read).Click();
            actual = Text(State);

            if (actual == expected)
            {
                break;
            }

            Thread.Sleep(200);
        }
        while (DateTime.UtcNow < deadline);

        Assert.Equal(expected, actual);

        // Still so a moment later: a write that arrives late would otherwise be missed by a test
        // that expects nothing to change.
        Thread.Sleep(500);
        Find(Read).Click();
        Assert.Equal(expected, Text(State));
    }

    private void Open()
    {
        if (IsShowing(Ui.ByAutomationId(Read)))
        {
            return;
        }

        Ui.OpenMenu();
        Ui.Driver.FindElement(Ui.ByText(PageTitle)).Click();

        Find(Read);
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsDateTimeInputTests(WindowsSampleApp app) : DateTimeInputTests(app)
{
    /// <remarks>
    /// .NET MAUI 10.0.20, which the sample is built with, gives WinUI's time picker midnight for an
    /// empty time, and writes that back to its <c>TimePicker</c>. Its later source gives WinUI the
    /// empty time instead; when the sample's .NET MAUI has that, this override fails and goes. The
    /// date picker has an empty state of its own and leaves an empty date alone.
    /// </remarks>
    protected override string EmptyTime => "00:00";
}

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidDateTimeInputTests(AndroidSampleApp app) : DateTimeInputTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosDateTimeInputTests(IosSampleApp app) : DateTimeInputTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystDateTimeInputTests(MacCatalystSampleApp app) : DateTimeInputTests(app);
