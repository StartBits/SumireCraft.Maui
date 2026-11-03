using Microsoft.Extensions.Logging;

namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// Writes every reported exception to the application's logger.
/// </summary>
/// <param name="logger">The logger to write to.</param>
/// <remarks>
/// <para>
/// This is the handler an application should almost always register, and the one to register
/// first: whatever else goes wrong later, the failure is on record.
/// </para>
/// <para>
/// Nothing is filtered. An exception the user was told about is still worth a log entry, because
/// the log is what is left once the user has dismissed the message and moved on.
/// </para>
/// <para>
/// Register it like any other handler; <c>UseSumireCraftCore()</c> does not add it, because what
/// an application records is its own decision:
/// </para>
/// <code>
/// builder.Services.AddSingleton&lt;IExceptionHandler, LoggingExceptionHandler&gt;();
/// </code>
/// </remarks>
/// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
public sealed partial class LoggingExceptionHandler(ILogger<LoggingExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Stands in for <see cref="ExceptionReport.Source"/> when the reporter had no name to give,
    /// so the message never reads as though something were missing.
    /// </summary>
    private const string UnknownSource = "an unknown location";

    private readonly ILogger<LoggingExceptionHandler> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="report"/> is <see langword="null"/>.</exception>
    public void Handle(ExceptionReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var level = LevelFor(report.Origin);

        LogEscapedException(
            _logger,
            level,
            report.Origin,
            report.Source ?? UnknownSource,
            report.Exception);
    }

    /// <summary>
    /// Writes the entry itself.
    /// </summary>
    /// <remarks>
    /// Source-generated rather than a call to <see cref="LoggerExtensions"/>: the generated method
    /// checks whether the level is enabled before touching its arguments, and formats without
    /// boxing the origin or allocating an argument array. Reporting an exception should not cost
    /// more than the exception did.
    /// </remarks>
    [LoggerMessage(Message = "{Origin} exception escaped from {Source}.")]
    private static partial void LogEscapedException(
        ILogger logger,
        LogLevel level,
        ExceptionOrigin origin,
        string source,
        Exception exception);

    /// <summary>
    /// Decides how loudly to report an exception, from how much of the application it took with it.
    /// </summary>
    /// <remarks>
    /// An <see cref="ExceptionOrigin.Unhandled"/> exception reached a last-resort handler, so the
    /// application is likely on its way down and this may be the final entry in the log. Anything
    /// else cost the user one operation.
    /// </remarks>
    private static LogLevel LevelFor(ExceptionOrigin origin) => origin switch
    {
        ExceptionOrigin.Unhandled => LogLevel.Critical,
        _ => LogLevel.Error,
    };
}
