using Microsoft.Extensions.Logging;
using SumireCraft.Maui.Controls;
using SumireCraft.Maui.Exceptions;
using SumireCraft.Maui.Sample.Exceptions;
using SumireCraft.Maui.Sample.Pages;

namespace SumireCraft.Maui.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // The safety net is off unless asked for. The sample asks, so that what nothing caught still
        // reaches the log on the way down.
        builder
            .UseMauiApp<App>()
            .UseSumireCraft(options => options.InstallSafetyNet = true)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Handlers run in the order they are registered. The log goes first, so that a failure is
        // on record whatever the alert does after it.
        builder.Services.AddSingleton<IExceptionHandler, LoggingExceptionHandler>();
        builder.Services.AddSingleton<IExceptionHandler, AlertExceptionHandler>();

        builder.Services.AddTransient<ExceptionPage>();
        builder.Services.AddTransient<ExceptionPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
