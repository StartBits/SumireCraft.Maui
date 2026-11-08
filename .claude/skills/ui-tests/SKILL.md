---
name: ui-tests
description: Running, writing or debugging the Appium UI tests in tests/SumireCraft.Maui.UITests, and adding or changing a page of the sample application they drive. Covers the commands, how the sample and the tests are laid out, and what the Windows, Android, iOS (XCUITest) and Mac Catalyst (Mac2) drivers do that they do not document.
---

# UI tests and the sample application

What a unit test cannot see is a real platform: gestures arriving, a layout measured on a screen, a
dialog coming up. UI tests drive the sample application through Appium for those.

## Running them

```powershell
appium --address 127.0.0.1                      # leave running in another terminal

dotnet build samples/SumireCraft.Maui.Sample -f net10.0-windows10.0.19041.0
dotnet build samples/SumireCraft.Maui.Sample -f net10.0-android    # with an emulator running

dotnet test --project tests/SumireCraft.Maui.UITests/SumireCraft.Maui.UITests.csproj
dotnet test --project tests/SumireCraft.Maui.UITests/SumireCraft.Maui.UITests.csproj -- --filter-class "*.Android*"
```

Start Appium with `--address 127.0.0.1`, on the Mac too. Without it, Appium listens on every network
the machine is on (`0.0.0.0`, seen on Windows with Appium 3.7.0), and whoever reaches the port can
drive the emulator, the simulator or, through WinAppDriver and Mac2, the desktop. The tests connect
to `127.0.0.1` already. The flag covers Appium's own port, 4723, and nothing a driver opens for
itself -- WinAppDriver's 4724, WebDriverAgent's 8100, Mac2's 10100 -- whose addresses have not been
looked at yet. During a run, `Get-NetTCPConnection -State Listen` on Windows or
`lsof -nP -iTCP -sTCP:LISTEN` on a Mac shows them.

On a Mac, for iOS (on a simulator that is already running) and Mac Catalyst:

```sh
appium --address 127.0.0.1                      # leave running in another terminal
xcrun simctl boot "iPhone 17"                   # any iPhone simulator; the tests drive the first one running

dotnet build samples/SumireCraft.Maui.Sample -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64
dotnet build samples/SumireCraft.Maui.Sample -f net10.0-maccatalyst

dotnet test --project tests/SumireCraft.Maui.UITests/SumireCraft.Maui.UITests.csproj -- --filter-class "*.Ios*" --filter-class "*.MacCatalyst*"
```

When the installed .NET for iOS asks for a different Xcode than the one installed, the build stops
before compiling anything. Stop there and ask the maintainer to install the Xcode it names: what is
tested should be a combination Microsoft supports. `-p:ValidateXcodeVersion=false` gets past the
check, but it switches a check off, so it is the maintainer's call alone, and a report of what ran
says it was used.

.NET for Mac Catalyst 27.0 refuses to build an application whose `SupportedOSPlatformVersion` is
below 17.0. The libraries keep 15.0 (`Directory.Build.props`), which they still build at, because
what the packages promise is the maintainer's decision and not the SDK's; the sample asks for 17.0
(`samples/Directory.Build.props`). When an SDK raises what it accepts from an application again,
raise the sample's and leave the libraries' alone.

Without a server on port 4723, or without a built sample for that platform, the tests report
themselves as skipped rather than failing. They are not part of CI, which runs the unit tests by
name. A test that fails attaches a screenshot of the sample as it was, under `TestResults/`.

The tests drive whichever build is **newest**, looking in `bin/Debug` and `bin/Consumer` and
ignoring `bin/Release`. So building one configuration and then testing is enough, but building two
and then testing drives the second. `SUMIRE_SAMPLE_WINDOWS`, `SUMIRE_SAMPLE_ANDROID`,
`SUMIRE_SAMPLE_IOS` and `SUMIRE_SAMPLE_MACCATALYST` name a file (or, for the last two, an `.app`
bundle) outright when that guess is not wanted. Testing against the packages rather than the
projects is the `consumer-build` skill.

**The Windows tests move the maintainer's real mouse cursor** (`windows: scroll` especially). Say so
before running them. A test that fails once and passes again may have collided with the maintainer
rather than found a bug.

**The Mac Catalyst tests move the maintainer's real mouse cursor too**, and take over the screen
while they run. Say so before running them.

