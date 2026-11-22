namespace SumireCraft.Maui.UITests;

/// <summary>
/// What a page observing its handler hears from the platform as it is opened and closed, which no
/// unit test can see: .NET MAUI disconnects a page as it closes, and how it does so is each
/// platform's own.
/// </summary>
/// <remarks>
/// The sample's pages that can be opened from the ObserveHandler page count themselves in and out of
/// a long-lived service from <c>ObserveHandler</c>, and the count is what each page shows.
/// </remarks>
public abstract class ObserveHandlerTests : SamplePageTests
{
    private const string PageTitle = "ObserveHandler";
    private const string Count = "OpenPageCount";
    private const string ObservedCount = "ObservedPageCount";
    private const string Back = "ObservedPageBack";

    private protected ObserveHandlerTests(SampleAppFixture app)
        : base(app)
    {
    }

    [Fact]
    public void APushedPage_IsConnectedAndDisconnectedOnceClosed()
    {
        Open();

        Find("PushObservedPage").Click();
        Expect(ObservedCount, "Open pages: 1");

        LetThePageSettle();
        Find(Back).Click();
        Expect(Count, "Open pages: 0");
    }

    [Fact]
    public void AModalPage_IsConnectedAndDisconnectedOnceClosed()
    {
        Open();

        Find("ShowObservedModal").Click();
        Expect(ObservedCount, "Open pages: 1");

        LetThePageSettle();
        Find(Back).Click();
        Expect(Count, "Open pages: 0");
    }

    /// <remarks>
    /// The page underneath stays in the tree on Mac Catalyst, so the page on top is reached by the
    /// identifiers the sample numbers by how deep it is.
    /// </remarks>
    [Fact]
    public void TwoPagesOneOnAnother_AreBothConnectedAndDisconnectedAsEachCloses()
    {
        Open();

        Find("PushObservedPage").Click();
        Expect(ObservedCount, "Open pages: 1");

        LetThePageSettle();
        Find("PushAnotherObservedPage").Click();
        Expect(ObservedCount + "2", "Open pages: 2");

        LetThePageSettle();
        Find(Back + "2").Click();
        Expect(ObservedCount, "Open pages: 1");

        LetThePageSettle();
        Find(Back).Click();
        Expect(Count, "Open pages: 0");
    }

    // A page is connected, and shows its count, while it is still sliding in. A click on Windows
    // during that slide was seen to reach nothing at all, so a button on a page just opened is pressed
    // once the slide is over.
    private static void LetThePageSettle() => Thread.Sleep(1000);

    private void Open()
    {
        if (IsShowing(Ui.ByAutomationId(Count)))
        {
            Expect(Count, "Open pages: 0");
            return;
        }

        Ui.OpenMenu();
        Ui.Driver.FindElement(Ui.ByText(PageTitle)).Click();

        Expect(Count, "Open pages: 0");
    }
}

[Collection(WindowsSampleApp.Collection)]
public sealed class WindowsObserveHandlerTests(WindowsSampleApp app) : ObserveHandlerTests(app);

[Collection(AndroidSampleApp.Collection)]
public sealed class AndroidObserveHandlerTests(AndroidSampleApp app) : ObserveHandlerTests(app);

[Collection(IosSampleApp.Collection)]
public sealed class IosObserveHandlerTests(IosSampleApp app) : ObserveHandlerTests(app);

[Collection(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystObserveHandlerTests(MacCatalystSampleApp app) : ObserveHandlerTests(app);
