using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.Sample.Wrapping;

/// <summary>
/// The entry an application called Abc would have its pages use: its own type, so that no page
/// names the library, and replacing the library one day means changing this file alone.
/// </summary>
/// <remarks>
/// Its look is a style for this type in Wrapping/AbcStyles.xaml: a line under the text, thicker
/// while the entry has the focus. A style rather than the constructor, so that a page can still
/// change it. The style for every <see cref="Entry"/> in Styles.xaml reaches it as it reaches a
/// <see cref="SmrEntry"/>.
/// </remarks>
public class AbcEntry : SmrEntry;
