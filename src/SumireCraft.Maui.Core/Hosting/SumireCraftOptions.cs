namespace SumireCraft.Maui;

/// <summary>
/// The choices an application can make when it initializes SumireCraft.Maui.Core.
/// </summary>
/// <remarks>
/// Everything here has a default that suits an application with no opinion. Options exist for the
/// things SumireCraft does to state it does not own -- not for things it merely allocates.
/// </remarks>
public sealed class SumireCraftOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to watch for exceptions that nothing else caught.
    /// The default is <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The safety net subscribes to process-wide events that no single library owns:
    /// <see cref="AppDomain.UnhandledException"/>, <see cref="TaskScheduler.UnobservedTaskException"/>
    /// and, on Windows, the WinUI application's own unhandled exception event. Everything it catches
    /// is reported with <see cref="Exceptions.ExceptionOrigin.Unhandled"/>.
    /// </para>
    /// <para>
    /// It only reports. It never marks an exception as handled and never keeps an application alive
    /// that was about to die, so switching it on cannot change what an application does -- only
    /// what it records on the way down.
    /// </para>
    /// <para>
    /// What it catches goes to the <see cref="Exceptions.IExceptionSink"/> like any other report, and
    /// from there to the <see cref="Exceptions.IExceptionHandler"/>s the application registered. With
    /// none registered, it records nothing anywhere.
    /// </para>
    /// <para>
    /// Off by default all the same. Those events belong to the whole process, and an application that
    /// asked for a widget has not asked for SumireCraft to listen to them. Leave it off when something
    /// else already owns them: a crash reporter such as Sentry or Firebase Crashlytics, or an
    /// application that subscribes to them itself and wants to decide the order in which things run.
    /// </para>
    /// </remarks>
    public bool InstallSafetyNet { get; set; }

    /// <summary>
    /// Gets or sets how long the application's <see cref="Gates.IBusyGate"/> may stay busy without
    /// a break before it is cleared, or <see langword="null"/> to never clear it. The default is
    /// <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Setting this puts a <see cref="Gates.BusyWatchdog"/> on the gate that
    /// <c>UseSumireCraftCore()</c> registers. Unlike the safety net, the watchdog changes what an
    /// application does -- it ends a busy spell that the application started -- and no single
    /// timeout suits every application, so it is off unless asked for.
    /// </para>
    /// <para>
    /// Only that one gate is watched. Gates an application creates for itself are watched by
    /// watchdogs it creates for itself.
    /// </para>
    /// </remarks>
    public TimeSpan? BusyTimeout { get; set; }
}
