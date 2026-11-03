using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// What one page of the sample should do, written once for every platform.
/// </summary>
/// <remarks>
/// <para>
/// Each page's tests are an abstract class derived from this one, and each platform derives a
/// class of its own from that, which says no more than which run of the sample it drives. A way of
/// reaching the sample that differs per platform belongs in <see cref="SampleUi"/>; a test only one
/// platform can run belongs in that platform's class. Neither belongs in an <c>if</c> inside a
/// test.
/// </para>
/// <para>
/// When a test fails, what was on screen is attached to its result. That is often the quickest way
/// to see why: an alert left up, a page that never opened.
/// </para>
/// </remarks>
public abstract class SamplePageTests : IDisposable
{
    private protected SamplePageTests(SampleAppFixture app)
    {
        app.SkipUnlessReady();

        Ui = app.Ui;
    }

    private protected SampleUi Ui { get; }

    /// <summary>
    /// How long a label is given to say what it should before the test fails on it.
    /// </summary>
    protected virtual TimeSpan LabelPatience => TimeSpan.FromSeconds(5);

    public void Dispose()
    {
        if (TestContext.Current.TestState?.Result == TestResult.Failed)
        {
            AttachScreenshot();
        }

        GC.SuppressFinalize(this);
    }

    protected AppiumElement Find(string automationId) =>
        Ui.Driver.FindElement(Ui.ByAutomationId(automationId));

    protected string Text(string automationId) => Find(automationId).Text;

    protected bool IsShowing(By locator)
    {
        var timeouts = Ui.Driver.Manage().Timeouts();

        // Asking whether something is absent is the one question the implicit wait makes slow.
        // The old value is put back from the constant rather than read: the Windows driver has no
        // answer for a request to read it.
        timeouts.ImplicitWait = TimeSpan.Zero;

        try
        {
            return Ui.Driver.FindElements(locator).Count > 0;
        }
        finally
        {
            timeouts.ImplicitWait = SampleAppFixture.Patience;
        }
    }

    /// <summary>
    /// Waits for a label to say what it should, then fails with what it said instead.
    /// </summary>
    protected void Expect(string automationId, string expected)
    {
        var deadline = DateTime.UtcNow + LabelPatience;
        string actual;

        do
        {
            actual = Text(automationId);

            if (actual == expected)
            {
                return;
            }

            Thread.Sleep(100);
        }
        while (DateTime.UtcNow < deadline);

        Assert.Equal(expected, actual);
    }

    private void AttachScreenshot()
    {
        try
        {
            var screenshot = Ui.Driver.GetScreenshot();

            TestContext.Current.AddAttachment("screenshot", screenshot.AsByteArray, "image/png");
        }
        catch (WebDriverException)
        {
            // The session is gone, which may well be what failed the test. There is nothing to
            // show, and the failure already says what happened.
        }
    }
}
