namespace SumireCraft.Maui.Taps;

/// <summary>
/// Counts taps as they arrive and announces which of several triggers the finished sequence meant.
/// </summary>
/// <remarks>
/// <para>
/// One tap cannot be acted on the moment it lands once a second trigger wants five of them:
/// firing the one-tap trigger first would navigate away from the screen the fifth tap was meant to
/// reach. So a sequence stays open for as long as the tapping carries on, and settles when
/// <see cref="IntervalMilliseconds"/> passes with no further tap. Nothing fires while someone is
/// still tapping, however far past the highest count they go, and a burst of any length is
/// answered once.
/// </para>
/// <para>
/// The winner is the matching trigger asking for the most taps. Where an
/// <see cref="TapMatch.Exactly"/> and an <see cref="TapMatch.AtLeast"/> trigger ask for the same
/// count and both match, the exact one wins, because it is the more specific statement of the two.
/// A sequence that matches nothing is still a sequence: it ends quietly and the next tap starts
/// over.
/// </para>
/// <para>
/// There is no exception to that, not even for a single trigger asking for a single tap. Somewhere
/// that only ever wants one tap wants a <c>TapGestureRecognizer</c>: counting is what this is for,
/// and waiting is what counting costs.
/// </para>
/// <para>
/// One trigger asking for <see cref="TapMatch.AtLeast"/> one tap falls out of the rules as a way of
/// answering a run of taps once, whenever it ends -- which is a reasonable thing to put under a
/// view that must not be acted on twice.
/// </para>
/// <para>
/// Nothing here knows about views, gestures or commands. It is given taps and hands back a trigger,
/// which is what makes the rules above testable without a running application.
/// </para>
/// </remarks>
public sealed class TapSequence : IDisposable
{
    private readonly IReadOnlyList<ITapTrigger> _triggers;
    private readonly TimeProvider _timeProvider;
    private readonly Lock _sync = new();

    /// <summary>
    /// The timer waiting out the silence that would settle the current sequence, or
    /// <see langword="null"/> while no sequence is open. Guarded by <see cref="_sync"/>.
    /// </summary>
    private ITimer? _timer;

    /// <summary>
    /// Taps counted into the sequence that is open now. Guarded by <see cref="_sync"/>.
    /// </summary>
    private int _taps;

    /// <summary>
    /// The triggers as they stood at the latest tap of the open sequence, which is all the clock
    /// gets to see of them. Guarded by <see cref="_sync"/>.
    /// </summary>
    private IReadOnlyList<Line> _lines = [];

    /// <summary>
    /// Numbers the sequences, so a timer that fires late can tell that its sequence is over.
    /// Guarded by <see cref="_sync"/>.
    /// </summary>
    private int _generation;

    /// <summary>
    /// Guarded by <see cref="_sync"/>.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TapSequence"/> class.
    /// </summary>
    /// <param name="triggers">
    /// The triggers to choose between. The list is read on every tap rather than copied once, so a
    /// caller that fills it later -- a XAML parser, say -- does not have to rebuild the sequence.
    /// It is read in <see cref="Tap"/>, on the thread that taps, and each trigger's count and
    /// match with it: the clock settles a sequence on a thread of its own and never touches the
    /// list, so whoever owns it can change it without a lock. A change made after the last tap of
    /// a burst counts from the next burst. Entries asking for fewer than one tap are ignored.
    /// </param>
    /// <param name="intervalMilliseconds">
    /// How long a sequence waits for another tap before it settles.
    /// </param>
    /// <param name="timeProvider">
    /// The clock to measure with, or <see langword="null"/> for <see cref="TimeProvider.System"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="triggers"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="intervalMilliseconds"/> is zero or negative.
    /// </exception>
    public TapSequence(
        IReadOnlyList<ITapTrigger> triggers,
        int intervalMilliseconds,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(triggers);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(intervalMilliseconds, 0);

        _triggers = triggers;
        _timeProvider = timeProvider ?? TimeProvider.System;
        IntervalMilliseconds = intervalMilliseconds;
    }

    /// <summary>
    /// Raised when a sequence settles on a trigger. A sequence that matches nothing raises nothing.
    /// </summary>
    /// <remarks>
    /// Handlers run on the clock's own thread, never on the one that tapped, because a sequence is
    /// only ever settled by silence. A handler that touches the user interface has to marshal for
    /// itself.
    /// </remarks>
    public event EventHandler<TapTriggeredEventArgs>? Triggered;

