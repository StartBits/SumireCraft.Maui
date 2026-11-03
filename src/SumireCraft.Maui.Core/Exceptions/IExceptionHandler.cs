namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// Does something about an exception that has already been caught: writes it to a log, shows it to
/// the user, sends it to a crash reporter.
/// </summary>
/// <remarks>
/// <para>
/// Register as many as the application needs. They all run, in registration order; none of them
/// gets to stop the others, because the reason for keeping a log is that it is there when the
/// interesting failure happens.
/// </para>
/// <para>
/// A handler runs on whichever thread reported the exception, which may not be the UI thread. One
/// that touches the user interface has to marshal for itself.
/// </para>
/// <para>
/// A handler lives as long as the application does, and so does everything it holds. Reach for the
/// current page when one is needed rather than capturing it in a field or a lambda: a handler that
/// closes over a page keeps that page, its bindings and its view model alive for the rest of the
/// process.
/// </para>
/// </remarks>
public interface IExceptionHandler
{
    /// <summary>
    /// Handles one reported exception.
    /// </summary>
    /// <param name="report">The exception and its context.</param>
    /// <remarks>
    /// Not every handler is interested in every report. Look at
    /// <see cref="ExceptionReport.Origin"/> and return without doing anything when this one is not
    /// yours.
    /// </remarks>
    void Handle(ExceptionReport report);
}
