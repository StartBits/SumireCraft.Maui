using System.ComponentModel;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// A <see cref="Picker"/> whose box the application can draw: a line under the text, a border all
/// round, with corners rounded or square.
/// </summary>
/// <remarks>
/// <para>
/// The border is drawn as on a <see cref="SmrEntry"/>. Until <see cref="BorderThickness"/> or
/// <see cref="CornerRadius"/> is given, the picker is drawn as .NET MAUI draws a
/// <see cref="Picker"/>. What opens when the picker is tapped is the platform's own either way.
/// </para>
/// <para>
/// Everything else is .NET MAUI's <see cref="Picker"/>, and a style an application wrote for every
/// <see cref="Picker"/> reaches this picker too.
/// </para>
/// </remarks>
public class SmrPicker : PickerDefaults, IBorderedInput
{
    /// <inheritdoc cref="SmrEntry.BorderThicknessProperty"/>
    public static readonly BindableProperty BorderThicknessProperty = InputBorder.CreateBorderThicknessProperty(typeof(SmrPicker));

    /// <inheritdoc cref="SmrEntry.CornerRadiusProperty"/>
    public static readonly BindableProperty CornerRadiusProperty = InputBorder.CreateCornerRadiusProperty(typeof(SmrPicker));

    /// <inheritdoc cref="SmrEntry.BorderColorProperty"/>
    public static readonly BindableProperty BorderColorProperty = InputBorder.CreateBorderColorProperty(typeof(SmrPicker));

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
/// Brings the style an application wrote for every <see cref="Picker"/> to a
/// <see cref="SmrPicker"/>. Not for applications to use.
/// </summary>
/// <remarks>
/// Keeps an implicit style based on the application's style for <see cref="Picker"/> in each
/// picker's own resources, as <see cref="EntryDefaults"/> does for an entry. It is public only
/// because <see cref="SmrPicker"/> derives from it; nothing outside this library can.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class PickerDefaults : Picker
{
    /// <summary>
    /// Keeps the application's style for every <see cref="Picker"/> where it reaches this picker.
    /// </summary>
    private protected PickerDefaults()
    {
        InheritedStyle.Bridge(this, typeof(Picker), typeof(PickerDefaults));
    }
}
