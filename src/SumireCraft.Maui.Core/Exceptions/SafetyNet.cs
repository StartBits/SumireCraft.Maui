namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// Watches the places an exception can leave an application from when nothing caught it, and
/// reports what it sees to the application's <see cref="IExceptionSink"/>.
/// </summary>
/// <param name="sink">Where to report what the net catches.</param>
/// <remarks>
/// <para>
/// The net cannot save an application. Once one of these events fires the process is usually on
/// its way down, and .NET MAUI offers no way to stop it. What the net changes is the record: an
/// application that used to vanish now says why first.
/// </para>
/// <para>
/// It never marks an exception as handled. Keeping an application alive after it has already
/// failed in a way nobody anticipated trades a visible crash for an invisible one, which is the
/// wrong trade for a library to make on an application's behalf.
/// </para>
/// </remarks>
internal sealed partial class SafetyNet(IExceptionSink sink) : IMauiInitializeService
{
    private readonly IExceptionSink _sink = sink;

    /// <inheritdoc />
    /// <remarks>
    /// Nothing here is ever undone. These events live as long as the process, and so does the net
    /// that listens to them, which is the point: the last thing an application does is exactly when
    /// it still has to be able to say what happened.
    /// </remarks>
    public void Initialize(IServiceProvider services)
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        InstallPlatformHooks();
    }

    /// <summary>
    /// Subscribes to whatever else the platform reports failures through.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implemented only where there is something to subscribe to, which today is Windows alone
    /// (see <c>SafetyNet.Windows.cs</c>). Everywhere else nothing implements it and the compiler
    /// removes the call along with the declaration, so a platform that needs no hook needs no file
    /// either.
    /// </para>
    /// <para>
    /// Declared without an accessibility modifier on purpose. A partial method may only be left
    /// unimplemented in its original form, and that form forbids one.
    /// </para>
    /// </remarks>
    partial void InstallPlatformHooks();

    /// <summary>
    /// Reports an exception that reached <see cref="AppDomain.UnhandledException"/>.
    /// </summary>
    internal void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e) =>
        ReportSafely(UnhandledExceptionTranslator.FromAppDomain(e.ExceptionObject));

    /// <summary>
    /// Reports a task that failed with nobody waiting to hear about it.
    /// </summary>
    /// <remarks>
    /// <see cref="UnobservedTaskExceptionEventArgs.SetObserved"/> is deliberately not called.
    /// Observing the exception would change what happens next for an application that turned the
    /// legacy escalation back on, and the net is only here to watch.
    /// </remarks>
    internal void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) =>
        ReportSafely(UnhandledExceptionTranslator.FromUnobservedTask(e.Exception));

    /// <summary>
    /// Reports to the sink without ever letting a failure out.
    /// </summary>
    /// <param name="report">What to report.</param>
    /// <remarks>
    /// <see cref="ExceptionSink"/> already protects itself from a handler that throws, but this
    /// code runs while the process is on its way down and may be given a sink the application wrote
    /// itself. Throwing from here would replace the failure everybody is trying to diagnose with
    /// one of ours.
    /// </remarks>
    private void ReportSafely(ExceptionReport report)
    {
        try
        {
            _sink.Report(report);
        }
        catch
        {
            // There is nowhere left to report this to.
        }
    }
}
