using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.Gates;

/// <summary>
/// Puts a <see cref="BusyWatchdog"/> on the application's <see cref="IBusyGate"/> once the
/// container that holds the gate exists.
/// </summary>
/// <param name="timeout">The timeout the application chose in <see cref="SumireCraftOptions.BusyTimeout"/>.</param>
/// <remarks>
/// Registered by <c>UseSumireCraftCore()</c> only when a timeout is set. The watchdog lives as long
/// as the container does: disposing the container disposes this, which stops the watchdog.
/// </remarks>
internal sealed class BusyWatchdogInstaller(TimeSpan timeout) : IMauiInitializeService, IDisposable
{
    private BusyWatchdog? _watchdog;

    /// <inheritdoc />
    /// <remarks>
    /// Uses the application's <see cref="TimeProvider"/> when it has registered one, and the
    /// system clock otherwise.
    /// </remarks>
    public void Initialize(IServiceProvider services)
    {
        var watchdog = new BusyWatchdog(
            services.GetRequiredService<IBusyGate>(),
            timeout,
            services.GetRequiredService<IExceptionSink>(),
            services.GetService<TimeProvider>());

        // .NET MAUI initializes each service once, but a second watchdog on the same gate would
        // report every timeout twice, so a repeated call keeps the first and discards the new one.
        if (Interlocked.CompareExchange(ref _watchdog, watchdog, null) is not null)
        {
            watchdog.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose() => Interlocked.Exchange(ref _watchdog, null)?.Dispose();
}
