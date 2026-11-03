using Microsoft.Extensions.DependencyInjection.Extensions;
using SumireCraft.Maui.Exceptions;
using SumireCraft.Maui.Gates;

namespace SumireCraft.Maui;

/// <summary>
/// Extension methods that register SumireCraft.Maui.Core with a <see cref="MauiAppBuilder"/>.
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Initializes the shared services of SumireCraft.Maui.Core.
    /// </summary>
    /// <param name="builder">The <see cref="MauiAppBuilder"/> to initialize.</param>
    /// <param name="configure">
    /// Changes the defaults before anything is registered. Applications with no opinion pass
    /// nothing.
    /// </param>
    /// <returns>The same <see cref="MauiAppBuilder"/> instance, so calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// When using SumireCraft.Maui.Controls, call <c>UseSumireCraft()</c> instead. It calls this
    /// method internally, so there is no need to call both.
    /// </para>
    /// <para>
    /// Registers <see cref="IBusyGate"/> and <see cref="IExceptionSink"/> as singletons. An
    /// application that has already registered an implementation of its own keeps it.
    /// </para>
    /// <para>
    /// The sink is given every <see cref="IExceptionHandler"/> the application has registered.
    /// Registering none is fine: reporting to a sink with no handlers does nothing, so the rest of
    /// the library can report unconditionally.
    /// </para>
    /// <para>
    /// When <see cref="SumireCraftOptions.BusyTimeout"/> is set, a <see cref="BusyWatchdog"/> is put
    /// on the registered <see cref="IBusyGate"/> during <c>MauiAppBuilder.Build()</c>.
    /// </para>
    /// <para>
    /// Only the first call on a builder counts. A later one with no <paramref name="configure"/>
    /// returns at once, so calling both this and <c>UseSumireCraft()</c> -- or this from an
    /// extension of the application's own as well -- registers everything once rather than
    /// reporting every failure twice. A later one that passes <paramref name="configure"/> throws,
    /// because its options could only be dropped.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="SumireCraftOptions.BusyTimeout"/> is set to zero or a negative value.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="configure"/> is passed, and this method or <c>UseSumireCraft()</c> has
    /// already been called on <paramref name="builder"/>.
    /// </exception>
    public static MauiAppBuilder UseSumireCraftCore(
        this MauiAppBuilder builder,
        Action<SumireCraftOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Looked for in the builder's own services rather than kept in a static field, so that a
        // second builder in the same process -- a test, say -- is not taken for a second call.
        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(Registered)))
        {
            // Options asked for here could only be dropped, and an application started without what
            // it asked for is worse than one that does not start.
            if (configure is not null)
            {
                throw new InvalidOperationException(
                    "UseSumireCraft() or UseSumireCraftCore() has already been called on this builder, "
                    + "and options are taken from the first call only. Pass every option to that call.");
            }

            return builder;
        }

        builder.Services.AddSingleton<Registered>();

        var options = new SumireCraftOptions();
        configure?.Invoke(options);

        if (options.BusyTimeout is { } busyTimeout)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(busyTimeout, TimeSpan.Zero, "options.BusyTimeout");
        }

        builder.Services.TryAddSingleton<IBusyGate, BusyGate>();
        builder.Services.TryAddSingleton<IExceptionSink, ExceptionSink>();
        builder.Services.AddSingleton<IMauiInitializeService, ExceptionSinkBridge>();

        if (options.InstallSafetyNet)
        {
            builder.Services.AddSingleton<IMauiInitializeService, SafetyNet>();
        }

        if (options.BusyTimeout is { } timeout)
        {
            // Registered through a factory rather than as an instance, because the container
            // disposes only what it created, and disposing the installer is what stops the watchdog.
            builder.Services.AddSingleton<IMauiInitializeService>(_ => new BusyWatchdogInstaller(timeout));
        }

        return builder;
    }

    /// <summary>
    /// Marks a builder that <see cref="UseSumireCraftCore"/> has already been called on.
    /// </summary>
    private sealed class Registered;
}
