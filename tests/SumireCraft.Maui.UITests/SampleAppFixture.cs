using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// One run of the sample application, shared by every test in a collection.
/// </summary>
/// <remarks>
/// The session opens on first use rather than in the constructor. Whether there is anything to
/// drive the sample with is asked by each test as it starts, after the fixture has been built, and
/// a fixture that failed to start the application would fail every test in the collection instead
/// of letting each one say why it was skipped.
/// </remarks>
public abstract class SampleAppFixture : IDisposable
{
    /// <summary>
    /// How long a lookup waits for something to turn up. The first screen takes a moment to come
    /// up, and every lookup after that is immediate.
    /// </summary>
    /// <remarks>
    /// Held here because a test that lowers it to ask whether something is absent has to put it
    /// back afterwards, and the Windows driver does not answer a request to read it.
    /// </remarks>
    public static TimeSpan Patience { get; } = TimeSpan.FromSeconds(15);

    private readonly Lazy<string?> _reasonToSkip;
    private AppiumDriver? _driver;
    private SampleUi? _ui;

    protected SampleAppFixture()
    {
        _reasonToSkip = new(() =>
            !AppiumServer.IsListening ? AppiumServer.StartHint
            : SampleApp.Find(Platform) is null ? SampleApp.BuildHint(Platform)
            : ReasonToSkipOnThisPlatform());
    }

    /// <summary>
    /// The platform this run of the sample is on.
    /// </summary>
    public abstract SamplePlatform Platform { get; }

    public AppiumDriver Driver => _driver ??= Start();

    internal SampleUi Ui => _ui ??= CreateUi(Driver);

    /// <summary>
    /// Skips the current test when there is nothing to drive the sample with.
    /// </summary>
    /// <remarks>
    /// Driving the sample takes an Appium server and a built application, and whatever else the
    /// platform asks in <see cref="ReasonToSkipOnThisPlatform"/>: a Mac for iOS and Mac Catalyst,
    /// and a simulator already running for iOS. None of that is worth failing a run over when
    /// someone simply asked the solution for its tests, so the reason is reported as a skip instead.
    /// Whether an Android emulator is running is not checked, and neither is developer mode on
    /// Windows, because the error Appium gives for each already says so. The question is
    /// asked once per run: the answer does not change while the tests run, and finding that nobody
    /// is listening on a port takes a couple of seconds each time.
    /// </remarks>
    public void SkipUnlessReady()
    {
        if (_reasonToSkip.Value is { } reason)
        {
            Assert.Skip(reason);
        }
    }

    public void Dispose()
    {
        if (_driver is null)
        {
            return;
        }

        _driver.Quit();
        _driver.Dispose();
        _driver = null;
        _ui = null;

        GC.SuppressFinalize(this);
    }

    protected abstract AppiumDriver Start();

    private protected abstract SampleUi CreateUi(AppiumDriver driver);

    /// <summary>
    /// Anything else this platform needs before the sample can be driven, asked once the server
    /// and the build have been found.
    /// </summary>
    protected virtual string? ReasonToSkipOnThisPlatform() => null;

    protected static string Locate(SamplePlatform platform) =>
        SampleApp.Find(platform) ?? throw new InvalidOperationException(SampleApp.BuildHint(platform));

    protected static TDriver Ready<TDriver>(TDriver driver)
        where TDriver : AppiumDriver
    {
        driver.Manage().Timeouts().ImplicitWait = Patience;

        return driver;
    }
}

/// <summary>
/// Ties every test class that drives the sample on one platform to a single run of it.
/// </summary>
/// <remarks>
/// xUnit runs test classes in parallel, which without this would start one copy of the sample per
/// class and leave them fighting over the same window. Classes in the same collection run one
/// after another; the collections still run alongside each other, which is what makes a run cover
/// every platform this machine can drive in the time of the slowest.
/// </remarks>
[CollectionDefinition(WindowsSampleApp.Collection)]
public sealed class WindowsSampleCollection : ICollectionFixture<WindowsSampleApp>;

/// <inheritdoc cref="WindowsSampleCollection"/>
[CollectionDefinition(AndroidSampleApp.Collection)]
public sealed class AndroidSampleCollection : ICollectionFixture<AndroidSampleApp>;

/// <inheritdoc cref="WindowsSampleCollection"/>
[CollectionDefinition(IosSampleApp.Collection)]
public sealed class IosSampleCollection : ICollectionFixture<IosSampleApp>;

