namespace SumireCraft.Maui.Controls;

/// <summary>
/// Which corner of the badged content the badge sits over.
/// </summary>
/// <remarks>
/// Start and end rather than left and right, so that a layout mirrored for a right-to-left
/// language puts the badge where the reader expects it.
/// </remarks>
public enum BadgePlacement
{
    /// <summary>
    /// The top of the end edge: the right in a left-to-right layout, the left in a right-to-left
    /// one. The default, and where a notification count almost always goes.
    /// </summary>
    TopEnd,

    /// <summary>
    /// The top of the start edge: the left in a left-to-right layout, the right in a right-to-left
    /// one.
    /// </summary>
    TopStart,

    /// <summary>
    /// The bottom of the end edge: the right in a left-to-right layout, the left in a right-to-left
    /// one.
    /// </summary>
    BottomEnd,

    /// <summary>
    /// The bottom of the start edge: the left in a left-to-right layout, the right in a
    /// right-to-left one.
    /// </summary>
    BottomStart,
}
