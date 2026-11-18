using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.Sample.Wrapping;

/// <summary>
/// An entry as an application called Abc would write the one its pages use: underlined by itself,
/// so that no page says so, and replacing the library one day means changing this file alone.
/// </summary>
/// <remarks>
/// The underline is turned on here, as the entry's own value, which a style cannot turn off again;
/// the line's colour and thicknesses are still the styles' to set. An application that wants a
/// style to decide everything sets <c>SmrUnderline.IsEnabled</c> from one for this type instead.
/// A style written for every <see cref="Entry"/> passes a derived entry by, unless it says
/// <c>ApplyToDerivedTypes="True"</c>.
/// </remarks>
public class AbcUnderlineEntry : Entry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbcUnderlineEntry"/> class, underlined.
    /// </summary>
    public AbcUnderlineEntry()
    {
        SmrUnderline.SetIsEnabled(this, true);
    }
}