/// <inheritdoc cref="WindowsSampleCollection"/>
[CollectionDefinition(MacCatalystSampleApp.Collection)]
public sealed class MacCatalystSampleCollection : ICollectionFixture<MacCatalystSampleApp>;

public sealed class WindowsSampleApp : SampleAppFixture
{
    /// <summary>
    /// Names the collection every test class driving the sample on Windows belongs to.
    /// </summary>
    public const string Collection = "sample on Windows";

    public override SamplePlatform Platform => SamplePlatform.Windows;

    protected override AppiumDriver Start() => Ready(new WindowsDriver(
        AppiumServer.Uri,
        new AppiumOptions
        {
            PlatformName = "Windows",
            AutomationName = "Windows",
            App = Locate(Platform),
        },
        TimeSpan.FromMinutes(2)));

    private protected override SampleUi CreateUi(AppiumDriver driver) => new WindowsUi(driver);
}

public sealed class AndroidSampleApp : SampleAppFixture
{
    /// <summary>
    /// Names the collection every test class driving the sample on Android belongs to.
    /// </summary>
    public const string Collection = "sample on Android";

    public override SamplePlatform Platform => SamplePlatform.Android;

    protected override AppiumDriver Start()
    {
        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
            App = Locate(Platform),
        };

        // A .NET MAUI application starts a splash activity first, so waiting for the one named in
        // the package would time out.
        options.AddAdditionalAppiumOption("appWaitActivity", "*");
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);

        // Every build of the sample carries the same version code, and without this the driver
        // takes one already on the device to be the same application and skips installing the
        // new one, so the tests quietly drive whatever was built last time.
        options.AddAdditionalAppiumOption("enforceAppInstall", true);

        return Ready(new AndroidDriver(AppiumServer.Uri, options, TimeSpan.FromMinutes(5)));
    }

    private protected override SampleUi CreateUi(AppiumDriver driver) => new AndroidUi(driver);
}

public sealed class IosSampleApp : SampleAppFixture
{
    /// <summary>
    /// Names the collection every test class driving the sample on iOS belongs to.
    /// </summary>
    public const string Collection = "sample on iOS";

    private readonly Lazy<string?> _simulator = new(Simulators.FirstBooted);

    public override SamplePlatform Platform => SamplePlatform.Ios;

    // Asked before the simulators are, so that nothing called xcrun is started on another system.
    protected override string? ReasonToSkipOnThisPlatform() =>
        !OperatingSystem.IsMacOS() ? "iOS is driven on a Mac only."
        : _simulator.Value is null
            ? "No iPhone simulator is running (an iPad is not driven). Start one first, for example: xcrun simctl boot \"iPhone 17\""
            : null;

    protected override AppiumDriver Start()
    {
        var options = new AppiumOptions
        {
            PlatformName = "iOS",
            AutomationName = "XCUITest",
            App = Locate(Platform),
        };

        // The simulator already running, rather than one picked by name and version: those change
        // with every Xcode, and booting another would take longer than the tests themselves.
        options.AddAdditionalAppiumOption("udid", _simulator.Value);

        // The first session builds the driver's agent with Xcode, which takes minutes.
        options.AddAdditionalAppiumOption("wdaLaunchTimeout", (int)TimeSpan.FromMinutes(5).TotalMilliseconds);

        return Ready(new IOSDriver(AppiumServer.Uri, options, TimeSpan.FromMinutes(10)));
    }

    private protected override SampleUi CreateUi(AppiumDriver driver) => new IosUi(driver);
}

public sealed class MacCatalystSampleApp : SampleAppFixture
{
    /// <summary>
    /// Names the collection every test class driving the sample on Mac Catalyst belongs to.
    /// </summary>
    public const string Collection = "sample on Mac Catalyst";

    public override SamplePlatform Platform => SamplePlatform.MacCatalyst;

    // A build on Windows can leave a bundle behind as well, which nothing there can run.
    protected override string? ReasonToSkipOnThisPlatform() =>
        OperatingSystem.IsMacOS() ? null : "Mac Catalyst is driven on a Mac only.";

    protected override AppiumDriver Start()
    {
        var options = new AppiumOptions
        {
            PlatformName = "mac",
            AutomationName = "Mac2",
        };

        // The Mac2 driver launches a bundle by its path under this name, and has no use for
        // AppiumOptions.App.
        options.AddAdditionalAppiumOption("appPath", Locate(Platform));

        return Ready(new MacDriver(AppiumServer.Uri, options, TimeSpan.FromMinutes(10)));
    }

    private protected override SampleUi CreateUi(AppiumDriver driver) => new MacCatalystUi(driver);
}
