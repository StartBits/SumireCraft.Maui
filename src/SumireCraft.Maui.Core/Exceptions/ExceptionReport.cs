namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// One exception, together with enough context for a handler to decide what to do about it.
/// </summary>
/// <param name="exception">The exception that was caught.</param>
/// <param name="origin">What the application was doing when it surfaced.</param>
/// <param name="source">
/// Where it came from, for a human reading a log: usually the name of the method that threw.
/// </param>
/// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
public sealed class ExceptionReport(Exception exception, ExceptionOrigin origin, string? source = null)
{
    /// <summary>
    /// Gets the exception that was caught.
    /// </summary>
    public Exception Exception { get; } = exception ?? throw new ArgumentNullException(nameof(exception));

    /// <summary>
    /// Gets what the application was doing when the exception surfaced.
    /// </summary>
    public ExceptionOrigin Origin { get; } = origin;

    /// <summary>
    /// Gets the name of whatever the exception escaped from, or <see langword="null"/> when the
    /// reporter had nothing useful to say.
    /// </summary>
    public string? Source { get; } = source;
}
