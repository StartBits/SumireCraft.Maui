# Takes the pictures in README.md from the sample's Showcase page on a running Android emulator:
# assets/hero.gif, assets/badges.png and assets/taps.gif. Everything is found by AutomationId, so a
# change to the page's layout needs no change here. See SKILL.md beside this file.
#
#   powershell -File .claude/skills/readme-images/shoot.ps1            # into a folder under %TEMP%
#   powershell -File .claude/skills/readme-images/shoot.ps1 -Copy      # and then into assets/

param(
    [string] $OutDir = (Join-Path $env:TEMP 'sumire-readme-images'),
    [switch] $Copy
)

$ErrorActionPreference = 'Stop'

if (-not $env:ANDROID_HOME) {
    throw 'ANDROID_HOME is not set. It names the Android SDK, as the UI tests need it to.'
}
$adb = Join-Path $env:ANDROID_HOME 'platform-tools\adb.exe'
$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
if (-not $ffmpeg) {
    # winget puts it here, and a shell started before the install does not have it on PATH yet.
    $ffmpeg = Join-Path $env:LOCALAPPDATA 'Microsoft\WinGet\Links\ffmpeg.exe'
}
$ffprobe = Join-Path (Split-Path $ffmpeg) 'ffprobe.exe'
foreach ($tool in $adb, $ffmpeg, $ffprobe) {
    if (-not (Test-Path $tool)) {
        throw "Not found: $tool"
    }
}
New-Item -ItemType Directory -Force $OutDir | Out-Null

# The bounds of every element on screen whose AutomationId starts with Showcase, in screen pixels.
function Get-Showcase {
    & $adb shell uiautomator dump /sdcard/sumire-ui.xml | Out-Null
    $xml = (& $adb shell cat /sdcard/sumire-ui.xml) -join ''
    $found = @{}
    foreach ($m in [regex]::Matches($xml, 'resource-id="[^"]*?(Showcase\w+)"[^>]*?bounds="\[(\d+),(\d+)\]\[(\d+),(\d+)\]"')) {
        $found[$m.Groups[1].Value] = [pscustomobject]@{
            X1 = [int]$m.Groups[2].Value; Y1 = [int]$m.Groups[3].Value
            X2 = [int]$m.Groups[4].Value; Y2 = [int]$m.Groups[5].Value
        }
    }
    return $found
}

