---
name: readme-images
description: Taking the pictures in README.md again -- assets/hero.gif, assets/badges.png and assets/taps.gif -- from the sample's Showcase page on the Android emulator. Use when a widget's look or behaviour changes, when the Showcase page changes, or when the maintainer asks for new pictures.
---

# README pictures

The pictures in `README.md` live in `assets/` and come from one page, the sample's
`Pages/ShowcasePage`, on the Android emulator. All of them are Android: the maintainer chose one
platform so that the README looks of a piece. `shoot.ps1` beside this file takes all three in one
run.

| File | Where in README.md | What it shows |
| --- | --- | --- |
| `assets/hero.gif` | under the title | a badge counting up to `99+`, then one tap and a burst of five |
| `assets/badges.png` | the start of *SmrBadgeView* | a number, `99+` and a dot, then the same row right to left |
| `assets/taps.gif` | the start of *SmrTapBehavior* | one tap opening the inbox, five taps opening the debug room |

The page has a section per picture, and every section and control carries an `AutomationId`
starting with `Showcase`. The script finds them on screen by that name, so the layout can change
without touching the script; a new picture means a new section with its own `AutomationId`.

## Steps

1. Start the emulator (the AVD in use is a Pixel 7, 1080x2400) in a command of its own, and let
   it settle: for a few minutes after booting its load is 15 or more, and the recorder drops
   frames. The script refuses to shoot above a load of 4 (`adb shell cat /proc/loadavg`). ffmpeg
   must be installed (`winget install Gyan.FFmpeg`); `ANDROID_HOME` must be set, as the UI tests
   need it too.
2. Build and start the sample on it, then open **Showcase** from the menu:

   ```powershell
   dotnet build samples/SumireCraft.Maui.Sample -f net10.0-android -p:EmbedAssembliesIntoApk=true -t:Run
   ```

   `-t:Run` sometimes stays running after the application is up. Stop it then; the application
   stays. Stopping it takes down anything started in the same command, the emulator included,
   which is why the emulator gets a command of its own. An application already installed starts
   with `adb shell monkey -p com.companyname.sumirecraft.maui.sample -c android.intent.category.LAUNCHER 1`.

3. Shoot, into a folder under `%TEMP%`, and look at the result before anything else. The script
   stops without copying anything if the burst of five did not end on "Debug room" or a clip came
   out too short; it cannot see everything, so look anyway:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/skills/readme-images/shoot.ps1
   ```

   To look at a GIF without a viewer, cut it into a contact sheet and read that:
   `ffmpeg -i hero.gif -vf "fps=1.5,scale=200:-1,tile=8x2" -frames:v 1 sheet.png`.
4. Show the maintainer, then run it again with `-Copy` to put the files in `assets/`. The README
   already points at them, so nothing else changes unless a picture is added or removed.

## Found by trying

- **`adb shell input tap` does not show touches.** "Show taps" draws only what comes from the touch
  screen itself, and `input` injects past that point. Writing to `/dev/input` with `sendevent` is
  refused by SELinux, and a Play Store image cannot `adb root`. The emulator console can:
  `adb emu event mouse <x> <y> 0 1` presses on the touch screen and `... 0 0` lets go, in screen
  pixels. A press is held 60 ms so the dot shows.
- **The console is fast enough for a burst, with room to spare only if kept tight.** One `adb emu`
  call takes about 30 ms, so a tap with its 60 ms hold and 40 ms after it comes about every 160 ms,
  inside the default interval of 300 ms. The page keeps the default, and the GIFs are not sped up.
  On a loaded machine adb starts slower and the gaps grow; a gap past 300 ms splits the burst and
  the inbox opens, so check that the burst still ends on "Debug room" with the inbox count unchanged.
- **`screenrecord` writes a frame only when the screen changes.** A clip ends on the last change, so
  the script holds the last screen for a moment. It waits 1.5 s before the first touch, part of it
  before the recorder is running, and cuts 0.5 s, leaving a still moment at the start.
- **Sections are found by `uiautomator dump`.** An `AutomationId` lands in `resource-id`, layouts
  included, with its bounds in screen pixels; the crops are taken from those.
