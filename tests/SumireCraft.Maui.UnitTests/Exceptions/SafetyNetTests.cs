using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.UnitTests.Exceptions;

/// <summary>
/// Exercises the net's handlers directly. Subscribing is deliberately left untested: raising
/// <see cref="AppDomain.UnhandledException"/> would end the test host, so the one line that
/// subscribes is kept small enough to read instead.
/// </summary>
public class SafetyNetTests
{
    [Fact]
    public void OnUnhandledException_ReportsWhatWasThrown()
    {
        var sink = new RecordingSink();
        var net = new SafetyNet(sink);
        var failure = new InvalidOperationException("boom");

        net.OnUnhandledException(null, new UnhandledExceptionEventArgs(failure, isTerminating: true));

        var report = Assert.Single(sink.Received);
        Assert.Same(failure, report.Exception);
        Assert.Equal(ExceptionOrigin.Unhandled, report.Origin);
    }

    [Fact]
    public void OnUnobservedTaskException_ReportsWhatTheTaskFailedWith()
    {
        var sink = new RecordingSink();
        var net = new SafetyNet(sink);
        var failure = new InvalidOperationException("boom");

        net.OnUnobservedTaskException(
            null,
            new UnobservedTaskExceptionEventArgs(new AggregateException(failure)));

        Assert.Same(failure, Assert.Single(sink.Received).Exception);
    }

    [Fact]
    public void OnUnobservedTaskException_LeavesTheExceptionUnobserved()
    {
        var args = new UnobservedTaskExceptionEventArgs(
            new AggregateException(new InvalidOperationException()));

        new SafetyNet(new RecordingSink()).OnUnobservedTaskException(null, args);

        // The net records what happened; it does not change what happens next. Observing the
        // exception here would quietly alter an application that opted into the legacy escalation.
        Assert.False(args.Observed);
    }

    [Fact]
    public void AFailingSinkDoesNotEscapeTheNet()
    {
        var net = new SafetyNet(new ThrowingSink());

        // This runs while the process is already on its way down. Throwing here would replace the
        // failure everyone is trying to diagnose with one of our own.
        net.OnUnhandledException(
            null,
            new UnhandledExceptionEventArgs(new InvalidOperationException(), isTerminating: true));
    }

    private sealed class RecordingSink : IExceptionSink
    {
        public List<ExceptionReport> Received { get; } = [];

        public void Report(ExceptionReport report) => Received.Add(report);
    }

    private sealed class ThrowingSink : IExceptionSink
    {
        public void Report(ExceptionReport report) =>
            throw new InvalidOperationException("this sink is broken");
    }
}
