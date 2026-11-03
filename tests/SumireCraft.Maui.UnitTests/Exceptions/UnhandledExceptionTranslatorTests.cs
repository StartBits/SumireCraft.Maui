using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.UnitTests.Exceptions;

public class UnhandledExceptionTranslatorTests
{
    [Fact]
    public void FromAppDomain_KeepsTheExceptionThatWasThrown()
    {
        var failure = new InvalidOperationException("boom");

        var report = UnhandledExceptionTranslator.FromAppDomain(failure);

        Assert.Same(failure, report.Exception);
    }

    [Fact]
    public void FromAppDomain_ReportsTheOriginAsUnhandled()
    {
        var report = UnhandledExceptionTranslator.FromAppDomain(new InvalidOperationException());

        Assert.Equal(ExceptionOrigin.Unhandled, report.Origin);
    }

    [Fact]
    public void FromAppDomain_NamesTheHookItCameFrom()
    {
        var report = UnhandledExceptionTranslator.FromAppDomain(new InvalidOperationException());

        // Which net caught it is the first thing worth knowing from a log.
        Assert.Equal("AppDomain.UnhandledException", report.Source);
    }

    [Fact]
    public void FromAppDomain_DescribesSomethingThatWasNotAnException()
    {
        // ExceptionObject is typed as object, and a language other than C# can throw anything.
        var report = UnhandledExceptionTranslator.FromAppDomain("a bare string");

        Assert.Contains("a bare string", report.Exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FromAppDomain_StillProducesAReportWhenThereIsNothingToDescribe()
    {
        var report = UnhandledExceptionTranslator.FromAppDomain(null);

        Assert.NotNull(report.Exception);
        Assert.Equal(ExceptionOrigin.Unhandled, report.Origin);
    }

    [Fact]
    public void FromUnobservedTask_UnwrapsAnAggregateThatHoldsOneException()
    {
        var failure = new InvalidOperationException("boom");

        var report = UnhandledExceptionTranslator.FromUnobservedTask(new AggregateException(failure));

        // The aggregate is packaging, not information. One failure should read as one failure.
        Assert.Same(failure, report.Exception);
    }

    [Fact]
    public void FromUnobservedTask_KeepsAnAggregateThatHoldsSeveral()
    {
        var aggregate = new AggregateException(
            new InvalidOperationException("first"),
            new InvalidOperationException("second"));

        var report = UnhandledExceptionTranslator.FromUnobservedTask(aggregate);

        var kept = Assert.IsType<AggregateException>(report.Exception);
        Assert.Equal(2, kept.InnerExceptions.Count);
    }

    [Fact]
    public void FromUnobservedTask_FlattensNestedAggregates()
    {
        var failure = new InvalidOperationException("boom");
        var nested = new AggregateException(new AggregateException(failure));

        var report = UnhandledExceptionTranslator.FromUnobservedTask(nested);

        Assert.Same(failure, report.Exception);
    }

    [Fact]
    public void FromUnobservedTask_NamesTheHookItCameFrom()
    {
        var report = UnhandledExceptionTranslator.FromUnobservedTask(
            new AggregateException(new InvalidOperationException()));

        Assert.Equal("TaskScheduler.UnobservedTaskException", report.Source);
    }

    [Fact]
    public void FromUnobservedTask_StillProducesAReportWhenThereIsNothingToDescribe()
    {
        var report = UnhandledExceptionTranslator.FromUnobservedTask(null);

        Assert.NotNull(report.Exception);
        Assert.Equal(ExceptionOrigin.Unhandled, report.Origin);
    }

    [Fact]
    public void FromPlatformApplication_KeepsTheExceptionAndNamesTheHook()
    {
        var failure = new InvalidOperationException("boom");

        var report = UnhandledExceptionTranslator.FromPlatformApplication(failure);

        Assert.Same(failure, report.Exception);
        Assert.Equal(ExceptionOrigin.Unhandled, report.Origin);
        Assert.Equal("Application.UnhandledException", report.Source);
    }

    [Fact]
    public void FromPlatformApplication_StillProducesAReportWhenThereIsNothingToDescribe()
    {
        // WinUI hands out an event that carries a message but not always an exception.
        var report = UnhandledExceptionTranslator.FromPlatformApplication(null);

        Assert.NotNull(report.Exception);
        Assert.Equal(ExceptionOrigin.Unhandled, report.Origin);
    }
}