# What an element on screen says, found by its AutomationId.
function Get-Text([string] $id) {
    & $adb shell uiautomator dump /sdcard/sumire-ui.xml | Out-Null
    $xml = (& $adb shell cat /sdcard/sumire-ui.xml) -join ''
    return [regex]::Match($xml, "text=`"([^`"]*)`"\s+resource-id=`"[^`"]*?$id`"").Groups[1].Value
}

function Get-Duration([string] $clip) {
    return [double](& $ffprobe -v error -show_entries format=duration -of csv=p=0 (Join-Path $OutDir "$clip.mp4"))
}

# Touches go through the emulator console, so that they arrive as the touch screen's own and "show
# taps" draws them. `adb shell input` injects them past the point where the dots are drawn. Each
# console call takes about 30 ms.
function Press($element, [int] $holdMs = 60) {
    $x = [int](($element.X1 + $element.X2) / 2)
    $y = [int](($element.Y1 + $element.Y2) / 2)
    & $adb emu event mouse $x $y 0 1 | Out-Null
    Start-Sleep -Milliseconds $holdMs
    & $adb emu event mouse $x $y 0 0 | Out-Null
}

function Record([string] $name, [scriptblock] $actions) {
    & $adb shell rm -f "/sdcard/$name.mp4"
    $recorder = Start-Process -FilePath $adb -ArgumentList 'shell', 'screenrecord', '--bit-rate', '8000000', "/sdcard/$name.mp4" -PassThru -WindowStyle Hidden
    Start-Sleep -Milliseconds 1500
    & $actions
    # Long enough for a burst to be read (the default interval is 300 ms); the GIF holds the last
    # screen from there.
    Start-Sleep -Milliseconds 600
    & $adb shell pkill -INT screenrecord
    $recorder.WaitForExit(15000) | Out-Null
    Start-Sleep -Seconds 1
    & $adb pull "/sdcard/$name.mp4" (Join-Path $OutDir "$name.mp4") | Out-Null
}

# The screen rows from the top of one element to the bottom of another, across the whole width.
function Rows($top, $bottom, [int] $margin = 24) {
    $y = $top.Y1 - $margin
    return "iw:$($bottom.Y2 - $top.Y1 + 2 * $margin):0:$y"
}

# Plays the clips one after the other as a GIF 480 pixels wide. screenrecord writes a frame only
# when the screen changes, so each clip's last screen is held for $hold seconds. Record waits 1.5 s
# before the first touch, part of which passes before the recorder is running; 0.5 s is cut from the
# start, which leaves the screen as it was for a moment before anything is touched.
function ConvertTo-Gif([string] $out, [string[]] $clips, [string] $crop, [double] $hold) {
    $inputs = @()
    $parts = @()
    $labels = ''
    for ($i = 0; $i -lt $clips.Count; $i++) {
        $inputs += '-i', (Join-Path $OutDir "$($clips[$i]).mp4")
        $parts += "[$i]fps=15,trim=start=0.5,setpts=PTS-STARTPTS,crop=$crop,tpad=stop_mode=clone:stop_duration=$hold[c$i]"
        $labels += "[c$i]"
    }
    $graph = ($parts -join ';') + ";$($labels)concat=n=$($clips.Count):v=1,scale=480:-1:flags=lanczos,split[x][y];" +
        '[x]palettegen=max_colors=128:stats_mode=diff[p];[y][p]paletteuse=dither=bayer:bayer_scale=5:diff_mode=rectangle'
    & $ffmpeg -v error -y @inputs -filter_complex $graph -loop 0 (Join-Path $OutDir $out)
}

$ui = Get-Showcase
foreach ($id in 'ShowcaseBadges', 'ShowcaseCounter', 'ShowcaseAddOne', 'ShowcaseAddMany', 'ShowcaseTaps', 'ShowcaseTapTarget', 'ShowcaseOpened', 'ShowcaseReset') {
    if (-not $ui.ContainsKey($id)) {
        throw "$id is not on screen. Open the Showcase page in the sample."
    }
}

# A freshly booted emulator runs at a load of 15 or more for a few minutes, and screenrecord then
# starts late and drops frames: the badge jumps straight to 99+ and the taps go unseen.
$load = [double]((& $adb shell cat /proc/loadavg) -split ' ')[0]
if ($load -gt 4) {
    throw "The emulator is still busy (load $load). Wait until it settles below 4 and run again."
}

$showTouches = (& $adb shell settings get system show_touches).Trim()
& $adb shell settings put system show_touches 1
try {
    Press $ui.ShowcaseReset
    Start-Sleep -Milliseconds 500
    # Otherwise a "Debug room" left from an earlier run would pass the check after recording.
    $start = Get-Text 'ShowcaseOpened'
    if ($start -in 'Inbox', 'Debug room') {
        throw "Reset did not take: the page still reads '$start'. Nothing was recorded."
    }

    Record 'counter' {
        foreach ($i in 1..2) { Press $ui.ShowcaseAddOne; Start-Sleep -Milliseconds 600 }
        foreach ($i in 1..4) { Press $ui.ShowcaseAddMany; Start-Sleep -Milliseconds 600 }
    }

    # One tap, a pause long enough to read the result, then five taps about 160 ms apart (two
    # console calls, the 60 ms hold and 40 ms between). The page keeps the default interval of
    # 300 ms, and a gap that grew past it would split the burst and open the inbox: the opposite of
    # what the picture is for, so look at the result.
    Record 'taps' {
        Press $ui.ShowcaseTapTarget
        Start-Sleep -Milliseconds 1800
        foreach ($i in 1..5) { Press $ui.ShowcaseTapTarget; Start-Sleep -Milliseconds 40 }
    }
}
finally {
    # As it was before, rather than off: some keep it on. An emulator where it was never set
    # answers "null", which means off.
    if ($showTouches -notin '0', '1') {
        $showTouches = '0'
    }
    & $adb shell settings put system show_touches $showTouches
}

# What the pictures are for has to have happened, and been recorded. The burst ends on the debug
# room only if its five taps stayed one burst; a short clip means the recorder missed part of it.
$opened = Get-Text 'ShowcaseOpened'
if ($opened -ne 'Debug room') {
    throw "The burst of five ended on '$opened', not 'Debug room': it was split. Nothing was copied."
}
foreach ($clip in @{ counter = 3.5; taps = 2.5 }.GetEnumerator()) {
    $seconds = Get-Duration $clip.Key
    if ($seconds -lt $clip.Value) {
        throw "$($clip.Key).mp4 is $seconds s, shorter than $($clip.Value) s: the recorder missed the start. Nothing was copied."
    }
}

$screen = Join-Path $OutDir 'screen.png'
cmd /c "`"$adb`" exec-out screencap -p > `"$screen`""
& $ffmpeg -v error -y -i $screen -vf "crop=$(Rows $ui.ShowcaseBadges $ui.ShowcaseBadges),scale=720:-1:flags=lanczos" (Join-Path $OutDir 'badges.png')

ConvertTo-Gif 'hero.gif' @('counter', 'taps') (Rows $ui.ShowcaseCounter $ui.ShowcaseTaps) 1.5
ConvertTo-Gif 'taps.gif' @('taps') (Rows $ui.ShowcaseTaps $ui.ShowcaseTaps) 2

$pictures = 'hero.gif', 'badges.png', 'taps.gif'
foreach ($name in $pictures) {
    $file = Get-Item (Join-Path $OutDir $name)
    '{0,-11} {1,9:N0} bytes  {2}' -f $file.Name, $file.Length, $file.FullName
}

if ($Copy) {
    $assets = Join-Path $PSScriptRoot '..\..\..\assets'
    New-Item -ItemType Directory -Force $assets | Out-Null
    foreach ($name in $pictures) {
        Copy-Item (Join-Path $OutDir $name) (Join-Path $assets $name) -Force
    }
    "Copied into $((Resolve-Path $assets).Path)"
}
