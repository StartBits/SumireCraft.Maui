---
name: code-analysis
description: Keeping src, tests and samples free of warnings and of the suggestions an editor shows (IDE and CA hints), and the full sweep that a new .NET SDK calls for. Use when the SessionStart hook says the SDK changed, when the maintainer asks for a sweep, when C# was edited with Claude Code outside VS Code, or when a diagnostic or an .editorconfig rule needs deciding.
---

# Code analysis

The build stays at 0 warnings, and the editor shows no suggestions either: in `src/`, `tests/` and
`samples/` alike. Two things keep it there.

- **Each edit.** After every Write or Edit of a C# file, the VS Code extension hands Claude what
  VS Code now reports for that file, inside `<ide_diagnostics>`, hints included. Fix them before
  moving on. The commit check (`.claude/hooks/check-commit.mjs`) rebuilds the projects the change
  touched and blocks one that prints a warning.

  Only warnings reach a build; a hint never does. So the hints of an edit are seen only when Claude
  Code runs inside VS Code. Working from a terminal, nothing reports them: tell the maintainer, and
  before the work is called done, open VS Code and run the sweep below over what changed (steps 1
  to 4; step 2 can be narrowed to the changed files by passing their `uri`).
- **A new SDK.** A file nobody edits stays as clean as it was, until a new SDK brings analyzers
  that see it differently. The SessionStart hook (`.claude/hooks/analysis-sdk.mjs`) compares
  `dotnet --version` with the SDK of the last sweep, and says so when they differ. Visual Studio
  updates bring new SDKs, so expect it after one.

## The full sweep

1. **Make sure VS Code is analysing.** `.vscode/settings.json` sets
   `dotnet.backgroundAnalysis.analyzerDiagnosticsScope` to `fullSolution`, so every file is analysed,
   not only the open ones. Call `getDiagnostics` on a file known to have a diagnostic, or on any file
   in `src/`. If nothing at all comes back, see *When VS Code shows nothing* below.
2. **Read every diagnostic.** Call `getDiagnostics` without a `uri`. The result is too large to read
   in place and is saved to a file; summarize it:

   ```powershell
   node .claude/skills/code-analysis/summarize-diagnostics.mjs <the saved file>
   ```

3. **Fix them**, in all three folders. Before removing a `using` or simplifying a name in a file
   under `Platforms/` or with a platform suffix, remember VS Code analyses one target framework per
   project; the full build in step 4 is what proves the other platforms still compile.
4. **Build every target framework** at 0 warnings, and run the unit tests:

   ```powershell
   dotnet build SumireCraft.Maui.slnx -c Release
   dotnet test --project tests/SumireCraft.Maui.UnitTests/SumireCraft.Maui.UnitTests.csproj -c Release
   ```

5. **Record the SDK** as swept, so the hook stops asking:

   ```powershell
   node .claude/hooks/analysis-sdk.mjs --record
   ```

The fixes are a meaningful change when they alter code, so they go to `maintainer-review` like any
other.

## When a suggestion cannot be followed

VS Code's C# extension carries a compiler newer than the SDK's, and can suggest syntax the build
rejects. Try the suggestion and build; if the SDK refuses it, find out why the editor thinks
otherwise rather than silencing the rule. `IDE0028` once suggested `[with(capacity)]`, a preview
feature, because `LangVersion` was `latest`; `Directory.Build.props` now leaves the language version
to the target framework.

A rule that the project disagrees with is decided by the maintainer, and goes in `.editorconfig`
with a comment saying why, the way the test-only exemptions (`IDE0017`, `IDE0022`, `CA1859`) are.

## When VS Code shows nothing

- **The .NET runtime was updated while VS Code was open.** The C# language server keeps running on
  the old one and fails on every request (`Could not load file or assembly 'System.Runtime.Numerics'`
  in the C# output log, under `%APPDATA%\Code\logs\<session>\window1\exthost\ms-dotnettools.csharp\`).
  Ask the maintainer to run *Developer: Reload Window*.
- **A change to `Directory.Build.props` or a project file** is not always picked up. Reload Window
  again.
- **A hint is not in the Problems panel.** Suggestions arrive in VS Code as hints, which the editor
  draws as faint dots and the Problems panel leaves out. `getDiagnostics` returns them.

## Raising the severity of whole categories

Tried and set aside on 2026-10-31: `dotnet_analyzer_diagnostic.category-Style.severity = warning`
makes the build report the style rules, but it also wakes the rules that are silent by default --
`IDE0058` (an unused return value) alone gave about 150 warnings, most in the tests -- and a
category setting overrides the `:severity` written on an option, so a category set to `none` also
turns off `csharp_prefer_braces = true:warning` and its kind. The editor's suggestions, kept at zero
as above, are the list to follow instead.
