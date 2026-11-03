namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// The one place an application sends exceptions it has caught but cannot deal with where it
/// found them.
/// </summary>
/// <remarks>
/// <para>
/// A single instance is meant to be shared across the whole application through dependency
/// injection, so a failure caught in a networking layer reaches the same handlers as one caught in
/// a command.
/// </para>
/// <para>
/// Reporting never throws and never blocks on the user interface. Whatever the handlers do, the
/// caller gets control back and can carry on.
/// </para>
/// </remarks>
public interface IExceptionSink
{
    /// <summary>
    /// Hands one exception to every registered <see cref="IExceptionHandler"/>.
    /// </summary>
    /// <param name="report">The exception and its context.</param>
    /// <remarks>
    /// Handlers run on the calling thread, in registration order. A handler that fails does not
    /// stop the ones behind it, and its failure is never passed back to the caller: the whole point
    /// of the sink is that reporting a problem cannot itself become one.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="report"/> is <see langword="null"/>.</exception>
    void Report(ExceptionReport report);
}
