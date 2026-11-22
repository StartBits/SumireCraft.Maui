using System.ComponentModel;
using SumireCraft.DateTimes;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// A <see cref="DatePicker"/> that takes the date of a <see cref="DateTimes.DateTimeInput"/>, and
/// whose box the application can draw.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DateTimeInput"/> is <see cref="SmrDateTimeInput"/>'s <c>Date</c> under a shorter
/// name: one value, joined to the picker as <see cref="SmrDateTimeInput"/> joins any date picker,
/// binding both ways by default. Unlike a <see cref="DatePicker"/>, which starts at today, this
/// picker starts empty, as an input does.
/// </para>
/// <para>
/// The border is drawn as on a <see cref="SmrEntry"/>. Until <see cref="BorderThickness"/> or
/// <see cref="CornerRadius"/> is given, the picker is drawn as .NET MAUI draws a
/// <see cref="DatePicker"/>. On Mac Catalyst, where .NET MAUI shows the system's date picker, a
/// compact pill whose look is the system's own, the border is not drawn.
/// </para>
/// <para>
/// Everything else is .NET MAUI's <see cref="DatePicker"/>, and a style an application wrote for
/// every <see cref="DatePicker"/> reaches this picker too.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <smr:SmrDatePicker DateTimeInput="{Binding Start}" BorderThickness="0,0,0,1" />
/// <smr:SmrTimePicker DateTimeInput="{Binding Start}" BorderThickness="0,0,0,1" />
/// ]]></code>
/// </example>
public class SmrDatePicker : DatePickerDefaults, IBorderedInput
{
    /// <summary>
    /// Backs the <see cref="DateTimeInput"/> property: <see cref="SmrDateTimeInput.DateProperty"/>.
    /// </summary>
    public static readonly BindableProperty DateTimeInputProperty = SmrDateTimeInput.DateProperty;

    /// <inheritdoc cref="SmrEntry.BorderThicknessProperty"/>
    public static readonly BindableProperty BorderThicknessProperty = InputBorder.CreateBorderThicknessProperty(typeof(SmrDatePicker));

    /// <inheritdoc cref="SmrEntry.CornerRadiusProperty"/>
    public static readonly BindableProperty CornerRadiusProperty = InputBorder.CreateCornerRadiusProperty(typeof(SmrDatePicker));

    /// <inheritdoc cref="SmrEntry.BorderColorProperty"/>
    public static readonly BindableProperty BorderColorProperty = InputBorder.CreateBorderColorProperty(typeof(SmrDatePicker));

    /// <summary>
    /// Initializes a new instance of the <see cref="SmrDatePicker"/> class, showing no date.
    /// </summary>
    public SmrDatePicker()
    {
        Date = null;
    }

    /// <summary>
    /// Gets or sets the input whose date the picker shows and writes. Writing
    /// <see langword="null"/> gives the picker a new, empty input.
    /// </summary>
    /// <remarks>
    /// Being the attached property, it raises <c>PropertyChanged</c> under the attached property's
    /// name, <c>Date</c>, which is also the name of the picker's own <c>Date</c>: a binding whose
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
/// Brings the style an application wrote for every <see cref="DatePicker"/> to a
/// <see cref="SmrDatePicker"/>. Not for applications to use.
/// </summary>
/// <remarks>
/// Keeps an implicit style based on the application's style for <see cref="DatePicker"/> in each
/// picker's own resources, as <see cref="EntryDefaults"/> does for an entry. It is public only
/// because <see cref="SmrDatePicker"/> derives from it; nothing outside this library can.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class DatePickerDefaults : DatePicker
{
    /// <summary>
    /// Keeps the application's style for every <see cref="DatePicker"/> where it reaches this picker.
    /// </summary>
    private protected DatePickerDefaults()
    {
        InheritedStyle.Bridge(this, typeof(DatePicker), typeof(DatePickerDefaults));
    }
}
