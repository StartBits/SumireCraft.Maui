# Security policy

Thank you for looking. A report that keeps an application safe is welcome however small it seems,
and you do not need to know the fix.

## Reporting a vulnerability

Please report it privately, through GitHub's
[private vulnerability reporting](https://github.com/StartBits/SumireCraft.Maui/security/advisories/new),
not in an issue: an issue is public from the moment it is opened.

What helps, as far as you have it:

- what goes wrong, and what someone could do with it;
- the package and its version, and the .NET MAUI version;
- the platforms where you saw it;
- steps, or a few lines of XAML or C#, that make it happen.

## What happens next

SumireCraft.Maui is kept by one person, so there is no promised date. You will hear back in the
report itself, where the fix is discussed with you before anything is made public. The fix comes
in a patch release, and the advisory is published with it, crediting you unless you would rather
it did not.

## Supported versions

Fixes go to the latest release only. [Supported versions](README.md#supported-versions) in the
README says what that means for an application that stays on an earlier .NET.

## What this covers

- The three packages on nuget.org: `SumireCraft.Maui.Controls`, `SumireCraft.Maui.Core` and
  `SumireCraft.Core`.
- How they are built and published: the workflows in this repository, and anything that could put
  something into a package that is not in its source.
- What runs by itself for someone who clones this repository and opens it: the build files, the
  Claude Code settings and hooks under `.claude/`, and the editor settings under `.vscode/`.

The library handles no secrets and no user data of its own, but it runs inside every application
that installs it. The sample application is not shipped: report a problem in it privately if it
shows something wrong in the packages, and in an issue otherwise.

A vulnerability in .NET MAUI or .NET itself goes to Microsoft, as the
[.NET MAUI security policy](https://github.com/dotnet/maui/security/policy) says.
