# Checks the version numbers in build/Version.props against the tags, before anything is published.
#
# ci.yml runs it on a pull request into main, with -Base, and release.yml runs it before publishing,
# without. Merging into main publishes whatever changed in a package, so the pull request is where a
# version that was not raised is caught, and the release checks again in case it was not.
#
# Written for both Windows PowerShell 5.1 and PowerShell 7, so that it can be tried on a machine
# without the latter: hence no [semver], which 5.1 lacks.
param(
    # The commit a pull request is compared with. Without it, every check applies.
    [string] $Base = ''
)

$ErrorActionPreference = 'Stop'

# What goes into a package. Also written as the paths filter in release.yml, and in needs-build.ps1,
# which always builds them because ci.yml skips this check along with the build; keep the three in
# step. Cautious on purpose: Directory.Packages.props and global.json count even when only a test or
# the sample needed the change.
$packagePaths = @('src', 'build', 'global.json', ':(glob)Directory.*.props')

# What of that goes into Core: all but Controls, and but the numbers themselves, which change in every
# release without Core changing. Everything else under src/ counts as Core, so a package added there
# is excluded here as Controls is, or every change to it asks Core for a new number.
# SumireCraft.Core, the package that depends on no UI framework, is such a package: "Core" in this
# file is SumireCraft.Maui.Core alone.
$corePaths = $packagePaths + @(
    ':(exclude)src/SumireCraft.Maui.Controls',
    ':(exclude)src/SumireCraft.Core',
    ':(exclude)build/Version.props')

# What goes into SumireCraft.Core, by the same reasoning: all but the other two packages and the
# numbers. It is numbered as Core is, on its own.
$sharedPaths = $packagePaths + @(
    ':(exclude)src/SumireCraft.Maui.Controls',
    ':(exclude)src/SumireCraft.Maui.Core',
    ':(exclude)build/Version.props')

function Fail([string] $message) {
    Write-Host "::error::$message"
    exit 1
}

# A bare -- is taken by PowerShell as the end of a function's parameters and never reaches $args,
# so a caller that means git's -- passes it quoted: '--'.
function Invoke-Git {
    $output = & git @args
    if ($LASTEXITCODE -ne 0) {
        Fail "git $args failed."
    }
    $output
}

# Compares two versions of the form 1.2.3 or 1.2.3-suffix. A prerelease comes before its release;
# two suffixes are compared as text, which is all this repository has needed.
function Compare-Version([string] $a, [string] $b) {
    $partsA = $a.Split('-', 2)
    $partsB = $b.Split('-', 2)
    $order = ([version]$partsA[0]).CompareTo([version]$partsB[0])
    if ($order -ne 0) { return $order }
    if ($partsA.Count -eq 1 -and $partsB.Count -eq 1) { return 0 }
    if ($partsA.Count -eq 1) { return 1 }
    if ($partsB.Count -eq 1) { return -1 }
    return [string]::CompareOrdinal($partsA[1], $partsB[1])
}

function Get-Version([string] $project) {
    $version = (& dotnet msbuild $project -getProperty:Version | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or -not $version) {
        Fail "Could not read the version of $project."
    }
    $version
}

$controls = Get-Version 'src/SumireCraft.Maui.Controls/SumireCraft.Maui.Controls.csproj'
$core = Get-Version 'src/SumireCraft.Maui.Core/SumireCraft.Maui.Core.csproj'
$shared = Get-Version 'src/SumireCraft.Core/SumireCraft.Core.csproj'
Write-Host "Controls $controls, Core $core, SumireCraft.Core $shared"

# For the steps after this one in release.yml. They run only if every check below passes.
if ($env:GITHUB_OUTPUT) {
    "controls=$controls" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "core=$core" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "publish-core=$(($core -eq $controls).ToString().ToLowerInvariant())" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "shared=$shared" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "publish-shared=$(($shared -eq $controls).ToString().ToLowerInvariant())" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
}

if ($Base) {
    $touched = Invoke-Git diff --name-only $Base HEAD '--' @packagePaths
    if (-not $touched) {
        Write-Host 'Nothing that goes into a package has changed, so no version is needed.'
        exit 0
    }
}

$released = @(Invoke-Git tag --list 'v*' |
    Where-Object { $_ -match '^v\d+\.\d+\.\d+(-.+)?$' } |
    ForEach-Object { $_.Substring(1) })

if ($released -contains $controls) {
    Fail "v$controls has already been released. Raise SumireVersion in build/Version.props."
}

$latest = $null
foreach ($version in $released) {
    if (-not $latest -or (Compare-Version $version $latest) -gt 0) {
        $latest = $version
    }
}

if ($latest -and (Compare-Version $controls $latest) -le 0) {
    Fail "The release version $controls is not above the latest release, $latest."
}

# Each of Core and SumireCraft.Core either takes this release's number or keeps the number of an
# earlier release, in which case nothing that goes into it may have changed since that release's
# tag. A tag from before a package existed shows the whole package as changed, so a package cannot
# keep a number it was never released under.
function Test-Kept([string] $name, [string] $version, [string] $property, [string[]] $paths) {
    if ($version -eq $controls) {
        return
    }
    if ((Compare-Version $version $controls) -gt 0) {
        Fail "$name $version is ahead of the release version $controls."
    }
    if ($released -notcontains $version) {
        Fail "$name stays at $version, but there is no tag v$version to compare it against."
    }
    $changed = Invoke-Git diff --name-only "v$version" HEAD '--' @paths
    if ($changed) {
        Write-Host "::error::$name has changed since v$version, so it needs this release's number ($controls) as $property in build/Version.props:"
        $changed | ForEach-Object { Write-Host "  $_" }
        exit 1
    }
}

Test-Kept 'Core' $core 'SumireCoreVersion' $corePaths
Test-Kept 'SumireCraft.Core' $shared 'SumireCraftCoreVersion' $sharedPaths

if ($latest) {
    Write-Host "Versions are in order: $controls follows $latest."
} else {
    Write-Host "Versions are in order: $controls is the first release."
}
