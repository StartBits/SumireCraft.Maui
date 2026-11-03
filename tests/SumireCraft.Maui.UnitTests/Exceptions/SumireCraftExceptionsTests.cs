using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.UnitTests.Exceptions;

/// <summary>
/// Serialises the test classes that touch the process-wide entry point, so one cannot see the sink
/// another one installed.
/// </summary>
[CollectionDefinition(Name)]
public sealed class StaticExceptionReportingCollection
{
    public const string Name = "StaticExceptionReporting";
}

[Collection(StaticExceptionReportingCollection.Name)]
public class SumireCraftExceptionsTests : IDisposable
{
    public void Dispose()
    {
        SumireCraftExceptions.Use(null);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Report_DoesNothingWhenNoSinkHasBeenInstalled()
    {
        SumireCraftExceptions.Use(null);

        // An application that never called UseSumireCraftCore(), or one still starting up, must be
        // able to report without anything happening to it.
        SumireCraftExceptions.Report(new InvalidOperationException(), ExceptionOrigin.Command);
    }

    [Fact]
    public void Report_ForwardsTheReportToTheSink()
    {
        var sink = new RecordingSink();
        SumireCraftExceptions.Use(sink);
        var report = new ExceptionReport(new InvalidOperationException(), ExceptionOrigin.Command);

        SumireCraftExceptions.Report(report);

        Assert.Same(report, Assert.Single(sink.Received));
    }

    [Fact]
    public void Report_BuildsTheReportFromItsArguments()
    {
        var sink = new RecordingSink();
        SumireCraftExceptions.Use(sink);
        var failure = new InvalidOperationException();

        SumireCraftExceptions.Report(failure, ExceptionOrigin.Background, "SyncAsync");

        var report = Assert.Single(sink.Received);
        Assert.Same(failure, report.Exception);
        Assert.Equal(ExceptionOrigin.Background, report.Origin);
        Assert.Equal("SyncAsync", report.Source);
    }

    [Fact]
    public void Report_LeavesTheSourceUnsetWhenTheCallerHasNoneToGive()
    {
        var sink = new RecordingSink();
        SumireCraftExceptions.Use(sink);

        SumireCraftExceptions.Report(new InvalidOperationException(), ExceptionOrigin.Unhandled);

        Assert.Null(Assert.Single(sink.Received).Source);
    }

    [Fact]
    public void Report_ThrowsWhenTheReportIsNull()
    {
        SumireCraftExceptions.Use(new RecordingSink());

        Assert.Throws<ArgumentNullException>(() => SumireCraftExceptions.Report(null!));
    }

    [Fact]
    public void Report_ThrowsWhenTheExceptionIsNull()
    {
        SumireCraftExceptions.Use(new RecordingSink());

        Assert.Throws<ArgumentNullException>(
            () => SumireCraftExceptions.Report(null!, ExceptionOrigin.Command));
    }

    [Fact]
    public void Report_ThrowsWhenTheExceptionIsNullEvenWithoutASink()
    {
        SumireCraftExceptions.Use(null);

        // A null exception is a defect in the caller, not a runtime failure to be swallowed. It
        // cannot happen inside a catch block, which is where this method is meant to be called.
        Assert.Throws<ArgumentNullException>(
            () => SumireCraftExceptions.Report(null!, ExceptionOrigin.Command));
    }

    [Fact]
    public void Use_ReplacesTheSinkThatWasThereBefore()
    {
        var first = new RecordingSink();
        var second = new RecordingSink();
        SumireCraftExceptions.Use(first);

        SumireCraftExceptions.Use(second);
        SumireCraftExceptions.Report(new InvalidOperationException(), ExceptionOrigin.Command);

        Assert.Empty(first.Received);
        Assert.Single(second.Received);
    }

    [Fact]
    public void Bridge_HandsTheSinkToTheEntryPointWhenMauiInitializesIt()
    {
        var sink = new RecordingSink();
        var bridge = new ExceptionSinkBridge(sink);

        bridge.Initialize(new EmptyServiceProvider());
        SumireCraftExceptions.Report(new InvalidOperationException(), ExceptionOrigin.Command);

        Assert.Single(sink.Received);
    }

    private sealed class RecordingSink : IExceptionSink
    {
        public List<ExceptionReport> Received { get; } = [];

        public void Report(ExceptionReport report) => Received.Add(report);
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
