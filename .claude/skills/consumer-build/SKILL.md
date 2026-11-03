---
name: consumer-build
description: Checking that the NuGet packages themselves are right by building the sample in the Consumer configuration against packed packages, and seeing what trimming does to a change with a Release Android build. Use when packaging, before a release, or when asked to check Release or Consumer. Everyday checks use Debug; do not suggest these builds unprompted, as they are slow.
---

# The Consumer configuration and trimming

Day-to-day checks run in Debug. Release and Consumer builds take a long time, so they belong to the
run-up to packaging, or to a direct request.

## Consumer

`Consumer` builds the sample against the published packages instead of the projects beside it, so
that what the UI tests drive is what someone installing from NuGet would get. It is Release with a
different reference, and the one thing it proves that Release cannot is that the package itself is
right -- its layout, its dependencies, its targets.

```powershell
dotnet pack  SumireCraft.Maui.slnx -c Release -o artifacts
dotnet build samples/SumireCraft.Maui.Sample -c Consumer -f net10.0-windows10.0.19041.0
dotnet test  --project tests/SumireCraft.Maui.UITests/SumireCraft.Maui.UITests.csproj
```

Running the UI tests is the `ui-tests` skill. They drive the newest of `bin/Debug` and
`bin/Consumer`, so a Debug build made after the Consumer one is what gets tested.

Pack first: the package has to exist before it can be found. Packing again under the same version
is not enough: restore takes the copy already in the global packages folder, so delete
`~/.nuget/packages/sumirecraft.maui.*/<version>` before building Consumer again. The extra restore
source is set in the sample's own project file rather than in `nuget.config`, because `artifacts/`
is not in the repository and a source pointing at a folder nobody has is a restore error for
everybody.

The SDK and the platform SDKs decide what Release does by the configuration's name, so left to them
a Consumer build would not be optimized, and an Android one would be neither trimmed nor compiled
ahead of time. The root `Directory.Build.props` spells out `Optimize`, and
`samples/Directory.Build.props` spells out what the platform SDKs set for Release, saying what it
leaves out and why. Android and Apple were checked against Release with `dotnet msbuild
-getProperty`; after a .NET upgrade, check again.

## What trimming does to a change

The libraries are `IsAotCompatible`, and the trim analyzer catches most of what trimming would
break at build time. To see it at run time, build the sample for Android in Release, which trims,
name the APK in `SUMIRE_SAMPLE_ANDROID` (the tests ignore `bin/Release` otherwise) and run the UI
tests against it.

## When every Android test fails at once

If the emulator shows "SumireCraft.Maui.Sample keeps stopping", read the crash before anything
else: `adb logcat -d -b crash`.

Seen once, with .NET 10: a Release build for Android, made over the outputs an earlier Release
build had left, aborted at start with `Invalid compressed assembly descriptor index` from
`libmonodroid.so`, and every Android test failed. Deleting the sample's
`obj/Release/net10.0-android` and `bin/Release/net10.0-android` and building again fixed it. The
likely cause, not confirmed in .NET for Android's documentation, is that the incremental build
left the app's list of assemblies out of step with the assemblies packed in it. A Consumer build
packs the same way and could do the same; delete `obj/Consumer/net10.0-android` and
`bin/Consumer/net10.0-android` before building it, just as the steps above clear the package
cache.

If the app still aborts after building from clean, this is not the cause: look at what changed in
the library, trimming above all.
