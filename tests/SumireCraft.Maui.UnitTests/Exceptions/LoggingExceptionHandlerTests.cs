using Microsoft.Extensions.Logging;
using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.UnitTests.Exceptions;

public class LoggingExceptionHandlerTests
{
    [Fact]
    public void Handle_LogsTheExceptionItself()
    {
        var logger = new RecordingLogger();
        var handler = new LoggingExceptionHandler(logger);
        var failure = new InvalidOperationException("something failed");

        handler.Handle(new ExceptionReport(failure, ExceptionOrigin.Command));

        var entry = Assert.Single(logger.Entries);
        Assert.Same(failure, entry.Exception);
    }

    [Fact]
    public void Handle_LogsACommandFailureAsAnError()
    {
        var logger = new RecordingLogger();
        var handler = new LoggingExceptionHandler(logger);

        handler.Handle(NewReport(ExceptionOrigin.Command));

        Assert.Equal(LogLevel.Error, Assert.Single(logger.Entries).Level);
    }

    [Fact]
    public void Handle_LogsABackgroundFailureAsAnError()
    {
        var logger = new RecordingLogger();
        var handler = new LoggingExceptionHandler(logger);

        handler.Handle(NewReport(ExceptionOrigin.Background));

        // Nobody saw it, but it is still a defect.
        Assert.Equal(LogLevel.Error, Assert.Single(logger.Entries).Level);
    }

    [Fact]
    public void Handle_LogsAnUnhandledExceptionAsCritical()
    {
        var logger = new RecordingLogger();
        var handler = new LoggingExceptionHandler(logger);

        handler.Handle(NewReport(ExceptionOrigin.Unhandled));

        // Nothing caught this one, so the application is probably already on its way down. That is
        // worth more than the failure of a single command.
        Assert.Equal(LogLevel.Critical, Assert.Single(logger.Entries).Level);
    }

    [Fact]
    public void Handle_NamesTheSourceAndTheOrigin()
    {
        var logger = new RecordingLogger();
        var handler = new LoggingExceptionHandler(logger);

        handler.Handle(NewReport(ExceptionOrigin.Command, "SaveAsync"));

        var message = Assert.Single(logger.Entries).Message;
        Assert.Contains("SaveAsync", message, StringComparison.Ordinal);
        Assert.Contains("Command", message, StringComparison.Ordinal);
    }

    [Fact]
    public void Handle_StillSaysSomethingUsefulWithoutASource()
    {
        var logger = new RecordingLogger();
        var handler = new LoggingExceptionHandler(logger);

        handler.Handle(NewReport(ExceptionOrigin.Background));

        var message = Assert.Single(logger.Entries).Message;
        Assert.Contains("Background", message, StringComparison.Ordinal);
        Assert.DoesNotContain("null", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Handle_ThrowsWhenReportIsNull()
    {
        var handler = new LoggingExceptionHandler(new RecordingLogger());

        Assert.Throws<ArgumentNullException>(() => handler.Handle(null!));
    }

    [Fact]
    public void Constructor_ThrowsWhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new LoggingExceptionHandler(null!));
    }

    private static ExceptionReport NewReport(ExceptionOrigin origin, string? source = null) =>
        new(new InvalidOperationException("something failed"), origin, source);

    private sealed record LogEntry(LogLevel Level, Exception? Exception, string Message);

    private sealed class RecordingLogger : ILogger<LoggingExceptionHandler>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add(new LogEntry(logLevel, exception, formatter(state, exception)));
    }
}
