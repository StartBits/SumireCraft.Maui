namespace SumireCraft.Maui.Controls;

/// <summary>
/// The border an input is drawn with, as the platform is handed it.
/// </summary>
/// <param name="Thickness">Each side's thickness, in device-independent units.</param>
/// <param name="CornerRadius">Each corner's radius, in device-independent units.</param>
/// <param name="Color">The border's colour, or <see langword="null"/> for the platform's own text colour.</param>
internal readonly record struct BorderLook(Thickness Thickness, CornerRadius CornerRadius, Color? Color)
{
    /// <summary>
    /// Gets the border as a platform that draws its sides where it is told draws it, laid out the way
    /// given: right to left, the thickness given for the left is at the right, as .NET MAUI lays out a
    /// Margin or a Padding. The corners stay where they are, as .NET MAUI's Border keeps them there.
    /// </summary>
    public BorderLook LaidOut(EffectiveFlowDirection flow) => (flow & EffectiveFlowDirection.RightToLeft) == 0
        ? this
        : this with { Thickness = new Thickness(Thickness.Right, Thickness.Top, Thickness.Left, Thickness.Bottom) };
}
