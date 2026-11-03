using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.UnitTests.Exceptions;

public class ExceptionSinkTests
{
    [Fact]
    public void Report_ReachesEveryHandler()
    {
        var first = new RecordingHandler();
        var second = new RecordingHandler();
        var sink = new ExceptionSink([first, second]);
        var report = NewReport();

        sink.Report(report);

        Assert.Equal([report], first.Received);
        Assert.Equal([report], second.Received);
    }

    [Fact]
    public void Report_RunsHandlersInRegistrationOrder()
    {
        var order = new List<string>();
        var sink = new ExceptionSink(
        [
            new DelegateHandler(_ => order.Add("first")),
            new DelegateHandler(_ => order.Add("second")),
            new DelegateHandler(_ => order.Add("third")),
        ]);

        sink.Report(NewReport());

        Assert.Equal(["first", "second", "third"], order);
    }

    [Fact]
    public void Report_DoesNothingWhenThereAreNoHandlers()
    {
        var sink = new ExceptionSink([]);

        sink.Report(NewReport());
    }

    [Fact]
    public void Report_DoesNotThrowWhenAHandlerThrows()
    {
        var sink = new ExceptionSink([new ThrowingHandler()]);

        // Reporting a problem must never become one. A handler that fails is the sink's business,
        // not the caller's: the caller has already lost one exception and cannot use another.
        sink.Report(NewReport());
    }

    [Fact]
    public void Report_KeepsGoingWhenAHandlerThrows()
    {
        var afterTheFailure = new RecordingHandler();
        var sink = new ExceptionSink([new ThrowingHandler(), afterTheFailure]);
        var report = NewReport();

        sink.Report(report);

        // A broken alert must not cost us the log entry.
        Assert.Equal([report], afterTheFailure.Received);
    }

    [Fact]
    public async Task Report_RunsHandlersOnTheCallingThread()
    {
        var handlerThreadId = 0;
        var sink = new ExceptionSink(
            [new DelegateHandler(_ => handlerThreadId = Environment.CurrentManagedThreadId)]);
        var callerThreadId = 0;

        await Task.Run(
            () =>
            {
                callerThreadId = Environment.CurrentManagedThreadId;
                sink.Report(NewReport());
            },
            TestContext.Current.CancellationToken);

        // The sink does not marshal. Handlers that touch the UI marshal for themselves, which keeps
        // the sink usable from any thread and matches how IBusyGate raises its event.
        Assert.Equal(callerThreadId, handlerThreadId);
    }

    [Fact]
    public async Task Report_CanBeCalledFromInsideAHandler()
    {
        var received = new List<string?>();
        ExceptionSink? sink = null;
        sink = new ExceptionSink(
        [
            new DelegateHandler(report =>
            {
                received.Add(report.Source);

                if (report.Source == "outer")
                {
                    sink!.Report(NewReport("inner"));
                }
            }),
        ]);

        var cancellation = TestContext.Current.CancellationToken;
        var reporting = Task.Run(() => sink.Report(NewReport("outer")), cancellation);
        var finished = await Task.WhenAny(reporting, Task.Delay(TimeSpan.FromSeconds(5), cancellation));

        Assert.Same(reporting, finished);
        await reporting;
        Assert.Equal(["outer", "inner"], received);
    }

    [Fact]
    public void Report_ThrowsWhenReportIsNull()
    {
        var sink = new ExceptionSink([]);

        Assert.Throws<ArgumentNullException>(() => sink.Report(null!));
    }

    [Fact]
    public void Constructor_ThrowsWhenHandlersIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ExceptionSink(null!));
    }

    [Fact]
    public void Report_IsSafeToCallFromMultipleThreads()
    {
        var handler = new CountingHandler();
        var sink = new ExceptionSink([handler]);

        Parallel.For(0, 1000, _ => sink.Report(NewReport()));

        Assert.Equal(1000, handler.Count);
    }

    private static ExceptionReport NewReport(string? source = null) =>
        new(new InvalidOperationException("something failed"), ExceptionOrigin.Command, source);

    private sealed class RecordingHandler : IExceptionHandler
    {
        public List<ExceptionReport> Received { get; } = [];

        public void Handle(ExceptionReport report) => Received.Add(report);
    }

    private sealed class DelegateHandler(Action<ExceptionReport> handle) : IExceptionHandler
    {
        public void Handle(ExceptionReport report) => handle(report);
    }

    private sealed class ThrowingHandler : IExceptionHandler
    {
        public void Handle(ExceptionReport report) =>
            throw new InvalidOperationException("this handler is broken");
    }

    private sealed class CountingHandler : IExceptionHandler
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public void Handle(ExceptionReport report) => Interlocked.Increment(ref _count);
    }
}
