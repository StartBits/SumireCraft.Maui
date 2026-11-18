namespace SumireCraft.Maui.Controls;

/// <summary>
/// The line an underlined input is drawn with, as the platform is handed it.
/// </summary>
/// <param name="Color">The line's colour, or <see langword="null"/> for the platform's own text colour.</param>
/// <param name="Thickness">The line's thickness, in device-independent units.</param>
internal readonly record struct UnderlineLook(Color? Color, double Thickness);
