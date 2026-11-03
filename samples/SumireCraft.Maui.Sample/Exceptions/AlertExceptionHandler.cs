using Microsoft.Extensions.Logging;
using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.Sample.Exceptions;

/// <summary>
/// Tells the user that something they asked for has failed.
/// </summary>
/// <param name="logger">Where to note an alert that could not be shown.</param>
/// <remarks>
/// <para>
/// A sample rather than part of the library, because no two applications agree on what an alert
/// should say: the title, the wording, the buttons and the languages are all the application's own.
/// What is worth copying is the handful of decisions below, each of which is easy to get wrong.
/// </para>
/// <para>
/// <b>Only <see cref="ExceptionOrigin.Command"/>.</b> That is the one origin with somebody in front
/// of the screen waiting for an answer. An alert about background work comes out of nowhere and
/// interrupts whatever the user moved on to, and an <see cref="ExceptionOrigin.Unhandled"/>
/// exception means the process is on its way down, with no time left to show anything.
/// </para>
/// <para>
/// <b>Nothing from the exception reaches the screen.</b> Exception messages are written for
/// developers, in one language, and sometimes say more about the application's insides than a user
/// should see. The details belong in the log, which is why <see cref="LoggingExceptionHandler"/>
/// is registered ahead of this one.
/// </para>
/// <para>
/// <b>One alert at a time.</b> Commands can fail in quick succession, WinUI refuses to open a
/// second dialog while one is up, and a stack of identical alerts tells the user nothing the first
/// one did not. Reports that arrive while an alert is showing are left to the log.
/// </para>
/// <para>
/// <b>No page is held.</b> The handler lives as long as the application, so it looks the page up
/// each time it needs one instead of keeping it, which would keep that page alive for good.
/// </para>
/// </remarks>
public sealed partial class AlertExceptionHandler(ILogger<AlertExceptionHandler> logger) : IExceptionHandler
{
    private const string Title = "Something went wrong";
    private const string Message = "That did not finish. Please try again.";
    private const string Dismiss = "Got it";

    private readonly ILogger<AlertExceptionHandler> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private int _showing;

    /// <inheritdoc />
    public void Handle(ExceptionReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.Origin != ExceptionOrigin.Command)
        {
            return;
        }

        if (Interlocked.Exchange(ref _showing, 1) == 1)
        {
            return;
        }

        // Handlers run on whichever thread reported, and the sink promises never to hold the
        // caller up waiting for the user. So the alert is handed to the UI thread and not awaited.
        MainThread.BeginInvokeOnMainThread(() => _ = ShowAsync());
    }

    private async Task ShowAsync()
    {
        try
        {
            // The first window is right for an application that has only one. One with several
            // would look for the window the user is working in.
            if (Application.Current?.Windows is [var window, ..] && window.Page is { } page)
            {
                await page.DisplayAlertAsync(Title, Message, Dismiss);
            }
        }
        catch (Exception exception)
        {
            // Logged rather than reported. Reporting would send it back through the sink to this
            // same handler, which would try to show another alert about the alert.
            LogAlertFailed(_logger, exception);
        }
        finally
        {
            Volatile.Write(ref _showing, 0);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "An alert about a failed command could not be shown.")]
    private static partial void LogAlertFailed(ILogger logger, Exception exception);
}
