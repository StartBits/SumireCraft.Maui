using SumireCraft.Maui.Controls;
using SumireCraft.Maui.Exceptions;
using SumireCraft.Maui.UnitTests.Exceptions;

namespace SumireCraft.Maui.UnitTests.Taps;

// Where a command's failure goes is the application's decision, so the behavior neither catches it
// nor reports it. Kept apart from SmrTapBehaviorTests because it installs the process-wide sink to
// see that nothing reaches it.
[Collection(StaticExceptionReportingCollection.Name)]
public class SmrTapBehaviorExceptionTests : IDisposable
{
    public void Dispose()
    {
        SumireCraftExceptions.Use(null);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ACommandThatThrows_IsNeitherCaughtNorReported()
    {
        var sink = new RecordingSink();
        SumireCraftExceptions.Use(sink);
        var failure = new InvalidOperationException();
        var (view, _, time) = SmrTapBehaviorTests.Attach(
            new SmrTap { Command = new Command(() => throw failure) });

        SmrTapBehaviorTests.Tap(view, 1);

        // With no dispatcher, the command runs on the clock's tick, which is what moving the clock
        // by hand runs.
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => SmrTapBehaviorTests.Settle(time)));
        Assert.Empty(sink.Received);
    }

    private sealed class RecordingSink : IExceptionSink
    {
        public List<ExceptionReport> Received { get; } = [];

        public void Report(ExceptionReport report) => Received.Add(report);
    }
}
