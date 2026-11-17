using System.ComponentModel;
using SumireCraft.DateTimes;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// A <see cref="TimePicker"/> that takes the time of a <see cref="DateTimes.DateTimeInput"/>.
/// </summary>
/// <remarks>
/// <para>
/// The picker is .NET MAUI's own, with one property more. It shows the time of its
/// <see cref="DateTimeInput"/>, writes what is picked into it, and leaves the input's date alone.
/// A date and a time are a <see cref="SmrDatePicker"/> and a <see cref="SmrTimePicker"/> bound to
/// one input, placed wherever the page wants them. An input without a time is shown empty, and so
/// is a picker that was given no input, where a <see cref="TimePicker"/> starts at midnight.
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
/// <c>Time</c> and the input's time are one value: whatever is written to <c>Time</c> goes into the
/// input. That includes what the platform writes: on Windows, the <see cref="TimePicker"/> of .NET
/// MAUI 10.0.20 cannot show an empty time, shows midnight instead, and writes it, so there an
/// input's time is no longer empty once a picker has shown it.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <smr:SmrDatePicker DateTimeInput="{Binding Start}" />
/// <smr:SmrTimePicker DateTimeInput="{Binding Start}" />
/// ]]></code>
/// </example>
// DateTimeInput, its bindable property, the OnPropertyChanged that writes into it and the constructor
// that starts the picker empty, where a TimePicker starts at midnight, are written by the source
// generator in SumireCraft.Maui.Controls.Generators, as they are for an application's own picker with
// the same attribute.
[DateTimeInputTime(nameof(Time))]
public partial class SmrTimePicker : TimePickerDefaults;

/// <summary>
/// Brings the style an application wrote for every <see cref="TimePicker"/> to a
/// <see cref="SmrTimePicker"/>. Not for applications to use.
/// </summary>
/// <remarks>
/// It does for <see cref="SmrTimePicker"/> what <see cref="DatePickerDefaults"/> does for
/// <see cref="SmrDatePicker"/>, in the same way and for the same reasons.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TimePickerDefaults : TimePicker
{
    // The application's style for every TimePicker, found as any dynamic resource is.
    private static readonly BindableProperty ApplicationStyleProperty = BindableProperty.Create(
        "ApplicationStyle",
        typeof(Style),
        typeof(TimePickerDefaults),
        propertyChanged: (bindable, _, newValue) => ((TimePickerDefaults)bindable).BaseOn((Style?)newValue));

    private readonly ResourceDictionary _own;

    /// <summary>
    /// Keeps the style where an application's styles rank above it.
    /// </summary>
    private protected TimePickerDefaults()
    {
        _own = Resources;
        SetDynamicResource(ApplicationStyleProperty, typeof(TimePicker).FullName);
    }

    private void BaseOn(Style? applicationStyle)
    {
        // A picker given a dictionary of the application's in place of its own goes without, as
        // DatePickerDefaults says: that dictionary may be shared, and is not written to.
        if (!ReferenceEquals(Resources, _own))
        {
            return;
        }

        var key = typeof(TimePickerDefaults).FullName;
        if (applicationStyle is null)
        {
            Resources.Remove(key);
            return;
        }

        Resources[key] = new Style(typeof(TimePickerDefaults))
        {
            ApplyToDerivedTypes = true,
            BasedOn = applicationStyle,
        };
    }
}
