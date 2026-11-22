using System.ComponentModel;
using SumireCraft.DateTimes;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// A <see cref="TimePicker"/> that takes the time of a <see cref="DateTimes.DateTimeInput"/>, and
/// whose box the application can draw.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DateTimeInput"/> is <see cref="SmrDateTimeInput"/>'s <c>Time</c> under a shorter
/// name: one value, joined to the picker as <see cref="SmrDateTimeInput"/> joins any time picker,
/// binding both ways by default. This picker starts empty, as an input does. On Windows, the
/// <see cref="TimePicker"/> of .NET MAUI 10.0.20 cannot show an empty time: it shows midnight, and
/// the input takes that.
/// </para>
/// <para>
/// The border is drawn as on a <see cref="SmrEntry"/>. Until <see cref="BorderThickness"/> or
/// <see cref="CornerRadius"/> is given, the picker is drawn as .NET MAUI draws a
/// <see cref="TimePicker"/>. On Mac Catalyst, where .NET MAUI shows the system's time picker, a
/// compact pill whose look is the system's own, the border is not drawn.
/// </para>
/// <para>
/// Everything else is .NET MAUI's <see cref="TimePicker"/>, and a style an application wrote for
/// every <see cref="TimePicker"/> reaches this picker too.
/// </para>
/// </remarks>
public class SmrTimePicker : TimePickerDefaults, IBorderedInput
{
    /// <summary>
    /// Backs the <see cref="DateTimeInput"/> property: <see cref="SmrDateTimeInput.TimeProperty"/>.
    /// </summary>
    public static readonly BindableProperty DateTimeInputProperty = SmrDateTimeInput.TimeProperty;

    /// <inheritdoc cref="SmrEntry.BorderThicknessProperty"/>
    public static readonly BindableProperty BorderThicknessProperty = InputBorder.CreateBorderThicknessProperty(typeof(SmrTimePicker));

    /// <inheritdoc cref="SmrEntry.CornerRadiusProperty"/>
    public static readonly BindableProperty CornerRadiusProperty = InputBorder.CreateCornerRadiusProperty(typeof(SmrTimePicker));

    /// <inheritdoc cref="SmrEntry.BorderColorProperty"/>
    public static readonly BindableProperty BorderColorProperty = InputBorder.CreateBorderColorProperty(typeof(SmrTimePicker));

    /// <summary>
    /// Initializes a new instance of the <see cref="SmrTimePicker"/> class, showing no time.
    /// </summary>
    public SmrTimePicker()
    {
        Time = null;
    }

    /// <summary>
    /// Gets or sets the input whose time the picker shows and writes. Writing
    /// <see langword="null"/> gives the picker a new, empty input.
    /// </summary>
    /// <remarks>
    /// Being the attached property, it raises <c>PropertyChanged</c> under the attached property's
    /// name, <c>Time</c>, which is also the name of the picker's own <c>Time</c>: a binding whose
    /// source is the picker and whose path is <c>DateTimeInput</c> is not told of a new input. Bind
    /// the input from the view model instead, as the picker's target.
    /// </remarks>
    public DateTimeInput? DateTimeInput
    {
        get => (DateTimeInput?)GetValue(DateTimeInputProperty);
        set => SetValue(DateTimeInputProperty, value);
    }

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
/// Brings the style an application wrote for every <see cref="TimePicker"/> to a
/// <see cref="SmrTimePicker"/>. Not for applications to use.
/// </summary>
/// <remarks>
/// Keeps an implicit style based on the application's style for <see cref="TimePicker"/> in each
/// picker's own resources, as <see cref="EntryDefaults"/> does for an entry. It is public only
/// because <see cref="SmrTimePicker"/> derives from it; nothing outside this library can.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TimePickerDefaults : TimePicker
{
    /// <summary>
    /// Keeps the application's style for every <see cref="TimePicker"/> where it reaches this picker.
    /// </summary>
    private protected TimePickerDefaults()
    {
        InheritedStyle.Bridge(this, typeof(TimePicker), typeof(TimePickerDefaults));
    }
}
