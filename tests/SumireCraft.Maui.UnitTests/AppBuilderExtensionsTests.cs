using Microsoft.Extensions.Time.Testing;
using SumireCraft.Maui.Controls;
using SumireCraft.Maui.Exceptions;
using SumireCraft.Maui.Gates;

namespace SumireCraft.Maui.UnitTests;

public class AppBuilderExtensionsTests
{
    [Fact]
    public void UseSumireCraft_ReturnsSameBuilder()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);

        var result = builder.UseSumireCraft();

        Assert.Same(builder, result);
    }

    [Fact]
    public void UseSumireCraftCore_ReturnsSameBuilder()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);

        var result = builder.UseSumireCraftCore();

        Assert.Same(builder, result);
    }

    [Fact]
    public void UseSumireCraft_ThrowsWhenBuilderIsNull()
    {
        MauiAppBuilder builder = null!;

        Assert.Throws<ArgumentNullException>(() => builder.UseSumireCraft());
    }

    [Fact]
    public void UseSumireCraftCore_RegistersASingleBusyGateForTheWholeApplication()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();

        var first = provider.GetRequiredService<IBusyGate>();
        var second = provider.GetRequiredService<IBusyGate>();

        Assert.IsType<BusyGate>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void UseSumireCraftCore_LeavesAnApplicationsOwnBusyGateInPlace()
    {
        var mine = new BusyGate();
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.Services.AddSingleton<IBusyGate>(mine);

        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();

        Assert.Same(mine, provider.GetRequiredService<IBusyGate>());
    }

    [Fact]
    public void UseSumireCraftCore_RegistersASingleExceptionSinkForTheWholeApplication()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();

        var first = provider.GetRequiredService<IExceptionSink>();
        var second = provider.GetRequiredService<IExceptionSink>();

        Assert.IsType<ExceptionSink>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void UseSumireCraftCore_LeavesAnApplicationsOwnExceptionSinkInPlace()
    {
        var mine = new ExceptionSink([]);
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.Services.AddSingleton<IExceptionSink>(mine);

        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();

        Assert.Same(mine, provider.GetRequiredService<IExceptionSink>());
    }

    [Fact]
    public void UseSumireCraftCore_RegistersASinkThatWorksBeforeAnyHandlerIsAdded()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();

        // An application that has registered no handler still gets a sink it can report to.
        provider.GetRequiredService<IExceptionSink>()
            .Report(new ExceptionReport(new InvalidOperationException(), ExceptionOrigin.Command));
    }

    [Fact]
    public void UseSumireCraftCore_GivesTheSinkTheHandlersTheApplicationRegistered()
    {
        var handler = new RecordingHandler();
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.Services.AddSingleton<IExceptionHandler>(handler);
        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();
        var report = new ExceptionReport(new InvalidOperationException(), ExceptionOrigin.Command);

        provider.GetRequiredService<IExceptionSink>().Report(report);

        Assert.Equal([report], handler.Received);
    }

    [Fact]
    public void UseSumireCraftCore_RegistersTheBridgeThatConnectsTheStaticEntryPoint()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();

        // .NET MAUI calls this during Build(), which is the first moment the sink can be resolved.
        Assert.Contains(
            provider.GetServices<IMauiInitializeService>(),
            service => service is ExceptionSinkBridge);
    }

    [Fact]
    public void SafetyNet_IsOffByDefault()
    {
        Assert.False(new SumireCraftOptions().InstallSafetyNet);
    }

    [Fact]
    public void UseSumireCraftCore_LeavesTheSafetyNetOutByDefault()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();

        // Left out of the registrations, not merely made to do nothing: an application that did not
        // ask must not have SumireCraft on those events at all.
        Assert.DoesNotContain(
            provider.GetServices<IMauiInitializeService>(),
            service => service is SafetyNet);
    }

    [Fact]
    public void UseSumireCraftCore_InstallsTheSafetyNetWhenAsked()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore(options => options.InstallSafetyNet = true);

        using var provider = builder.Services.BuildServiceProvider();

        Assert.Contains(provider.GetServices<IMauiInitializeService>(), service => service is SafetyNet);
    }

    [Fact]
    public void BusyTimeout_IsOffByDefault()
    {
        Assert.Null(new SumireCraftOptions().BusyTimeout);
    }

    [Fact]
    public void UseSumireCraftCore_DoesNotWatchTheBusyGateByDefault()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();

        Assert.DoesNotContain(
            provider.GetServices<IMauiInitializeService>(),
            service => service is BusyWatchdogInstaller);
    }

    [Fact]
    public void UseSumireCraftCore_WatchesTheBusyGateWhenATimeoutIsSet()
    {
        var time = new FakeTimeProvider();
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.Services.AddSingleton<TimeProvider>(time);
        builder.UseSumireCraftCore(options => options.BusyTimeout = TimeSpan.FromSeconds(8));

        using var provider = builder.Services.BuildServiceProvider();

        // Only the installer is run. Initializing everything would also connect the static entry
        // point, which other tests rely on being left alone.
        provider.GetServices<IMauiInitializeService>()
            .OfType<BusyWatchdogInstaller>()
            .Single()
            .Initialize(provider);

        var gate = provider.GetRequiredService<IBusyGate>();
        var owner = new object();
        gate.Enter(owner);
        time.Advance(TimeSpan.FromSeconds(8));

        Assert.False(gate.IsBusy);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void UseSumireCraftCore_RejectsABusyTimeoutThatIsNotPositive()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);

        // Rejected here rather than when the container first builds the watchdog, so the mistake
        // points at the line that made it.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.UseSumireCraftCore(options => options.BusyTimeout = TimeSpan.Zero));
    }

    [Fact]
    public void UseSumireCraft_PassesTheOptionsThroughToTheCore()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraft(options => options.InstallSafetyNet = true);

        using var provider = builder.Services.BuildServiceProvider();

        Assert.Contains(provider.GetServices<IMauiInitializeService>(), service => service is SafetyNet);
    }

    [Fact]
    public void UseSumireCraft_RegistersTheCoreServicesAsWell()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraft();

        using var provider = builder.Services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IBusyGate>());
        Assert.NotNull(provider.GetRequiredService<IExceptionSink>());
    }

    // A second call is easy to make without meaning to: an application calls UseSumireCraft() while
    // a shared extension of its own calls UseSumireCraftCore(). Registering twice would put two
    // nets on the process-wide events and two watchdogs on the gate, and every failure would be
    // reported twice. A second call that asks for nothing is let through; one that passes options
    // could only have them dropped, so it throws rather than start an application other than the
    // one asked for.

    [Fact]
    public void UseSumireCraftCore_CalledAgainWithoutOptions_RegistersEverythingOnce()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore(Everything);

        var returned = builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();
        var initializers = provider.GetServices<IMauiInitializeService>().ToList();

        Assert.Same(builder, returned);
        Assert.Single(initializers, service => service is ExceptionSinkBridge);
        Assert.Single(initializers, service => service is SafetyNet);
        Assert.Single(initializers, service => service is BusyWatchdogInstaller);
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(IBusyGate));
        Assert.Single(builder.Services, descriptor => descriptor.ServiceType == typeof(IExceptionSink));
    }

    [Fact]
    public void UseSumireCraftCore_AfterUseSumireCraft_RegistersEverythingOnce()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraft(options => options.InstallSafetyNet = true);
        builder.UseSumireCraftCore();

        using var provider = builder.Services.BuildServiceProvider();
        var initializers = provider.GetServices<IMauiInitializeService>().ToList();

        Assert.Single(initializers, service => service is ExceptionSinkBridge);
        Assert.Single(initializers, service => service is SafetyNet);
    }

    [Fact]
    public void UseSumireCraftCore_CalledAgainWithOptions_ThrowsWithoutRunningThem()
    {
        var configuredAgain = false;
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore();

        Assert.Throws<InvalidOperationException>(() => builder.UseSumireCraftCore(options =>
        {
            configuredAgain = true;
            Everything(options);
        }));

        Assert.False(configuredAgain);
    }

    [Fact]
    public void UseSumireCraft_AfterUseSumireCraftCore_WithOptions_Throws()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore();

        Assert.Throws<InvalidOperationException>(() => builder.UseSumireCraft(Everything));
    }

    [Fact]
    public void UseSumireCraft_CalledAgainWithOptions_Throws()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraft(Everything);

        Assert.Throws<InvalidOperationException>(() => builder.UseSumireCraft(Everything));
    }

    [Fact]
    public void UseSumireCraftCore_OnAnotherBuilder_IsNotTakenForASecondCall()
    {
        MauiApp.CreateBuilder(useDefaults: false).UseSumireCraftCore(Everything);

        var builder = MauiApp.CreateBuilder(useDefaults: false);
        builder.UseSumireCraftCore(Everything);

        using var provider = builder.Services.BuildServiceProvider();

        Assert.Contains(provider.GetServices<IMauiInitializeService>(), service => service is SafetyNet);
    }

    /// <summary>
    /// Switches on everything that is off by default, so a registration made twice has something to
    /// show for it.
    /// </summary>
    private static void Everything(SumireCraftOptions options)
    {
        options.InstallSafetyNet = true;
        options.BusyTimeout = TimeSpan.FromSeconds(8);
    }

    private sealed class RecordingHandler : IExceptionHandler
    {
        public List<ExceptionReport> Received { get; } = [];

        public void Handle(ExceptionReport report) => Received.Add(report);
    }
}
