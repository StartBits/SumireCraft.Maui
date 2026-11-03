using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// The handful of things the sample is reached by differently on each platform.
/// </summary>
internal abstract class SampleUi(AppiumDriver driver)
{
    public AppiumDriver Driver { get; } = driver;

    /// <summary>
    /// The control that opens the menu.
    /// </summary>
    public abstract By MenuToggle { get; }

    /// <summary>
    /// Opens the menu, for a test to pick a page out of it.
    /// </summary>
    public virtual void OpenMenu() => Driver.FindElement(MenuToggle).Click();

    /// <summary>
    /// Finds a view by the <c>AutomationId</c> the sample gave it.
    /// </summary>
    public abstract By ByAutomationId(string automationId);

    /// <summary>
    /// Finds something by the text it displays, ignoring anything that text could have been typed
    /// into. Used for menu entries, which are found by the <c>Title</c> their <c>ShellContent</c>
    /// carries, and for a badge, which has no id of its own to be found by.
    /// </summary>
    /// <remarks>
    /// Both of those are the sample's own strings, so they read the same whatever language the
    /// machine is set to.
    /// </remarks>
    public abstract By ByText(string text);

    /// <summary>
    /// Finds a button on an alert by the text the application gave it.
    /// </summary>
    public virtual By ByAlertButton(string text) => ByText(text);

    /// <summary>
    /// Scrolls until something below the fold is on screen, so that its position can be read.
    /// </summary>
    /// <param name="target">What has to be on screen.</param>
    /// <param name="scroller">The scroll view holding it.</param>
    /// <returns>The target, found again once it is on screen.</returns>
    public virtual AppiumElement BringIntoView(By target, By scroller) => Driver.FindElement(target);

    /// <summary>
    /// Scrolls a scroll view back to its top, for the tests that expect to find the page as it
    /// opened.
    /// </summary>
    public virtual void ScrollToTop(By scroller)
    {
    }

    /// <summary>
    /// Puts away anything that came up to type with, so it stops covering the page.
    /// </summary>
    public virtual void DismissKeyboard()
    {
    }
}

internal sealed class WindowsUi(AppiumDriver driver) : SampleUi(driver)
{
    // WinUI's NavigationView calls its own toggle "OK". Meaningless, but at least it is a
    // constant; the only other handle on that control is a label in the display language.
    public override By MenuToggle { get; } = new ByAccessibilityId("OK");

    public override By ByAutomationId(string automationId) => new ByAccessibilityId(automationId);

    // Text reaches UI Automation as the element's name. By.Name is no use here: Selenium turns
    // that into a CSS selector, which this driver has no idea what to do with. A text box holds
    // its contents as a value rather than a name, so nothing has to be excluded.
    public override By ByText(string text) => By.XPath($"//*[@Name='{text}']");

    // The tree holds what is off screen as well, but with nowhere for it: a zero size and a
    // position that means nothing. So the wheel is turned over the scroll view until the target
    // has a size. The driver turns no wheel through its own actions; this extension is the way.
    public override AppiumElement BringIntoView(By target, By scroller)
    {
        for (var turn = 0; turn < 20; turn++)
        {
            var element = Driver.FindElement(target);

            if (element.Size.Width > 0 && element.Size.Height > 0)
            {
                return element;
            }

            TurnWheel(scroller, -3);
        }

        return Driver.FindElement(target);
    }

    public override void ScrollToTop(By scroller) => TurnWheel(scroller, 60);

    /// <param name="clicks">Wheel clicks; negative scrolls down, as a wheel turned towards you does.</param>
    private void TurnWheel(By scroller, int clicks)
    {
        Driver.ExecuteScript(
            "windows: scroll",
            new Dictionary<string, object>
            {
                ["elementId"] = Driver.FindElement(scroller).Id,
                ["deltaY"] = clicks,
            });

        Thread.Sleep(200);
    }
}

internal sealed class AndroidUi(AppiumDriver driver) : SampleUi(driver)
{
    // The toolbar's only image button. Its description is in the display language; being the only
    // one is not.
    public override By MenuToggle { get; } = By.ClassName("android.widget.ImageButton");

    public override By ByAutomationId(string automationId) => By.Id(automationId);

    // The entries carry no resource id of their own, only the title as text. An entry box holds
    // its contents under the same attribute, so it has to be left out or a badge showing what was
    // typed would match the box it was typed into as well.
    public override By ByText(string text) =>
        By.XPath($"//*[@text='{text}' and not(self::android.widget.EditText)]");

    // Compared without regard to case. A dialog button may be styled to draw its text in
    // capitals, and whether the driver then reports the text as written or as drawn depends on the
    // Android version.
    public override By ByAlertButton(string text) =>
        By.XPath(
            "//android.widget.Button[translate(@text, 'abcdefghijklmnopqrstuvwxyz', " +
            $"'ABCDEFGHIJKLMNOPQRSTUVWXYZ')='{text.ToUpperInvariant()}']");

    public override void DismissKeyboard()
    {
        try
        {
            (Driver as AndroidDriver)?.HideKeyboard();
        }
        catch (WebDriverException)
        {
            // There was no keyboard up to put away.
        }
    }
}