    /// <summary>
    /// Gets how long a sequence waits for another tap before it settles.
    /// </summary>
    public int IntervalMilliseconds { get; }

    /// <summary>
    /// Records one tap.
    /// </summary>
    /// <remarks>
    /// This never raises <see cref="Triggered"/>. A tap only ever opens a sequence or keeps one
    /// open; reading it is the clock's job. Taps arriving after <see cref="Dispose"/> are ignored.
    /// </remarks>
    public void Tap()
    {
        // Read before taking the lock, so that the clock is never kept waiting on a list it has no
        // business reading.
        var lines = ReadLines();

        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _lines = lines;
            _taps++;
            Wait();
        }
    }

    /// <summary>
    /// Abandons any sequence in progress and stops measuring.
    /// </summary>
    /// <remarks>
    /// A sequence the clock has already begun to settle when this is called may still raise
    /// <see cref="Triggered"/> once, on the clock's thread. Nothing is raised after that.
    /// </remarks>
    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            EndSequence();
        }
    }

    private void OnSilence(int generation)
    {
        ITapTrigger? winner;
        int taps;

        lock (_sync)
        {
            // The sequence this timer was started for may already have settled on a tap.
            if (_disposed || generation != _generation)
            {
                return;
            }

            taps = _taps;
            winner = Resolve(_lines, taps);
            EndSequence();
        }

        Raise(winner, taps);
    }

    /// <summary>
    /// Waits out the silence that would settle the open sequence, starting the wait again if one
    /// was already running. The caller must hold <see cref="_sync"/>.
    /// </summary>
    private void Wait()
    {
        var interval = TimeSpan.FromMilliseconds(IntervalMilliseconds);

        if (_timer is null)
        {
            // The generation is captured rather than read at callback time, so a timer that fires
            // while a later sequence is open can recognize that it is late.
            var generation = _generation;
            _timer = _timeProvider.CreateTimer(
                _ => OnSilence(generation),
                state: null,
                dueTime: interval,
                period: Timeout.InfiniteTimeSpan);
        }
        else
        {
            _timer.Change(interval, Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// Closes the open sequence, whether or not it matched anything. The caller must hold
    /// <see cref="_sync"/>.
    /// </summary>
    private void EndSequence()
    {
        _taps = 0;
        _lines = [];
        _generation++;
        _timer?.Dispose();
        _timer = null;
    }

    private void Raise(ITapTrigger? trigger, int tapCount)
    {
        if (trigger is not null)
        {
            Triggered?.Invoke(this, new TapTriggeredEventArgs(trigger, tapCount));
        }
    }

    /// <summary>
    /// The triggers worth comparing against -- everything that asks for at least one tap -- with
    /// what each asks for as it stands now. Called on the thread that taps.
    /// </summary>
    private List<Line> ReadLines()
    {
        var lines = new List<Line>(_triggers.Count);

        foreach (var trigger in _triggers)
        {
            if (trigger is null)
            {
                continue;
            }

            var count = trigger.Count;

            if (count >= 1)
            {
                lines.Add(new Line(trigger, count, trigger.Match));
            }
        }

        return lines;
    }

    private static ITapTrigger? Resolve(IReadOnlyList<Line> lines, int taps)
    {
        Line? winner = null;

        foreach (var line in lines)
        {
            var matches = line.Match == TapMatch.Exactly
                ? taps == line.Count
                : taps >= line.Count;

            if (matches && (winner is not { } holder || Beats(line, holder)))
            {
                winner = line;
            }
        }

        return winner?.Trigger;
    }

    /// <summary>
    /// More taps win. At the same count the exact trigger wins, being the more specific of the
    /// two. Two triggers alike in both leave the one registered first in place.
    /// </summary>
    private static bool Beats(Line challenger, Line holder) =>
        challenger.Count > holder.Count
        || (challenger.Count == holder.Count
            && challenger.Match == TapMatch.Exactly
            && holder.Match == TapMatch.AtLeast);

    /// <summary>
    /// A trigger, and what it asked for when it was read.
    /// </summary>
    private readonly record struct Line(ITapTrigger Trigger, int Count, TapMatch Match);
}
