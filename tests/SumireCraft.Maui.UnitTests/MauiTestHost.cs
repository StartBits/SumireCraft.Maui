using System.Runtime.CompilerServices;

namespace SumireCraft.Maui.UnitTests;

/// <summary>
/// Gives the test process a dispatcher.
/// </summary>
/// <remarks>
/// Plain net10.0 has no platform to supply one, and a good deal of .NET MAUI asks for it all the
/// same: a behavior attaching to a view, a control template being applied. Without a dispatcher
/// those throw. This one runs the work where it stands, which is all a test on one thread needs.
/// </remarks>
internal static class MauiTestHost
{
    [ModuleInitializer]
    internal static void Initialize() => DispatcherProvider.SetCurrent(new Provider());

    private sealed class Provider : IDispatcherProvider
    {
        private readonly InlineDispatcher _dispatcher = new();

        public IDispatcher? GetForCurrentThread() => _dispatcher;
    }

    private sealed class InlineDispatcher : IDispatcher
    {
        public bool IsDispatchRequired => false;

        public bool Dispatch(Action action)
        {
            action();

            return true;
        }

        public bool DispatchDelayed(TimeSpan delay, Action action) =>
            throw new NotSupportedException("Nothing under test schedules work for later.");

        public IDispatcherTimer CreateTimer() =>
            throw new NotSupportedException("Nothing under test asks for a dispatcher timer.");
    }
}
