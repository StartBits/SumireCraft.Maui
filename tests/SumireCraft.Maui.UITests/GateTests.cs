namespace SumireCraft.Maui.UITests;

/// <summary>
/// The busy gate as an application uses it, through the sample's BusyGate page: what a second tap
/// does while work is in flight, what the application's one gate holds back, and what the watchdog
/// does to a gate left busy.
/// </summary>
/// <remarks>
/// The unit tests pin the gate down; these show the sample using it as the README says to, so that
/// the page an application author copies from does what the README promises.
/// </remarks>
public abstract class GateTests : SamplePageTests
{
    private const string PageTitle = "BusyGate";
    private const string AppGate = "AppGateState";
    private const string UploadGate = "UploadGateState";
    private const string Loads = "LoadCount";
    private const string Refreshes = "RefreshCount";

    private protected GateTests(SampleAppFixture app)
        : base(app)
    {
    }

    // Long enough for the slowest thing the page does, the watchdog's five seconds, to happen.
    protected override TimeSpan LabelPatience => TimeSpan.FromSeconds(12);

    [Fact]
    public void ASecondTapWhileLoading_IsIgnored()
    {
        Open();

        Find("GateLoadButton").Click();
        Find("GateLoadButton").Click();

        Expect(Loads, "Loads started: 1, ignored: 1");
        Expect(AppGate, "Application gate: idle");
    }

    [Fact]
    public void AnotherCommandWhileLoading_IsHeldBackToo()
    {
        Open();

        Find("GateLoadButton").Click();
        Find("GateRefreshButton").Click();

        Expect(Refreshes, "Refreshes started: 0, ignored: 1");
        Expect(Loads, "Loads started: 1, ignored: 0");
        Expect(AppGate, "Application gate: idle");
    }

    [Fact]
    public void WorkInAGateOfItsOwn_HoldsNothingElseBack()
    {
        Open();

        Find("GateUploadButton").Click();
        Expect(UploadGate, "Uploads gate: busy");
        Find("GateLoadButton").Click();

        Expect(Loads, "Loads started: 1, ignored: 0");
        Expect(UploadGate, "Uploads gate: idle");
        Expect(AppGate, "Application gate: idle");
    }

    [Fact]
    public void AGateLeftBusy_IsClearedByTheWatchdog()
    {
        Open();

        Find("GateForgetButton").Click();
        Expect(AppGate, "Application gate: busy");

        Expect(AppGate, "Application gate: idle");
    }

    // Opens the page, waits for whatever an earlier test left in flight, and starts the counts over.
    private void Open()
    {
        if (!IsShowing(Ui.ByAutomationId(AppGate)))
        {
            Ui.OpenMenu();
            Ui.Driver.FindElement(Ui.ByText(PageTitle)).Click();
        }

        Expect(AppGate, "Application gate: idle");
        Expect(UploadGate, "Uploads gate: idle");
        Find("GateResetButton").Click();
        Expect(Loads, "Loads started: 0, ignored: 0");
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsGateTests(WindowsSampleApp app) : GateTests(app);

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidGateTests(AndroidSampleApp app) : GateTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosGateTests(IosSampleApp app) : GateTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystGateTests(MacCatalystSampleApp app) : GateTests(app);
