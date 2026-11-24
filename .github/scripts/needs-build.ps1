# Says whether a change needs the build, the tests and the packages, for ci.yml.
#
# A change that touches nothing a build reads -- documentation, Claude Code's own files, the README's
# pictures, GitHub's housekeeping -- has nothing to build, and skipping it saves the workloads and the
# build, most of the run. The list below names what can be skipped rather than what must be built,
# so that anything it does not name, a path added tomorrow included, is built.
#
# A package path (check-versions.ps1) is always built, whatever the list says, so a change skipped
# here raises no version and the version check is skipped with it.
#
# Written for both Windows PowerShell 5.1 and PowerShell 7, as check-versions.ps1 is.
param(
    # The commit the change is compared with. Without it, everything is built.
    [string] $Base = ''
)

$ErrorActionPreference = 'Stop'

# Regular expressions over a path as git prints it, from the root of the repository.

# The package paths, also written in check-versions.ps1 and as release.yml's paths filter; keep the
# three in step.
$packagePaths = @(
    '^src/',
    '^build/',
    '^global\.json$',
    '^Directory\.[^/]*\.props$'
)

$skippable = @(
    '^[^/]*\.md$',                    # the README, the changelog, CLAUDE.md and the like
    '^\.github/.*\.md$',
    '^\.claude/',                     # Claude Code's settings, hooks, skills and agents
    '^\.vscode/',
    '^assets/',                       # the README's pictures; the package icon is in build/
    '^LICENSE$',                      # the packages name their licence by expression
    '^\.github/ISSUE_TEMPLATE/',
    '^\.github/CODEOWNERS$',
    '^\.github/dependabot\.yml$'
)

function Test-Match([string] $path, [string[]] $patterns) {
    [bool]($patterns | Where-Object { $path -match $_ })
}

function Write-Decision([bool] $build) {
    if ($env:GITHUB_OUTPUT) {
        "build=$($build.ToString().ToLowerInvariant())" |
            Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    }
}

if (-not $Base) {
    Write-Host 'No base to compare with, so everything is built.'
    Write-Decision $true
    exit 0
}

# --no-renames lists a moved file at both its paths: by default git names only where it went, and
# a file moved out of src/ into a skipped folder would be skipped. --end-of-options keeps a base
# typed by hand from being read as an option.
$changed = @(& git diff --name-only --no-renames --end-of-options $Base HEAD)
if ($LASTEXITCODE -ne 0) {
    Write-Host "::error::git diff against $Base failed."
    exit 1
}

$built = @($changed | Where-Object {
    (Test-Match $_ $packagePaths) -or -not (Test-Match $_ $skippable)
})

if ($built.Count -gt 0) {
    Write-Host 'These need the build:'
    $built | ForEach-Object { Write-Host "  $_" }
    Write-Decision $true
} else {
    Write-Host "Nothing a build reads has changed (files changed: $($changed.Count)), so the build is skipped."
    Write-Decision $false
}
