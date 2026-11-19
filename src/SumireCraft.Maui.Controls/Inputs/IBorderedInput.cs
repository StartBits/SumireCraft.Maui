namespace SumireCraft.Maui.Controls;

// What the library's five inputs say about their border, read alike whichever of the five it is.
internal interface IBorderedInput
{
    Thickness? BorderThickness { get; }

    CornerRadius? CornerRadius { get; }

    Color? BorderColor { get; }
}
