namespace SumireCraft.Maui.Gates;

/// <summary>
/// Reports that a <see cref="IBusyGate"/> stayed busy for longer than its
/// <see cref="BusyWatchdog"/> allowed, and was cleared.
/// </summary>
/// <remarks>
/// <para>
/// Nothing throws this. The watchdog creates it and hands it to an
/// <see cref="Exceptions.IExceptionSink"/>, so it arrives without a stack trace. Where the gate got
/// stuck is not something the watchdog can see; it only knows how long.
/// </para>
/// <para>
/// A gate that stays busy is almost always a bug: an <see cref="IBusyGate.Exit"/> that was never
/// reached, or work that never finished. Clearing the gate rescues the user interface, and this
/// report keeps the rescue from hiding the bug.
/// </para>
/// </remarks>
/// <param name="duration">How long the gate had been busy when it was cleared.</param>
public sealed class BusyTimeoutException(TimeSpan duration)
    : TimeoutException($"The busy gate stayed busy for {duration} and was cleared.")
{
    /// <summary>
    /// Gets how long the gate had been busy when it was cleared.
    /// </summary>
    public TimeSpan Duration { get; } = duration;
}
