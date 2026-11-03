using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.Gates;

/// <summary>
/// Watches an <see cref="IBusyGate"/> and clears it when it has stayed busy for too long.
/// </summary>
/// <remarks>
/// <para>
/// A gate that never goes idle leaves every button that consults it dead, usually because an
/// <see cref="IBusyGate.Exit"/> was never reached. The watchdog is the way out: once the gate has
/// been busy without a break for <see cref="Timeout"/>, it calls <see cref="IBusyGate.Clear"/>
/// and reports a <see cref="BusyTimeoutException"/> with
/// <see cref="ExceptionOrigin.Background"/>, so the rescue does not hide the bug.
/// </para>
/// <para>
/// It measures the gate as a whole, not each owner in it. Work that keeps the gate busy
/// legitimately for longer than the timeout -- an upload, say -- belongs in a gate of its own,
/// with a watchdog of its own or none at all.
/// </para>
/// <para>
/// Nothing needs to enter the watchdog. It follows <see cref="IBusyGate.BusyChanged"/> and
/// consults <see cref="IBusyGate.IsBusy"/>, so the code it protects does not have to know it
/// exists.
/// </para>
/// <para>
/// One race is left open on purpose. If the gate goes idle and straight back to busy in the
/// instant between the timeout firing and the gate being cleared, the new work is cleared with the
/// old. Closing it would take a clear-only-if-unchanged operation on the gate, which is more
/// surface than a window that narrow is worth.
/// </para>
/// </remarks>
public sealed class BusyWatchdog : IDisposable
{
    private readonly IBusyGate _gate;
    private readonly IExceptionSink _sink;
    private readonly TimeProvider _timeProvider;
    private readonly Lock _sync = new();

    /// <summary>
    /// The timer for the current busy spell, or <see langword="null"/> while nothing is being
    /// timed. Guarded by <see cref="_sync"/>.
    /// </summary>
    private ITimer? _timer;

    /// <summary>
    /// Numbers the busy spells, so a timer that fires late can tell that its spell is over.
    /// Guarded by <see cref="_sync"/>.
    /// </summary>
    private int _spell;

    /// <summary>
    /// Guarded by <see cref="_sync"/>.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusyWatchdog"/> class and starts watching.
    /// </summary>
    /// <param name="gate">The gate to watch.</param>
    /// <param name="timeout">How long the gate may stay busy without a break.</param>
    /// <param name="sink">Where to report a timeout.</param>
    /// <param name="timeProvider">
    /// The clock to measure with, or <see langword="null"/> for <see cref="TimeProvider.System"/>.
    /// </param>
    /// <remarks>
    /// A gate that is already busy is timed from this moment, not from whenever it became busy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="gate"/> or <paramref name="sink"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="timeout"/> is zero or negative.
    /// </exception>
    public BusyWatchdog(IBusyGate gate, TimeSpan timeout, IExceptionSink sink, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(gate);
        ArgumentNullException.ThrowIfNull(sink);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);

        _gate = gate;
        _sink = sink;
        _timeProvider = timeProvider ?? TimeProvider.System;
        Timeout = timeout;

        _gate.BusyChanged += OnBusyChanged;
        Follow();
    }

    /// <summary>
    /// Gets how long the gate may stay busy without a break.
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Stops watching. The gate is left as it is.
    /// </summary>
    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            StopTimer();
        }

        _gate.BusyChanged -= OnBusyChanged;
    }

    private void OnBusyChanged(object? sender, EventArgs e) => Follow();

    /// <summary>
    /// Starts timing when the gate is busy and nothing is being timed, and stops when it is idle.
    /// </summary>
    /// <remarks>
    /// <see cref="IBusyGate.IsBusy"/> is read while holding <see cref="_sync"/>, so calls from
    /// different threads are applied in the order they saw the gate, and the last one wins. The
    /// gate never raises an event while holding its own lock, so taking its lock inside ours
    /// cannot deadlock.
    /// </remarks>
    private void Follow()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            if (!_gate.IsBusy)
            {
                StopTimer();
            }
            else if (_timer is null)
            {
                var spell = _spell;
                _timer = _timeProvider.CreateTimer(
                    _ => OnTimeout(spell),
                    state: null,
                    dueTime: Timeout,
                    period: System.Threading.Timeout.InfiniteTimeSpan);
            }
        }
    }

    private void OnTimeout(int spell)
    {
        lock (_sync)
        {
            if (_disposed || spell != _spell)
            {
                return;
            }

            // Checked again rather than trusted: the event announcing that the gate went idle
            // may still be on its way here.
            var stillBusy = _gate.IsBusy;
            StopTimer();

            if (!stillBusy)
            {
                return;
            }
        }

        // Clearing happens outside the lock because it raises BusyChanged, which comes straight
        // back to Follow. Reporting comes after clearing, so a handler that looks at the gate
        // finds it already rescued.
        _gate.Clear();
        _sink.Report(new ExceptionReport(
            new BusyTimeoutException(Timeout),
            ExceptionOrigin.Background,
            nameof(BusyWatchdog)));
    }

    /// <summary>
    /// Ends the current spell. The caller must hold <see cref="_sync"/>.
    /// </summary>
    private void StopTimer()
    {
        if (_timer is null)
        {
            return;
        }

        _timer.Dispose();
        _timer = null;
        _spell++;
    }
}
