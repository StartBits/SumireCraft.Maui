# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/).

`SumireCraft.Maui.Controls` is published with every release. `SumireCraft.Maui.Core` and
`SumireCraft.Core` are published only in a release that changes them, and otherwise keep the
version of the last release that did; a release's heading says what it leaves out
(*Controls only*, *without SumireCraft.Maui.Core* or *without SumireCraft.Core*).

A release that raises the minimum version of .NET MAUI says so under *Changed*.

## [1.1.0]

### Added

- `SmrField` in `SumireCraft.Maui.Controls`, which puts a title above whatever takes the input and
  its error below. The error's line is kept by default, so that nothing below moves when an error
  comes and goes; `ReserveErrorSpace` turns that off. `ReserveTitleSpace` keeps a line for a missing
  title, so that a field without one lines up with a titled one beside it. The title and the error
  are restyled through the style classes `SmrFieldTitle` and `SmrFieldError`, from App.xaml or a
  page's resources, names which `TitleStyleClass` and `ErrorStyleClass` change. `IsRequired` puts a
  mark after the title, `*` unless `RequiredMark` says otherwise, restyled through a style for
  `Span` under the key `SmrFieldRequiredMark`; `RequiredMarkDescription` gives a screen reader the
  words to read in its place.
- `SmrTemplatedView` in `SumireCraft.Maui.Controls`, a content view that brings a default control
  template which an application's styles replace like any other, including a style for every
  templated view at once. `SmrField` is built on it, and an application's own widgets can be too.
- `SmrDatePicker` and `SmrTimePicker` in `SumireCraft.Maui.Controls`, .NET MAUI's `DatePicker` and
  `TimePicker` with a `DateTimeInput` property. Bound to one input, the two take a date and a time,
  either of which can stay empty -- but for a time on Windows with .NET MAUI 10.0.20, whose
  `TimePicker` turns an empty time into midnight. A style for every `DatePicker` or `TimePicker`
  reaches them too.
- `DateTimeInputDate` and `DateTimeInputTime` in `SumireCraft.Maui.Controls`, attributes that give
  a `partial` class derived from a picker of another library the same `DateTimeInput` property,
  written by a source generator that comes inside the package, with a constructor that starts the
  picker empty and then calls `partial void OnCreated()`, for settings every picker of the kind
  shares. `SmrDatePicker` and `SmrTimePicker` are written by it too. A class it cannot write for
  fails to build with an `SMR` error. XAML that .NET MAUI turns into C# while building
  (`MauiXamlInflator` set to `SourceGen`) cannot see the property and fails with `MAUIX2002`; a
  picker in a class library of its own, or a page compiled by XamlC, can.
- A new package, `SumireCraft.Core`, which depends on no UI framework and targets plain `net10.0`.
  It holds `DateTimeInput` in the namespace `SumireCraft.DateTimes`: a date and a time that are
  entered separately and can each be empty, read and written through `DateOnly`, `TimeOnly`,
  `DateTime` or `TimeSpan`. `SumireCraft.Maui.Controls` depends on it, and like
  `SumireCraft.Maui.Core` it is released only when it changes.

## [1.0.0] - 2026-11-03

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

[1.1.0]: https://github.com/startbits/SumireCraft.Maui/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/startbits/SumireCraft.Maui/releases/tag/v1.0.0
