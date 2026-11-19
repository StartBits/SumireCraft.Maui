namespace SumireCraft.Maui.Controls;

/// <summary>
/// The border an input is drawn with, as the platform is handed it.
/// </summary>
/// <param name="Thickness">Each side's thickness, in device-independent units.</param>
/// <param name="CornerRadius">Each corner's radius, in device-independent units.</param>
/// <param name="Color">The border's colour, or <see langword="null"/> for the platform's own text colour.</param>
internal readonly record struct BorderLook(Thickness Thickness, CornerRadius CornerRadius, Color? Color);
