namespace SumireCraft.Maui.Taps;

/// <summary>
/// A number of taps, and how to compare it with the taps that arrived.
/// </summary>
/// <remarks>
/// This is everything <see cref="TapSequence"/> needs in order to pick a winner. What happens when
/// one wins -- a command, a callback, anything else -- belongs to whoever implements this.
/// </remarks>
public interface ITapTrigger
{
    /// <summary>
    /// Gets the number of taps this trigger is interested in. Anything below one is ignored.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets how <see cref="Count"/> is compared with the taps that arrived.
    /// </summary>
    TapMatch Match { get; }
}
