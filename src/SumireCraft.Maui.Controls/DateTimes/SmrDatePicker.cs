using System.ComponentModel;
using SumireCraft.DateTimes;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// A <see cref="DatePicker"/> that takes the date of a <see cref="DateTimes.DateTimeInput"/>.
/// </summary>
/// <remarks>
/// <para>
/// The picker is .NET MAUI's own, with one property more. It shows the date of its
/// <see cref="DateTimeInput"/>, writes what is picked into it, and leaves the input's time alone.
/// A date and a time are a <see cref="SmrDatePicker"/> and a <see cref="SmrTimePicker"/> bound to
/// one input, placed wherever the page wants them. An input without a date is shown empty, and so
/// is a picker that was given no input, where a <see cref="DatePicker"/> starts at today.
/// </para>
/// <para>
/// <see cref="DateTimeInput"/> is never <see langword="null"/>. A picker given none makes an empty
/// input of its own, and a binding writes back by default, so a view model whose property was
/// <see langword="null"/> receives that input and can read what was entered.
/// </para>
/// <para>
/// An input raises no change notification. A picker shows what its input held when it was given,
/// and what is picked afterwards; code that wants other values shown gives the picker another
/// input.
/// </para>
/// <para>
/// <c>Date</c> and the input's date are one value: whatever is written to <c>Date</c> goes into the
/// input. A <see cref="DatePicker"/> moves a date outside <c>MinimumDate</c> and
/// <c>MaximumDate</c>, 1900 to 2100 by default, to the nearest end, and the input takes the date
/// shown, as anything bound to a <see cref="DatePicker"/> does. Widen the range before giving the
/// input to keep such a date.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <smr:SmrDatePicker DateTimeInput="{Binding Start}" />
/// <smr:SmrTimePicker DateTimeInput="{Binding Start}" />
/// ]]></code>
/// </example>
// DateTimeInput, its bindable property, the OnPropertyChanged that writes into it and the constructor
// that starts the picker empty, where a DatePicker starts at today, are written by the source generator
// in SumireCraft.Maui.Controls.Generators, as they are for an application's own picker with the same
// attribute.
[DateTimeInputDate(nameof(Date))]
public partial class SmrDatePicker : DatePickerDefaults;

/// <summary>
/// Brings the style an application wrote for every <see cref="DatePicker"/> to a
/// <see cref="SmrDatePicker"/>. Not for applications to use.
/// </summary>
/// <remarks>
/// <para>
/// .NET MAUI gives an implicit style to the exact type it names, so the style for every
/// <see cref="DatePicker"/> in the Styles.xaml of a new project would pass a derived picker by. This
/// type keeps, in each picker's own resources, an implicit style based on whatever style the
/// application has for <see cref="DatePicker"/>, wherever it put it.
/// </para>
/// <para>
/// The style is keyed on this type, which sits between <see cref="DatePicker"/> and
/// <see cref="SmrDatePicker"/>, so that it ranks as .NET MAUI would rank a style for a base type: an
/// implicit style for <see cref="SmrDatePicker"/> itself takes its place, and a <c>Style</c> set on
/// a picker goes on top of it. Keyed on <see cref="SmrDatePicker"/> instead, it would be found
/// before an application's own style for <see cref="SmrDatePicker"/> and hide it. It is public only
/// because <see cref="SmrDatePicker"/> derives from it; nothing outside this library can.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class DatePickerDefaults : DatePicker
{
    // The application's style for every DatePicker, found as any dynamic resource is: an implicit
    // style is a resource whose key is its type's full name. A style's own BaseResourceKey would do
    // the looking up, but .NET MAUI follows it for a Style set on the view alone.
    private static readonly BindableProperty ApplicationStyleProperty = BindableProperty.Create(
        "ApplicationStyle",
        typeof(Style),
        typeof(DatePickerDefaults),
        propertyChanged: (bindable, _, newValue) => ((DatePickerDefaults)bindable).BaseOn((Style?)newValue));

    private readonly ResourceDictionary _own;

    /// <summary>
    /// Keeps the style where an application's styles rank above it.
    /// </summary>
    private protected DatePickerDefaults()
    {
        _own = Resources;
        SetDynamicResource(ApplicationStyleProperty, typeof(DatePicker).FullName);
    }

    private void BaseOn(Style? applicationStyle)
    {
        // A picker given a dictionary of the application's in place of its own goes without: that
        // dictionary may be shared, and is not written to. The style for every DatePicker then
        // passes the picker by, as it would any derived picker. Unlike a templated view's template,
        // nothing here is needed for the picker to work, so the dictionaries are not merged.
        if (!ReferenceEquals(Resources, _own))
        {
            return;
        }

        var key = typeof(DatePickerDefaults).FullName;
        if (applicationStyle is null)
        {
            Resources.Remove(key);
            return;
        }

        Resources[key] = new Style(typeof(DatePickerDefaults))
        {
            ApplyToDerivedTypes = true,
            BasedOn = applicationStyle,
        };
    }
}