An Appium server started in the background survives stopping the task: stop the `node.exe` whose
command line contains `appium`. On a Mac, the Mac2 driver's agent can outlive its session as well,
and holds port 10100 so that the next session fails at once with "port #10100 is busy" or
`ECONNREFUSED 127.0.0.1:10100`: `pkill -f WebDriverAgentRunner`. A session started straight after
that can fail once or twice with the agent's xcodebuild exiting with code 65; the next one starts.

Start the emulator before Appium. Close the sample before building, or its running executable
fails the build.

## The sample

The sample is a flyout with one page per thing worth showing, under `Pages/`. A widget that needs a
demonstration gets a page of its own rather than another section on a shared one, so that a test
can open exactly the page it is about, and so that a page stays small enough to take in at a
glance.

Tests reach a page through the menu, the same way a person does. `SampleUi` holds the things
said differently per platform, with the reason next to each; the short version is that an entry is
found by the `Title` its `ShellContent` carries, because the `AutomationId` reaches neither
platform's flyout.

Keep a page narrow. Android leaves anything off the right-hand edge out of the tree altogether, so
a row that overflows is a row whose last element no test can find.

## The tests

The tests for a page are written once, in an abstract `<Feature>Tests` (`TapTests`, `BadgeTests`,
`ExceptionTests`) derived from `SamplePageTests`. At the bottom of the same file,
`Windows<Feature>Tests`, `Android<Feature>Tests`, `Ios<Feature>Tests` and
`MacCatalyst<Feature>Tests` derive from it in one line each, which says no
more than which collection, and so which run of the sample, they belong to. A platform is added by
adding a line. Where the platforms differ, the difference goes in `SampleUi` if it is in how the
sample is reached, or in that platform's class if only that platform can run the test -- never in
an `if` inside a test.

The Windows driver does not apply the implicit wait to a search by text (`SampleUi.ByText`): it
answers at once that something still on its way, such as an alert, does not exist. Poll for those,
as `ExceptionTests.WaitFor` does. A search by `AutomationId` does wait.

## What the drivers do not document, found by trying

- Only `element.Click()` reaches MAUI's gesture recognizers. W3C actions and the Windows driver's
  `windows: click` do not, so a burst of taps is one round trip per tap. That is why the sample's
  interval is `TapTests.SampleInterval` (2000 ms) rather than the default; keep the two in step.
- On Android an `AutomationId` lands in `resource-id`, so `SampleUi` finds it with `By.Id`.
  `appWaitActivity` is `*` because MAUI starts from a splash activity, and `enforceAppInstall` is on
  because UiAutomator2 otherwise keeps an APK of the same version code and drives the old build.
- A Debug APK starts only with `-p:EmbedAssembliesIntoApk=true`; without it, Fast Deployment leaves the
  assemblies out and the application exits at once. A Debug build of the whole solution rebuilds the
  APK without it, so build the sample again before running the Android tests.
- When the Android application stops at once in any configuration, read `adb logcat -d -b crash`
  before guessing. For a Release or Consumer build, *When every Android test fails at once* in the
  `consumer-build` skill names one cause.
- Shapes such as `Ellipse` have no automation peer on Windows and cannot be found. Draw anything a test
  must find with a `Label`.
