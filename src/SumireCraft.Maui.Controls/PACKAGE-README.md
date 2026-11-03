# SumireCraft.Maui.Controls

Fills the gaps .NET MAUI leaves to every application -- without taking a decision away from yours.

- **SmrBadgeView** -- a count or a dot over the corner of any view. It stays on the content's own
  corner, mirrors for right-to-left, and never pushes the layout around as the count grows.
- **SmrTapBehavior** -- a different command for one tap, a double tap, or five, read once when the
  tapping stops. For a view that does not answer taps itself, such as a label or an image; not
  for a `Button`.

This package brings in [SumireCraft.Maui.Core](https://www.nuget.org/packages/SumireCraft.Maui.Core)
as well: exception reporting, an opt-in safety net, and a busy gate. Core is released only when it
changes, so its version can be lower than this package's.

.NET 10 with .NET MAUI 10.0.20+ · Android 21+ · iOS 15+ · Mac Catalyst 15+ · Windows 10 1809+ · trimming and Native AOT compatible · MIT

.NET 11 has not been tried yet; see
[Supported versions](https://github.com/startbits/SumireCraft.Maui#supported-versions).

## Getting started

```csharp
builder
    .UseMauiApp<App>()
    .UseSumireCraft();
```

```xml
xmlns:smr="https://github.com/startbits/SumireCraft.Maui"

<smr:SmrBadgeView Text="7">
    <Button Text="Inbox" />
</smr:SmrBadgeView>
```

## Documentation

Everything else -- screenshots, every property and its default, and how each widget behaves and
why -- is in the [README on GitHub](https://github.com/startbits/SumireCraft.Maui#readme).

- [Changelog](https://github.com/startbits/SumireCraft.Maui/blob/main/CHANGELOG.md)
- [Issues](https://github.com/startbits/SumireCraft.Maui/issues)
