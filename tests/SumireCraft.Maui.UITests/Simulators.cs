using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace SumireCraft.Maui.UITests;

/// <summary>
/// Asks Xcode which iPhone simulators are running.
/// </summary>
internal static class Simulators
{
    /// <summary>
    /// The identifier of the first running iPhone simulator, or <see langword="null"/> when none is
    /// running or there is no Xcode to ask.
    /// </summary>
    /// <remarks>
    /// An iPhone and nothing else. On an iPad (iOS 27.0) the driver reported the keyboard's done
    /// button well below where it was drawn, and pressing it left the keyboard up; in another run the
    /// keyboard had become a floating one over the page. The library does nothing an iPad would do
    /// differently, so the iPad is left out rather than worked around.
    /// </remarks>
    public static string? FirstBooted()
    {
        string listing;

        try
        {
            using var simctl = Process.Start(new ProcessStartInfo("xcrun", "simctl list devices booted --json")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });

            if (simctl is null)
            {
                return null;
            }

            listing = simctl.StandardOutput.ReadToEnd();
            simctl.WaitForExit();

            // The command line tools alone have xcrun but no simctl, and say so on standard error.
            if (simctl.ExitCode != 0)
            {
                return null;
            }
        }
        catch (Win32Exception)
        {
            // No xcrun: not a Mac.
            return null;
        }

        try
        {
            using var json = JsonDocument.Parse(listing);

            // Runtimes are keyed like com.apple.CoreSimulator.SimRuntime.iOS-26-5; watchOS and tvOS
            // simulators are listed beside them and cannot run the sample.
            return json.RootElement.GetProperty("devices").EnumerateObject()
                .Where(runtime => runtime.Name.Contains(".iOS-", StringComparison.Ordinal))
                .SelectMany(runtime => runtime.Value.EnumerateArray())
                .Where(device => device.GetProperty("deviceTypeIdentifier").GetString()?
                    .Contains(".iPhone-", StringComparison.Ordinal) == true)
                .Select(device => device.GetProperty("udid").GetString())
                .FirstOrDefault();
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            // Not the listing this was written against. Nothing can be driven without knowing which
            // simulator to drive, and the skip says what to look at.
            return null;
        }
    }
}
