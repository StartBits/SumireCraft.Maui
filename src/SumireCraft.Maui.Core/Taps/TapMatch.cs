namespace SumireCraft.Maui.Taps;

/// <summary>
/// How a trigger compares its own tap count with the number of taps that actually arrived.
/// </summary>
/// <remarks>
/// Either way the comparison is made once, against the finished burst. Neither answers while the
/// tapping is still going.
/// </remarks>
public enum TapMatch
{
    /// <summary>
    /// The trigger matches only that exact number of taps. One tap more and it does not.
    /// </summary>
    /// <remarks>
    /// This is the default because it is the stricter reading: a trigger written for two taps does
    /// not also answer to three. A burst that overshoots every trigger written this way is answered
    /// by nothing at all, which is usually what someone who counted meant.
    /// </remarks>
    Exactly,

    /// <summary>
    /// The trigger matches that number of taps or more.
    /// </summary>
    /// <remarks>
    /// The one to use for a line that should still answer a burst that ran long -- five taps for a
    /// debug room stays open at six, and a single tap written this way answers three as well.
    /// </remarks>
    AtLeast,
}
