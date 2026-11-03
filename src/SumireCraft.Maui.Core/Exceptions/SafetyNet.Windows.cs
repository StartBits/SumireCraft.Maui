namespace SumireCraft.Maui.Exceptions;

// The Windows half of the net.
//
// Windows is the one platform that needs a second hook. AppDomain.UnhandledException sees
// exceptions from background threads, but one thrown on the UI thread reaches the WinUI
// application instead, and listening to only one of the two loses half of an application's
// failures.
//
// e.Handled is deliberately left alone. WinUI is the only place where setting it would keep the
// application alive, and doing so would make the net change behaviour rather than record it.
internal sealed partial class SafetyNet
{
    partial void InstallPlatformHooks()
    {
        if (Microsoft.UI.Xaml.Application.Current is not { } application)
        {
            return;
        }

        application.UnhandledException += (_, e) =>
            ReportSafely(UnhandledExceptionTranslator.FromPlatformApplication(e.Exception));
    }
}
