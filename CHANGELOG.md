# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/).

A release that leaves `SumireCraft.Maui.Core` unchanged publishes `SumireCraft.Maui.Controls`
alone, and says so in its heading; Core keeps the version of the last release that changed it.

A release that raises the minimum version of .NET MAUI says so under *Changed*.

## [1.0.0]

The first release, of both packages.

### Added

- `UseSumireCraft()` in `SumireCraft.Maui.Controls` and `UseSumireCraftCore()` in
  `SumireCraft.Maui.Core`, which register the library with an application.
- `SumireCraftOptions`, which `UseSumireCraftCore()` and `UseSumireCraft()` take to switch the
  safety net on or to set a busy timeout.
- `SmrBadgeView` and `BadgePlacement` in `SumireCraft.Maui.Controls`, which put a small badge over
  one corner of whatever they wrap. `BadgeTemplate` puts any view in the corner instead of the
  label, and a derived type can override `CreateDefaultBadge()` to change the look for everyone who
  uses it.
- `SmrTapBehavior` and `SmrTap` in `SumireCraft.Maui.Controls`, which run a different command
  depending on how many times a view was tapped.
- The XAML namespace `https://github.com/startbits/SumireCraft.Maui`, which reaches every type in
  `SumireCraft.Maui.Controls`, with `smr` as its suggested prefix.
- `BadgeCounter` in `SumireCraft.Maui.Core`, which turns a count into the text and visibility a
  badge binds to: nothing at zero, `99+` above the maximum, both adjustable.
- `TapSequence`, `ITapTrigger`, `TapMatch` and `TapTriggeredEventArgs` in `SumireCraft.Maui.Core`,
  the tap-counting rules on their own, with no dependency on views or gestures.
- `InheritBindingContextFrom()` in `SumireCraft.Maui.Core`, which keeps one bindable object's
  binding context in step with another's. .NET MAUI does not pass a view's binding context to
  anything a behavior holds of its own, so a `{Binding}` written there resolves to nothing without
  it.
- `IExceptionSink`, `ExceptionSink`, `IExceptionHandler`, `ExceptionReport` and `ExceptionOrigin`
  in `SumireCraft.Maui.Core`: one place to report an exception that was caught, handed to every
  registered handler in order. A handler that throws never stops the others or reaches the caller.
- `LoggingExceptionHandler` in `SumireCraft.Maui.Core`, which writes every report to the
  application's logger.
- `SumireCraftExceptions` in `SumireCraft.Maui.Core`, which reports to the same sink from code
  that cannot have it injected.
- A safety net in `SumireCraft.Maui.Core` that reports what nothing caught --
  `AppDomain.UnhandledException`, `TaskScheduler.UnobservedTaskException` and, on Windows, the WinUI
  application's own event -- as `ExceptionOrigin.Unhandled`. It only records; it never marks an
  exception handled. Off unless `InstallSafetyNet` turns it on.
- `IBusyGate` and `BusyGate` in `SumireCraft.Maui.Core`, which track what is doing work so a second
  tap can be ignored while the first is still running. Owners are held weakly, so a forgotten exit
  leaks nothing.
- `BusyWatchdog` and `BusyTimeoutException` in `SumireCraft.Maui.Core`, which clear a gate that has
  stayed busy too long and report that it happened.
- `WeakReferenceList<T>` in `SumireCraft.Maui.Core`, a list that tracks objects without keeping
  them alive.
- Both packages are marked trimmable and compatible with Native AOT, and are built with the trim
  and AOT analyzers on.
- Both packages also target plain `net10.0`, so an application's unit test project can reference
  them.
- `SumireCraft.Maui.Controls` depends on `SumireCraft.Maui.Core` below its own next major version,
  so NuGet warns when an application takes a newer Core than that.

[1.0.0]: https://github.com/startbits/SumireCraft.Maui/releases/tag/v1.0.0
