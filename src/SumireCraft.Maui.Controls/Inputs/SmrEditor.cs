using System.ComponentModel;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// An <see cref="Editor"/> whose box the application can draw: a line under the text, a border all
/// round, with corners rounded or square.
/// </summary>
/// <remarks>
/// <para>
/// The border is drawn as on a <see cref="SmrEntry"/>, around the whole editor: a line under it
/// runs along the bottom of the box, however many lines of text it holds. Until
/// <see cref="BorderThickness"/> or <see cref="CornerRadius"/> is given, the editor is drawn as .NET
/// MAUI draws an <see cref="Editor"/>.
/// </para>
/// <para>
/// Everything else is .NET MAUI's <see cref="Editor"/>, and a style an application wrote for every
/// <see cref="Editor"/> reaches this editor too.
/// </para>
/// </remarks>
public class SmrEditor : EditorDefaults, IBorderedInput
{
    /// <inheritdoc cref="SmrEntry.BorderThicknessProperty"/>
    public static readonly BindableProperty BorderThicknessProperty = InputBorder.CreateBorderThicknessProperty(typeof(SmrEditor));

    /// <inheritdoc cref="SmrEntry.CornerRadiusProperty"/>
    public static readonly BindableProperty CornerRadiusProperty = InputBorder.CreateCornerRadiusProperty(typeof(SmrEditor));

    /// <inheritdoc cref="SmrEntry.BorderColorProperty"/>
    public static readonly BindableProperty BorderColorProperty = InputBorder.CreateBorderColorProperty(typeof(SmrEditor));

    /// <inheritdoc cref="SmrEntry.BorderThickness"/>
    [TypeConverter(typeof(Microsoft.Maui.Converters.ThicknessTypeConverter))]
    public Thickness? BorderThickness
    {
        get => (Thickness?)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <inheritdoc cref="SmrEntry.CornerRadius"/>
    [TypeConverter(typeof(Microsoft.Maui.Converters.CornerRadiusTypeConverter))]
    public CornerRadius? CornerRadius
    {
        get => (CornerRadius?)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <inheritdoc cref="SmrEntry.BorderColor"/>
    public Color? BorderColor
    {
        get => (Color?)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }
}

/// <summary>
/// Brings the style an application wrote for every <see cref="Editor"/> to a
/// <see cref="SmrEditor"/>. Not for applications to use.
/// </summary>
/// <remarks>
/// Keeps an implicit style based on the application's style for <see cref="Editor"/> in each
/// editor's own resources, as <see cref="EntryDefaults"/> does for an entry. It is public only
/// because <see cref="SmrEditor"/> derives from it; nothing outside this library can.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class EditorDefaults : Editor
{
    /// <summary>
    /// Keeps the application's style for every <see cref="Editor"/> where it reaches this editor.
    /// </summary>
    private protected EditorDefaults()
    {
        InheritedStyle.Bridge(this, typeof(Editor), typeof(EditorDefaults));
    }
}
