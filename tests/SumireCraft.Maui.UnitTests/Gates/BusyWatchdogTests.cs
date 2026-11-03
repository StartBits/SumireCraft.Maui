using Microsoft.Extensions.Time.Testing;
using SumireCraft.Maui.Exceptions;
using SumireCraft.Maui.Gates;

namespace SumireCraft.Maui.UnitTests.Gates;

public class BusyWatchdogTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(8);

    private sealed class Owner;

    [Fact]
    public void AGateThatStaysBusyPastTheTimeout_IsCleared()
    {
        var (gate, _, time) = Watch();
        var owner = new Owner();
        gate.Enter(owner);

        time.Advance(Timeout);

        Assert.False(gate.IsBusy);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void AGateThatGoesIdleInTime_IsLeftAlone()
    {
        var (gate, sink, time) = Watch();
        var owner = new Owner();
        gate.Enter(owner);

        time.Advance(Timeout - TimeSpan.FromSeconds(1));
        gate.Exit(owner);
        time.Advance(Timeout);

        Assert.Empty(sink.Received);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void GoingIdle_StartsTheCountAgain()
    {
        var (gate, sink, time) = Watch();
        var owner = new Owner();
        gate.Enter(owner);
        time.Advance(TimeSpan.FromSeconds(5));
        gate.Exit(owner);

        gate.Enter(owner);
        time.Advance(TimeSpan.FromSeconds(5));

        // Ten seconds of busy in total, but never eight in a row.
        Assert.True(gate.IsBusy);
        Assert.Empty(sink.Received);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void AGateThatBecomesBusyAgainAfterATimeout_IsTimedFromScratch()
    {
        var (gate, sink, time) = Watch();
        var owner = new Owner();
        gate.Enter(owner);
        time.Advance(Timeout);

        gate.Enter(owner);
        time.Advance(Timeout - TimeSpan.FromSeconds(1));
        Assert.True(gate.IsBusy);
        Assert.Single(sink.Received);

        time.Advance(TimeSpan.FromSeconds(1));
        Assert.False(gate.IsBusy);
        Assert.Equal(2, sink.Received.Count);

        GC.KeepAlive(owner);
    }

    [Fact]
    public void AGateThatIsAlreadyBusy_IsTimedFromTheMomentTheWatchdogStarts()
    {
        var gate = new BusyGate();
        var sink = new RecordingSink();
        var time = new FakeTimeProvider();
        var owner = new Owner();
        gate.Enter(owner);
        time.Advance(TimeSpan.FromMinutes(1));

        using var watchdog = new BusyWatchdog(gate, Timeout, sink, time);
        Assert.True(gate.IsBusy);

        time.Advance(Timeout);
        Assert.False(gate.IsBusy);

        GC.KeepAlive(owner);
    }

    [Fact]
    public void ATimeout_IsReportedOnceAsBackgroundWork()
    {
        var (gate, sink, time) = Watch();
        var owner = new Owner();
        gate.Enter(owner);

        time.Advance(Timeout);
        time.Advance(Timeout);

        var report = Assert.Single(sink.Received);
        Assert.IsType<BusyTimeoutException>(report.Exception);
        Assert.Equal(ExceptionOrigin.Background, report.Origin);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void TheReportedException_SaysHowLongTheGateWasBusy()
    {
        var (gate, sink, time) = Watch();
        var owner = new Owner();
        gate.Enter(owner);

        time.Advance(Timeout);

        var exception = Assert.IsType<BusyTimeoutException>(Assert.Single(sink.Received).Exception);
        Assert.Equal(Timeout, exception.Duration);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void TheGateIsAlreadyIdle_WhenTheTimeoutIsReported()
    {
        var gate = new BusyGate();
        var time = new FakeTimeProvider();
        var busyWhenReported = new List<bool>();
        var sink = new RecordingSink(_ => busyWhenReported.Add(gate.IsBusy));
        using var watchdog = new BusyWatchdog(gate, Timeout, sink, time);
        var owner = new Owner();
        gate.Enter(owner);

        time.Advance(Timeout);

        Assert.Equal([false], busyWhenReported);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void AfterDispose_NothingIsClearedOrReported()
    {
        var gate = new BusyGate();
        var sink = new RecordingSink();
        var time = new FakeTimeProvider();
        var watchdog = new BusyWatchdog(gate, Timeout, sink, time);
        var owner = new Owner();
        gate.Enter(owner);

        watchdog.Dispose();
        time.Advance(Timeout);

        Assert.True(gate.IsBusy);
        Assert.Empty(sink.Received);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void DisposingTwice_IsHarmless()
    {
        var watchdog = new BusyWatchdog(new BusyGate(), Timeout, new RecordingSink(), new FakeTimeProvider());

        watchdog.Dispose();
        watchdog.Dispose();
    }

    [Fact]
    public void TheConstructor_ChecksItsArguments()
    {
        var gate = new BusyGate();
        var sink = new RecordingSink();
        var time = new FakeTimeProvider();

        Assert.Throws<ArgumentNullException>(() => new BusyWatchdog(null!, Timeout, sink, time));
        Assert.Throws<ArgumentNullException>(() => new BusyWatchdog(gate, Timeout, null!, time));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BusyWatchdog(gate, TimeSpan.Zero, sink, time));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new BusyWatchdog(gate, TimeSpan.FromSeconds(-1), sink, time));
    }

    [Fact]
    public void ABusySpellThatEndsAndRestartsJustBeforeTheDeadline_DoesNotInheritTheOldDeadline()
    {
        var (gate, sink, time) = Watch();
        var first = new Owner();
        var second = new Owner();
        gate.Enter(first);
        time.Advance(Timeout - TimeSpan.FromTicks(1));

        // The old spell ends and a new one begins in the same instant the old timer is about to
        // fire. The new work has been busy for no time at all and must not be cleared.
        gate.Exit(first);
        gate.Enter(second);
        time.Advance(TimeSpan.FromTicks(1));

        Assert.True(gate.IsBusy);
        Assert.Empty(sink.Received);
        GC.KeepAlive(first);
        GC.KeepAlive(second);
    }

    private static (BusyGate Gate, RecordingSink Sink, FakeTimeProvider Time) Watch()
    {
        var gate = new BusyGate();
        var sink = new RecordingSink();
        var time = new FakeTimeProvider();

        // Deliberately never disposed: each test owns its gate, and the watchdog dies with it.
        _ = new BusyWatchdog(gate, Timeout, sink, time);

        return (gate, sink, time);
    }

    private sealed class RecordingSink(Action<ExceptionReport>? onReport = null) : IExceptionSink
    {
        public List<ExceptionReport> Received { get; } = [];

        public void Report(ExceptionReport report)
        {
            Received.Add(report);
            onReport?.Invoke(report);
        }
    }
}
