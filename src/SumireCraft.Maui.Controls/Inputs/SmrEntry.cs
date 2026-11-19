using System.ComponentModel;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// An <see cref="Entry"/> whose box the application can draw: a line under the text, a border all
/// round, with corners rounded or square.
/// </summary>
/// <remarks>
/// <para>
/// Until <see cref="BorderThickness"/> or <see cref="CornerRadius"/> is given, the entry is drawn as
/// .NET MAUI draws an <see cref="Entry"/>, in the platform's own box. Given either, the box is the
/// library's: <see cref="BorderThickness"/> thick at each side, one all round unless given, with each
/// corner rounded by <see cref="CornerRadius"/>, square unless given. <c>0,0,0,1</c> is a line under
/// the text. A background colour fills the same shape. Clearing both gives the platform's box back.
/// </para>
/// <para>
/// The border is <see cref="BorderColor"/>, or with none the text's colour, <c>TextColor</c>, or with
/// neither the platform's own text colour, and follows them as they change. A visual state sets any
/// of the three, so <c>Focused</c> can thicken or colour the border while the entry has the focus.
/// </para>
/// <para>
/// Everything else is .NET MAUI's <see cref="Entry"/>, and a style an application wrote for every
/// <see cref="Entry"/> -- the Styles.xaml of a new project has one -- reaches this entry too. A
/// style for <see cref="SmrEntry"/> goes on top of it, and what that leaves unset still comes from
/// the one for every <see cref="Entry"/>. <c>VisualStateManager.VisualStateGroups</c> is one value,
/// so a style that gives it replaces the groups the one for every <see cref="Entry"/> gave, and says
/// again whatever of them it keeps, such as <c>Disabled</c>.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <smr:SmrEntry Text="{Binding Name}" BorderThickness="0,0,0,1" />
///
/// <Style TargetType="smr:SmrEntry">
///     <Setter Property="BorderThickness" Value="1" />
///     <Setter Property="CornerRadius" Value="4" />
/// </Style>
/// ]]></code>
/// </example>
public class SmrEntry : EntryDefaults, IBorderedInput
{
    /// <summary>
    /// Backs the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly BindableProperty BorderThicknessProperty = InputBorder.CreateBorderThicknessProperty(typeof(SmrEntry));

    /// <summary>
    /// Backs the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly BindableProperty CornerRadiusProperty = InputBorder.CreateCornerRadiusProperty(typeof(SmrEntry));

    /// <summary>
    /// Backs the <see cref="BorderColor"/> property.
    /// </summary>
    public static readonly BindableProperty BorderColorProperty = InputBorder.CreateBorderColorProperty(typeof(SmrEntry));

    /// <summary>
    /// Gets or sets each side's thickness, or <see langword="null"/> for the platform's box.
    /// </summary>
    [TypeConverter(typeof(Microsoft.Maui.Converters.ThicknessTypeConverter))]
    public Thickness? BorderThickness
    {
        get => (Thickness?)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets each corner's radius, or <see langword="null"/> for the platform's box.
    /// </summary>
    [TypeConverter(typeof(Microsoft.Maui.Converters.CornerRadiusTypeConverter))]
    public CornerRadius? CornerRadius
    {
        get => (CornerRadius?)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the border's colour, or <see langword="null"/> for the text's.
    /// </summary>
    public Color? BorderColor
    {
        get => (Color?)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }
}

/// <summary>
/// Brings the style an application wrote for every <see cref="Entry"/> to a <see cref="SmrEntry"/>.
/// Not for applications to use.
/// </summary>
/// <remarks>
/// .NET MAUI gives an implicit style to the exact type it names, so the style for every
/// <see cref="Entry"/> would pass a derived entry by. This type keeps, in each entry's own
/// resources, an implicit style keyed on itself and based on the application's style for
/// <see cref="Entry"/>, so that it ranks as a style for a base type would: an implicit style for
/// <see cref="SmrEntry"/> itself takes its place, and a <c>Style</c> set on the entry goes on top of
/// it. It is public only because <see cref="SmrEntry"/> derives from it; nothing outside this
/// library can.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class EntryDefaults : Entry
{
    /// <summary>
    /// Keeps the application's style for every <see cref="Entry"/> where it reaches this entry.
    /// </summary>
    private protected EntryDefaults()
    {
        InheritedStyle.Bridge(this, typeof(Entry), typeof(EntryDefaults));
    }
}
