using SumireCraft.DateTimes;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// Attached properties that join a date or a time picker to a <see cref="DateTimeInput"/>.
/// </summary>
/// <remarks>
/// <para>
/// A picker given an input in <c>Date</c> shows the input's date, writes what is picked into it, and
/// leaves the input's time alone; one given an input in <c>Time</c> does the same with the time. A
/// date and a time are a <see cref="DatePicker"/> and a <see cref="TimePicker"/> bound to one input,
/// placed wherever the page wants them. An input without a date or a time is shown empty.
/// </para>
/// <para>
/// A joined picker always has an input. Writing <see langword="null"/>, or binding to a property
/// that holds <see langword="null"/>, gives the picker a new, empty input, and a binding writes back
/// by default, so a view model whose property was <see langword="null"/> receives that input and can
/// read what was entered. A picker given no input is left as it is, and a <see cref="DatePicker"/>
/// shows today, as it always does; clearing the value unjoins the picker.
/// </para>
/// <para>
/// An input raises no change notification. A picker shows what its input held when it was given,
/// and what is picked afterwards; code that wants other values shown gives the picker another input.
/// </para>
/// <para>
/// The date or the time goes into the picker's own property and comes back from it: a picker that
/// moves the value, as a <see cref="DatePicker"/> moves a date outside <c>MinimumDate</c> and
/// <c>MaximumDate</c>, 1900 to 2100 by default, to the nearest end, moves it in the input too. Widen
/// the range before giving the input to keep such a date. On Windows, the <see cref="TimePicker"/>
/// of .NET MAUI 10.0.20 cannot show an empty time: it shows midnight, and the input takes that.
/// </para>
/// <para>
/// A <see cref="DatePicker"/> holds its date in <see cref="DatePicker.DateProperty"/> and a
/// <see cref="TimePicker"/> its time in <see cref="TimePicker.TimeProperty"/>. A picker of another
/// library is told where it holds them with <c>DateTarget</c> and <c>TimeTarget</c>, once in a style
/// for its type or on each picker. The property's type picks how the part is held: a date as a
/// nullable <see cref="DateTime"/> or <see cref="DateOnly"/>, a time as a nullable
/// <see cref="TimeSpan"/>, <see cref="TimeOnly"/> or <see cref="DateTime"/>. A property of any other
/// type, a non-nullable one included, cannot show an empty input, and the picker is left alone. A
/// property holds one part: one given as both <c>DateTarget</c> and <c>TimeTarget</c> holds the date
/// alone, and the input's time is left as it is.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <DatePicker smr:SmrDateTimeInput.Date="{Binding Start}" />
/// <TimePicker smr:SmrDateTimeInput.Time="{Binding Start}" />
///
/// <Style TargetType="vendor:VendorDatePicker">
///   <Setter Property="smr:SmrDateTimeInput.DateTarget"
///           Value="{x:Static vendor:VendorDatePicker.SelectedDateProperty}" />
/// </Style>
/// ]]></code>
/// </example>
public static class SmrDateTimeInput
{
    /// <summary>
    /// Backs the <c>Date</c> attached property: the input whose date the picker shows and writes.
    /// </summary>
    public static readonly BindableProperty DateProperty = BindableProperty.CreateAttached(
        "Date",
        typeof(DateTimeInput),
        typeof(SmrDateTimeInput),
        defaultValue: null,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: static (bindable, _, newValue) =>
            DateTimeInputJoin.Join(bindable, DateTimeInputJoin.Part.Date, (DateTimeInput?)newValue),
        coerceValue: static (_, value) => value ?? new DateTimeInput());

    /// <summary>
    /// Backs the <c>Time</c> attached property: the input whose time the picker shows and writes.
    /// </summary>
    public static readonly BindableProperty TimeProperty = BindableProperty.CreateAttached(
        "Time",
        typeof(DateTimeInput),
        typeof(SmrDateTimeInput),
        defaultValue: null,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: static (bindable, _, newValue) =>
            DateTimeInputJoin.Join(bindable, DateTimeInputJoin.Part.Time, (DateTimeInput?)newValue),
        coerceValue: static (_, value) => value ?? new DateTimeInput());

