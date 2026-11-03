# SumireCraft.Maui.Core

The building blocks of SumireCraft.Maui, useful on their own without any of the widgets.

- **Exception reporting** -- one place to send every exception you catch: log it, show it, or
  forward it to your crash reporter.
- **Safety net** (opt-in) -- reports what nothing caught to your exception handlers, as far as .NET
  hears of it, before the process goes down.
- **Busy gate** -- is anything in flight, anywhere in the app? Keeps a second tap from starting
  the same work twice.

.NET 10 with .NET MAUI 10.0.20+ · Android 21+ · iOS 15+ · Mac Catalyst 15+ · Windows 10 1809+ · trimming and Native AOT compatible · MIT

.NET 11 has not been tried yet; see
[Supported versions](https://github.com/startbits/SumireCraft.Maui#supported-versions).

It also targets plain `net10.0`, so an application's unit test project can reference it.

This package is released only when it changes, so its versions skip numbers: 1.0.1 may be followed
by 1.0.3. A missing number is a release that changed only
[SumireCraft.Maui.Controls](https://www.nuget.org/packages/SumireCraft.Maui.Controls).

## Getting started

```csharp
builder
    .UseMauiApp<App>()
    .UseSumireCraftCore();
```

Nothing app-wide is switched on by this call. The safety net stays off until
`options => options.InstallSafetyNet = true` turns it on, and is best left off where a crash
reporter such as Sentry already listens to the same events.

Every report, the safety net's included, goes to the `IExceptionHandler`s you register, and only
there. With none registered, nothing is recorded anywhere.

```csharp
using SumireCraft.Maui.Exceptions;

builder.Services.AddSingleton<IExceptionHandler, LoggingExceptionHandler>();
```

`LoggingExceptionHandler` writes to your `ILogger`, which in .NET MAUI's template writes nowhere
outside a Debug build until you give it a provider of your own.

Using the widgets as well? Install
[SumireCraft.Maui.Controls](https://www.nuget.org/packages/SumireCraft.Maui.Controls) and call
`UseSumireCraft()` instead.

## Documentation

How each piece behaves, and why, is in the
[README on GitHub](https://github.com/startbits/SumireCraft.Maui#readme).

- [Changelog](https://github.com/startbits/SumireCraft.Maui/blob/main/CHANGELOG.md)
- [Issues](https://github.com/startbits/SumireCraft.Maui/issues)
