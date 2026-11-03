using System.Collections.Immutable;

namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// The default <see cref="IExceptionSink"/>.
/// </summary>
public sealed class ExceptionSink : IExceptionSink
{
    private readonly ImmutableArray<IExceptionHandler> _handlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionSink"/> class.
    /// </summary>
    /// <param name="handlers">
    /// The handlers to hand every report to, in the order they should run. An empty sequence is
    /// allowed: an application that has registered no handler yet still gets a sink that works.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="handlers"/> is <see langword="null"/>.</exception>
    public ExceptionSink(IEnumerable<IExceptionHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        _handlers = [.. handlers];
    }

    /// <inheritdoc />
    public void Report(ExceptionReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        foreach (var handler in _handlers)
        {
            try
            {
                handler.Handle(report);
            }
            catch
            {
                // Swallowed on purpose, and this is the one place in the library where that is the
                // right thing to do. The caller has already given up on one exception and has
                // nowhere to put a second one; rethrowing here would turn a broken alert into the
                // crash the sink exists to prevent. The handlers behind this one still run, so a
                // failing alert never costs us the log entry.
            }
        }
    }
}
