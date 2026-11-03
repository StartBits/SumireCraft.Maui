using OpenQA.Selenium;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// What the sample application should show when something fails.
/// </summary>
/// <remarks>
/// The sample tells the user only about failures they were waiting for. Everything is reported, and
/// the log gets all of it, but only a command the user started earns an alert.
/// </remarks>
public abstract class ExceptionTests : SamplePageTests
{
    private const string PageTitle = "Exceptions";
    private const string FailButton = "FailButton";
    private const string AlertTitle = "Something went wrong";
    private const string AlertButton = "Got it";
    private const string Nothing = "nothing yet";
    private const string Reported = "reported";

    private protected ExceptionTests(SampleAppFixture app)
        : base(app)
    {
    }

    [Fact]
    public void AFailingCommand_ShowsAnAlert()
    {
        Open();

        Find(FailButton).Click();

        DismissAlert();
        Expect("LastOutcome", Reported);
    }

    /// <summary>
    /// A timeout surfaces as an <see cref="OperationCanceledException"/> too, but nobody asked for
    /// it, so it is a failure like any other.
    /// </summary>
    [Fact]
    public void ACommandThatTimesOut_ShowsAnAlert()
    {
        Open();

        Find("TimeOutButton").Click();

        DismissAlert();
        Expect("LastOutcome", Reported);
    }

    [Fact]
    public void ACommandTheUserCancelled_ShowsNothing()
    {
        Open();

        Find("LoadButton").Click();
        Expect("LastOutcome", "running");

        Find("CancelButton").Click();
        Expect("LastOutcome", "cancelled");

        ExpectNoAlert();
    }

    [Fact]
    public void AFailureInTheBackground_ShowsNothing()
    {
        Open();

        Find("FailInBackgroundButton").Click();
        Expect("LastOutcome", "reported from the background");

        ExpectNoAlert();
    }

    /// <summary>
    /// Opens the page these tests are about and puts its outcome back to how it starts.
    /// </summary>
    /// <remarks>
    /// An alert a failed test left behind is put away first. It would otherwise sit in front of the
    /// page and fail every test after it for a reason that has nothing to do with them.
    /// </remarks>
    private void Open()
    {
        if (IsShowing(Ui.ByText(AlertTitle)))
        {
            Ui.Driver.FindElement(Ui.ByAlertButton(AlertButton)).Click();
        }

        if (!IsShowing(Ui.ByAutomationId(FailButton)))
        {
            Ui.OpenMenu();
            Ui.Driver.FindElement(Ui.ByText(PageTitle)).Click();

            // The implicit wait covers the transition.
            Find(FailButton);
        }

        Find("ResetButton").Click();
        Expect("LastOutcome", Nothing);
    }

    /// <summary>
    /// Waits for the alert, then puts it away so the next test starts with nothing in front of the
    /// page.
    /// </summary>
    /// <remarks>
    /// Before reading anything on the page: on Android an open dialog keeps the page out of the
    /// tree the driver searches.
    /// </remarks>
    private void DismissAlert()
    {
        Assert.True(WaitFor(Ui.ByText(AlertTitle)), "No alert came up.");

        Ui.Driver.FindElement(Ui.ByAlertButton(AlertButton)).Click();

        Assert.False(IsShowing(Ui.ByText(AlertTitle)), "The alert was still up after it was dismissed.");
    }

    /// <summary>
    /// Asks for something until it turns up or five seconds have passed.
    /// </summary>
    /// <remarks>
    /// Polled rather than left to the implicit wait, which the Windows driver does not apply to a
    /// search by text: it answers at once that a dialog still on its way does not exist.
    /// </remarks>
    private bool WaitFor(By locator)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);

        do
        {
            if (IsShowing(locator))
            {
                return true;
            }

            Thread.Sleep(100);
        }
        while (DateTime.UtcNow < deadline);

        return false;
    }

    /// <summary>
    /// Gives an alert time to come up before saying there is none.
    /// </summary>
    /// <remarks>
    /// The outcome on the page is written straight after the report, but an alert for a report
    /// made off the UI thread is put up later, once the UI thread gets to it. Asking at once would
    /// pass whether or not one was coming.
    /// </remarks>
    private void ExpectNoAlert()
    {
        Thread.Sleep(TimeSpan.FromSeconds(1));

        Assert.False(IsShowing(Ui.ByText(AlertTitle)), "An alert came up for a failure nobody was waiting for.");
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsExceptionTests(WindowsSampleApp app) : ExceptionTests(app);

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidExceptionTests(AndroidSampleApp app) : ExceptionTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosExceptionTests(IosSampleApp app) : ExceptionTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystExceptionTests(MacCatalystSampleApp app) : ExceptionTests(app);