/// <summary>
/// What iOS and Mac Catalyst have in common: both are UIKit, and both drivers read it through
/// XCTest.
/// </summary>
internal abstract class AppleUi(AppiumDriver driver) : SampleUi(driver)
{
    // UIKit gives the flyout's toggle the same constant name as WinUI does, with "Menu" as its
    // label in the display language.
    public override By MenuToggle { get; } = new ByAccessibilityId("OK");

    // An AutomationId becomes the accessibility identifier.
    public override By ByAutomationId(string automationId) => new ByAccessibilityId(automationId);

    // What a view displays is its label. A text field holds what was typed as its value, but it
    // may carry it as its label as well, so it is left out as on Android. So is the on-screen
    // keyboard, whose keys are labelled with the digits a badge shows.
    public override By ByText(string text) =>
        By.XPath(
            $"//*[@label='{text}' and not(self::XCUIElementTypeTextField) " +
            "and not(ancestor-or-self::XCUIElementTypeKeyboard)]");
}

internal sealed class IosUi(AppiumDriver driver) : AppleUi(driver)
{
    // XPath rather than By.ClassName, which Selenium 4 turns into a CSS selector the driver refuses.
    private static readonly By Keyboard = By.XPath("//XCUIElementTypeKeyboard");

    // The toolbar .NET MAUI puts above a keyboard is a sibling of the keyboard, not inside it, so
    // it is told apart from any other toolbar by sitting just above one.
    private static readonly By KeyboardDone = By.XPath(
        "//XCUIElementTypeToolbar[following::XCUIElementTypeKeyboard]/XCUIElementTypeButton");

    // The keyboard covers the lower half of the page, and anything under it cannot be tapped.
    // The driver's own way of hiding it looks for a key to press, and a numeric keyboard has none:
    // it answers "Did not know how to dismiss the keyboard". .NET MAUI puts a toolbar with a done
    // button above that keyboard, and pressing it is what a person would do.
    //
    // A keyboard still up afterwards fails the test. Going on would tap whatever lies under it and
    // read the keyboard's keys as the page's text, which is how these tests once passed seeing
    // nothing.
    //
    // On iOS 27 one press now and then left the keyboard up with its done button in view, and a
    // keyboard that did go could stay in the tree a moment longer. So the keyboard is given a second
    // to go after each press, and pressed again only if it is still there: pressing while it slides
    // away could land on whatever the toolbar was over.
    public override void DismissKeyboard()
    {
        var timeouts = Driver.Manage().Timeouts();
        timeouts.ImplicitWait = TimeSpan.Zero;

        try
        {
            if (!KeyboardIsUp())
            {
                return;
            }

            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);

            do
            {
                if (DateTime.UtcNow > deadline)
                {
                    throw new InvalidOperationException("The keyboard could not be put away, and it covers the page.");
                }

                PressDone();
            }
            while (!GoneWithin(TimeSpan.FromSeconds(1)));
        }
        finally
        {
            timeouts.ImplicitWait = SampleAppFixture.Patience;
        }

        // The keyboard leaves the tree before it has finished sliding away, and a tap sent in that
        // moment was seen to go nowhere: the next button pressed did not count, now and then.
        Thread.Sleep(500);
    }

    private bool KeyboardIsUp() => Driver.FindElements(Keyboard).Count > 0;

    private bool GoneWithin(TimeSpan time)
    {
        var until = DateTime.UtcNow + time;

        while (KeyboardIsUp())
        {
            if (DateTime.UtcNow > until)
            {
                return false;
            }

            Thread.Sleep(100);
        }

        return true;
    }

    private void PressDone()
    {
        try
        {
            if (Driver.FindElements(KeyboardDone).FirstOrDefault() is { } done)
            {
                done.Click();
            }
            else
            {
                (Driver as IOSDriver)?.HideKeyboard();
            }
        }
        catch (WebDriverException)
        {
            // The keyboard is asked about again, and the attempt repeated, until time runs out.
        }
    }
}

internal sealed class MacCatalystUi(AppiumDriver driver) : AppleUi(driver)
{
    // An alert is an AppKit sheet, not a UIKit view, and AppKit says it differently: the title and
    // the message are the value of a static text, and a button's text is its title. The page's own
    // text is still a label.
    public override By ByText(string text) =>
        By.XPath(
            $"//*[(@label='{text}' or (self::XCUIElementTypeStaticText and @value='{text}')) " +
            "and not(self::XCUIElementTypeTextField)]");

    public override By ByAlertButton(string text) =>
        By.XPath($"//XCUIElementTypeSheet//XCUIElementTypeButton[@title='{text}']");

    // The toggle sits in the window's toolbar, which AppKit draws rather than UIKit. Clicking it as
    // an element aborts the driver's agent inside XCTest, while it looks for the display the
    // element is on (macOS 27.2, Xcode 27.2). A click at the same point on the screen gets there.
    public override void OpenMenu()
    {
        var toggle = Driver.FindElement(MenuToggle);

        Driver.ExecuteScript(
            "macos: click",
            new Dictionary<string, object>
            {
                ["x"] = toggle.Location.X + (toggle.Size.Width / 2),
                ["y"] = toggle.Location.Y + (toggle.Size.Height / 2),
            });
    }
}