    /// <summary>
    /// Backs the <c>DateTarget</c> attached property: the picker's property its date is held in, for a
    /// picker that is not a <see cref="DatePicker"/>.
    /// </summary>
    public static readonly BindableProperty DateTargetProperty = BindableProperty.CreateAttached(
        "DateTarget",
        typeof(BindableProperty),
        typeof(SmrDateTimeInput),
        defaultValue: null,
        propertyChanged: static (bindable, _, _) => DateTimeInputJoin.Retarget(bindable, DateTimeInputJoin.Part.Date));

    /// <summary>
    /// Backs the <c>TimeTarget</c> attached property: the picker's property its time is held in, for a
    /// picker that is not a <see cref="TimePicker"/>.
    /// </summary>
    public static readonly BindableProperty TimeTargetProperty = BindableProperty.CreateAttached(
        "TimeTarget",
        typeof(BindableProperty),
        typeof(SmrDateTimeInput),
        defaultValue: null,
        propertyChanged: static (bindable, _, _) => DateTimeInputJoin.Retarget(bindable, DateTimeInputJoin.Part.Time));

    /// <summary>Gets the input whose date the picker shows.</summary>
    /// <param name="picker">The picker.</param>
    /// <returns>The input, or <see langword="null"/> when the picker's date is joined to none.</returns>
    public static DateTimeInput? GetDate(BindableObject picker) => (DateTimeInput?)picker.GetValue(DateProperty);

    /// <summary>Joins the picker's date to an input.</summary>
    /// <param name="picker">The picker.</param>
    /// <param name="value">The input; <see langword="null"/> gives the picker a new, empty one.</param>
    public static void SetDate(BindableObject picker, DateTimeInput? value) => picker.SetValue(DateProperty, value);

    /// <summary>Gets the input whose time the picker shows.</summary>
    /// <param name="picker">The picker.</param>
    /// <returns>The input, or <see langword="null"/> when the picker's time is joined to none.</returns>
    public static DateTimeInput? GetTime(BindableObject picker) => (DateTimeInput?)picker.GetValue(TimeProperty);

    /// <summary>Joins the picker's time to an input.</summary>
    /// <param name="picker">The picker.</param>
    /// <param name="value">The input; <see langword="null"/> gives the picker a new, empty one.</param>
    public static void SetTime(BindableObject picker, DateTimeInput? value) => picker.SetValue(TimeProperty, value);

    /// <summary>Gets the picker's property its date is held in.</summary>
    /// <param name="picker">The picker.</param>
    /// <returns>
    /// The property, or <see langword="null"/> when none was given, in which case a
    /// <see cref="DatePicker"/>'s own <see cref="DatePicker.DateProperty"/> is used.
    /// </returns>
    public static BindableProperty? GetDateTarget(BindableObject picker) => (BindableProperty?)picker.GetValue(DateTargetProperty);

    /// <summary>Sets the picker's property its date is held in.</summary>
    /// <param name="picker">The picker.</param>
    /// <param name="value">The property, of a nullable <see cref="DateTime"/> or <see cref="DateOnly"/>.</param>
    public static void SetDateTarget(BindableObject picker, BindableProperty? value) => picker.SetValue(DateTargetProperty, value);

    /// <summary>Gets the picker's property its time is held in.</summary>
    /// <param name="picker">The picker.</param>
    /// <returns>
    /// The property, or <see langword="null"/> when none was given, in which case a
    /// <see cref="TimePicker"/>'s own <see cref="TimePicker.TimeProperty"/> is used.
    /// </returns>
    public static BindableProperty? GetTimeTarget(BindableObject picker) => (BindableProperty?)picker.GetValue(TimeTargetProperty);

    /// <summary>Sets the picker's property its time is held in.</summary>
    /// <param name="picker">The picker.</param>
    /// <param name="value">
    /// The property, of a nullable <see cref="TimeSpan"/>, <see cref="TimeOnly"/> or <see cref="DateTime"/>.
    /// </param>
    public static void SetTimeTarget(BindableObject picker, BindableProperty? value) => picker.SetValue(TimeTargetProperty, value);
}