- Avoid "OK" and "Close" as the text of an alert's buttons: Windows already shows elements with those
  names. (WinUI's `NavigationView` calls its own menu toggle "OK".)
- An alert from `DisplayAlertAsync` does appear in the Windows tree; its title, message and buttons
  are found by their text.
- `By.Name` is no use on Windows: Selenium 4 turns it into a CSS selector, which WinAppDriver refuses.
  Search by text with XPath, as `SampleUi.ByText` does.
- The Windows driver accepts a new implicit wait but cannot report the current one. To lower it for a
  moment, put it back from the constant (`SampleAppFixture.Patience`) rather than from a value read.
- `Appium.WebDriver` 9.0.0 has no `AppiumBy`: use `new ByAccessibilityId(...)`. The application goes
  in `AppiumOptions.App`; `AddAdditionalAppiumOption("app", ...)` throws.
- On iOS and Mac Catalyst an `AutomationId` is the accessibility identifier, and what a view shows is
  its `label`. The flyout's toggle is called "OK" there too.
- iOS's on-screen keyboard is in the tree, and its keys are labelled with digits, so a search for a
  badge's "7" finds the key unless the keyboard is left out (`AppleUi.ByText` does).
- iOS: the driver cannot hide a numeric keyboard ("Did not know how to dismiss the keyboard"), and
  a test that goes on regardless finds and taps the page under it, so it can pass while seeing
  nothing. `IosUi.DismissKeyboard` presses the done button .NET MAUI puts on the keyboard's
  toolbar. Look at a screenshot whenever a test types into an entry. On iOS 27 one press now and
  then left the keyboard up with the button in view, and a keyboard that went could stay in the
  tree a moment longer, so the method gives it a second after each press and presses again, for up
  to five seconds.
- iOS is tested on an iPhone simulator only, and `Simulators.FirstBooted` passes over an iPad. On
  an iPad (iOS 27.0) the driver reported the done button well below where it was drawn, and
  pressing it left the keyboard up; in another run the keyboard had become a floating one over the
  page, which stayed on the simulator until `xcrun simctl erase`.
- An iOS runtime may ship for one device alone: iOS 27.1 and one of the iOS 27.2 runtimes take only
  the iPhone Duo. `xcrun simctl list runtimes -j` lists what each runtime supports; an iPhone on the
  general runtime nearest the version wanted is the one to boot.
- iOS and Mac Catalyst report an element whole, past its parent's edge, as Windows does, so the
  badge tests that measure the overhang (`UnclippedBadgeTests`) run there; Android alone cuts an
  element to its parent. Because the frame is reported whole either way, where an element is says
  nothing about whether it was drawn: `ABadge_IsDrawnPastItsCorner` reads the pixels past the
  corner instead. To see it fail, set `IsClippedToBounds="True"` on the `HorizontalStackLayout`
  holding the badged buttons; on the `SmrBadgeView` itself it clips nothing on iOS.
- Mac Catalyst: clicking the flyout's toggle as an element aborts the Mac2 driver's agent inside
  XCTest (`SLSGetDisplaysWithRect`; macOS 27.2, Xcode 27.2), because the toggle is in the toolbar
  AppKit draws. `MacCatalystUi.OpenMenu` clicks the same point with `macos: click`. Other elements
  click normally.
- Mac Catalyst: an alert is an AppKit sheet. Its title and message are a static text's `value`, its
  buttons' text their `title`.
- Built with Xcode 27 or later, an iOS or Mac Catalyst application that does not adopt the UIScene
  lifecycle stops at launch with `EXC_BREAKPOINT` in
  `_UIApplicationEvaluateRuntimeIssueForNoSceneLifecycleAdoption`. The sample's `SceneDelegate` and
  the `UIApplicationSceneManifest` in its Info.plist files are what prevent it; an iOS 26 simulator
  does not enforce it, a Mac on macOS 27 does.
- A Debug Mac Catalyst build updates the bundle for this machine's architecture alone and leaves a
  universal one beside it from an earlier build, which is why the tests pick a bundle by when its
  Info.plist was written.
- When stuck, save `driver.GetScreenshot()` and `driver.PageSource` and look at them before guessing,
  and change one thing at a time.

## One-time setup

`npm i -g appium --before=<a date a week ago>` (*Security* in `CLAUDE.md`; it does not reach the
drivers, which Appium installs through npm itself), then `appium driver install windows` and
`appium driver install uiautomator2`.
Windows also needs WinAppDriver (`appium driver run windows install-wad`, which asks to elevate) and
developer mode switched on. Android needs `JAVA_HOME` and `ANDROID_HOME` set **in the environment the
Appium server is started from** -- the driver shells out to `java` and to the SDK's own tools, and
says so plainly when either is missing.

On a Mac: `appium driver install xcuitest` and `appium driver install mac2`, with Xcode installed.
The first session of each driver builds its agent with Xcode, which takes minutes.

What follows needs `sudo` or changes the whole machine. The maintainer does it by hand; Claude
names the command and does not run it.

- Mac Catalyst needs `sudo automationmodetool enable-automationmode-without-authentication`, and
  the app running the tests (the terminal or VS Code) allowed under Privacy & Security,
  Accessibility. Both let anything running in that app drive other applications' UI -- Claude Code
  included, when it runs in that app, and so its own permission prompts and System Settings too.
  So they are switched on for a test run and off again afterwards, by
  `sudo automationmodetool disable-automationmode-without-authentication` and by removing the app
  from that list; or given to a terminal Claude Code does not run in, which then runs the tests.
  While they are on, Claude drives the sample alone: no `osascript` or System Events, and no
  `macos: click` outside the sample's window. When the run is over, Claude reminds the maintainer
  to switch them off.
- Install the workload as `sudo TMPDIR=/tmp dotnet workload install maui`: without `TMPDIR`, NuGet
  fails with "'NuGet-Migrations' ... lstat("")". It also leaves `~/.local/share/NuGet` owned by
  root, which turns every later restore's vulnerability check into warning NU1900 until
  `sudo chown -R $(whoami) ~/.local/share/NuGet`.
