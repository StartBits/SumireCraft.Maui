namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// Reports an exception from code that cannot be handed an <see cref="IExceptionSink"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IExceptionSink"/> is the real interface; inject it wherever injection is possible.
/// This exists for the places where it is not: a compile-time aspect writing into a method body, an
/// event handler on a view that nothing constructed through the container, a static helper.
/// </para>
/// <para>
/// <c>UseSumireCraftCore()</c> connects this to the application's sink while
/// <c>MauiAppBuilder.Build()</c> runs. There is no other way to set it, and nothing to configure.
/// </para>
/// <para>
/// Before that moment -- and in an application that never called <c>UseSumireCraftCore()</c> --
/// reporting does nothing at all. It is deliberately silent rather than loud: a diagnostic entry
/// point that throws during start-up would cause exactly the kind of failure it exists to record.
/// </para>
/// </remarks>
public static class SumireCraftExceptions
{
    private static IExceptionSink? _sink;

    /// <summary>
    /// Reports an exception to the application's sink.
    /// </summary>
    /// <param name="report">The exception and its context.</param>
    /// <exception cref="ArgumentNullException"><paramref name="report"/> is <see langword="null"/>.</exception>
    public static void Report(ExceptionReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        Volatile.Read(ref _sink)?.Report(report);
    }

    /// <summary>
    /// Reports an exception to the application's sink, building the report from its parts.
    /// </summary>
    /// <param name="exception">The exception that was caught.</param>
    /// <param name="origin">What the application was doing when it surfaced.</param>
    /// <param name="source">Usually the name of the method that threw.</param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The overload most callers want, because it fits on one line inside a <c>catch</c> block.
    /// </remarks>
    public static void Report(Exception exception, ExceptionOrigin origin, string? source = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var sink = Volatile.Read(ref _sink);

        // Checked before building the report so that an application without a sink pays nothing
        // for reporting, however often it reports.
        sink?.Report(new ExceptionReport(exception, origin, source));
    }

    /// <summary>
    /// Points this entry point at a sink, or at nothing when <paramref name="sink"/> is
    /// <see langword="null"/>.
    /// </summary>
    /// <param name="sink">The sink to report to.</param>
    /// <remarks>
    /// Called by <see cref="ExceptionSinkBridge"/> during <c>MauiAppBuilder.Build()</c>. Kept
    /// internal so that an application has exactly one way to arrange this, and so that two parts
    /// of a program cannot quietly take the entry point from each other.
    /// </remarks>
    internal static void Use(IExceptionSink? sink) => Volatile.Write(ref _sink, sink);
}
