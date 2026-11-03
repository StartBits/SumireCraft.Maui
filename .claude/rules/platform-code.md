---
paths:
  - "src/**/*.{Android,iOS,MacCatalyst,MaciOS,Windows}.cs"
  - "src/**/Platforms/**"
  - "src/**/*Handler.cs"
  - "build/Platforms.props"
---

# Platform-specific code (escape hatch)

Only when there is no cross-platform alternative, and say why. A file can be marked two ways, and
`build/Platforms.props` routes both. It is imported at the end of each `src` project file.

**By file name**, for the one or two files that complete a cross-platform type:

```
Exceptions/
├─ SafetyNet.cs
└─ SafetyNet.Windows.cs
```

**By folder**, for a widget that needs several files per platform:

```
Dividers/
├─ SmrDivider.cs
├─ SmrDividerHandler.cs
└─ Platforms/
   ├─ Android/SmrDividerHandler.cs
   ├─ MaciOS/SmrDividerHandler.cs     shared by iOS and MacCatalyst
   └─ Windows/SmrDividerHandler.cs
```

.NET MAUI does neither on its own. The SDK auto-excludes `Platforms/<Platform>/` at the **project
root** and nothing else, so both a nested folder and a `.Windows.cs` suffix are compiled into
every target framework until `build/Platforms.props` says otherwise. Verified behaviour:

| Suffix / folder | Compiled for |
| --- | --- |
| `.Android.cs` / `Platforms/Android/` | `net10.0-android` |
| `.MaciOS.cs` / `Platforms/MaciOS/` | `net10.0-ios` **and** `net10.0-maccatalyst` |
| `.iOS.cs` / `Platforms/iOS/` | `net10.0-ios` |
| `.MacCatalyst.cs` / `Platforms/MacCatalyst/` | `net10.0-maccatalyst` |
| `.Windows.cs` / `Platforms/Windows/` | `net10.0-windows*` |

**Spell these exactly as written.** The globs are case-sensitive on Linux and macOS, so a file
named `.windows.cs` is routed nowhere and lands in every build -- and Windows will not tell you,
because its file system does not care. `MaciOS` is .NET MAUI's own name for code shared by iOS and
Mac Catalyst; this repository used to call that folder `Apple`.

A widget's handler is registered in `UseSumireCraft()` through `ConfigureMauiHandlers`, never by
asking the application to register it. Registering roots the handler: a widget built from MAUI
primitives is trimmed away when an application does not use it, but a registered handler is kept
regardless. If one grows heavy enough to matter, register it only when the application opts in.

A widget built on a handler still goes in `Controls` with the rest (see *Packages* in `CLAUDE.md`).
A type that a Windows handler hands to WinUI is the one place `partial` is written for CsWinRT's
sake; say why in a comment.

iOS and Mac Catalyst run only on a Mac, where the UI tests drive them (the `ui-tests` skill). Say so
when a change could behave differently there, so that the maintainer can run them.
