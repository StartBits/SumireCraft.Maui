namespace SumireCraft.Maui.Taps;

/// <summary>
/// Says which trigger won a settled sequence of taps, and how many taps it took.
/// </summary>
/// <param name="trigger">The trigger that won.</param>
/// <param name="tapCount">The number of taps the sequence ended on.</param>
public sealed class TapTriggeredEventArgs(ITapTrigger trigger, int tapCount) : EventArgs
{
    /// <summary>
    /// Gets the trigger that won.
    /// </summary>
    public ITapTrigger Trigger { get; } = trigger;

    /// <summary>
    /// Gets the number of taps the sequence ended on.
    /// </summary>
    /// <remarks>
    /// This is not always <see cref="ITapTrigger.Count"/>. A trigger that matches
    /// <see cref="TapMatch.AtLeast"/> five taps can win on six.
    /// </remarks>
    public int TapCount { get; } = tapCount;
}
