using Microsoft.Extensions.Logging;
using SumireCraft.Maui.Controls;
using SumireCraft.Maui.Exceptions;
using SumireCraft.Maui.Gates;
using SumireCraft.Maui.Sample.Exceptions;
using SumireCraft.Maui.Sample.Inputs;
using SumireCraft.Maui.Sample.Pages;

namespace SumireCraft.Maui.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // The safety net is off unless asked for. The sample asks, so that what nothing caught still
        // reaches the log on the way down. The busy timeout is off unless asked for too; the sample's
        // five seconds is short enough for the BusyGate page to show the watchdog at work, and
        // longer than anything the sample keeps its gate busy for on purpose.
        builder
            .UseMauiApp<App>()
            .UseSumireCraft(options =>
            {
                options.InstallSafetyNet = true;
                options.BusyTimeout = TimeSpan.FromSeconds(5);
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // The Inputs page's indented entries, whose room the application makes through a mapper.
        IndentedEntry.AddMapping();

        // Handlers run in the order they are registered. The log goes first, so that a failure is
        // on record whatever the alert does after it.
        builder.Services.AddSingleton<IExceptionHandler, LoggingExceptionHandler>();
        builder.Services.AddSingleton<IExceptionHandler, AlertExceptionHandler>();

        builder.Services.AddTransient<ExceptionPage>();
        builder.Services.AddTransient<ExceptionPageViewModel>();

        // A gate of the uploads' own, under a key, so that an upload holds back nothing that asks the
        // application's gate. It has no watchdog: an upload is allowed to take as long as it takes.
        builder.Services.AddKeyedSingleton<IBusyGate>(GatePageViewModel.UploadsKey, (_, _) => new BusyGate());
        builder.Services.AddTransient<GatePage>();
        builder.Services.AddTransient<GatePageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
