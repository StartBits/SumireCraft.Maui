namespace SumireCraft.Maui.Controls;

/// <summary>
/// Gives a time picker of the application's own a <c>DateTimeInput</c> property, the way
/// <see cref="SmrTimePicker"/> has one, joined to the picker's time property named here.
/// </summary>
/// <remarks>
/// <para>
/// Put on a <see langword="partial"/> class derived from a picker, from .NET MAUI or from another
/// library, a source generator that comes with this package writes the rest of the class, as it
/// writes <see cref="SmrTimePicker"/>: a <c>DateTimeInput</c> bindable property and its
/// <c>DateTimeInputProperty</c> field, an override of <c>OnPropertyChanged</c> that writes what is
/// picked into the input, and a constructor without arguments. The picker's time and the input's
/// time are then one value, as they are on <see cref="SmrTimePicker"/>, whose remarks say what
/// follows from that.
/// </para>
/// <para>
/// The constructor empties the picker's time, which would otherwise start at a value of the picker's
/// own choosing that the input does not hold, and then calls <c>partial void OnCreated()</c>.
/// Implement it for what every picker of the kind should start with; left unimplemented, it is
/// compiled away. A constructor of the class's own that takes arguments calls <c>this()</c>.
/// </para>
/// <para>
/// The property named is the picker's time: one the class can read and write, that raises a change
/// when it is set, as a bindable property does. Its type picks the part of the input it is joined to:
/// a <see cref="TimeSpan"/>? takes <c>TimeAsTimeSpan</c>, a <see cref="TimeOnly"/>? takes
/// <c>TimeAsTimeOnly</c>, and a <see cref="DateTime"/>? takes <c>TimeAsDateTime</c>, whose date
/// means nothing. A type that cannot hold an empty time has nowhere to put one, and is not accepted.
/// </para>
/// <para>
/// The class is a top-level class that is not generic, derived from <see cref="BindableObject"/>
/// through a class it can construct without arguments, and declares none of the members the
/// generator writes. What it cannot be is reported when it is built, as an error whose code starts
/// with <c>SMR</c>. A class takes this attribute or <see cref="DateTimeInputDateAttribute"/>, not
/// both: each gives it the one <c>DateTimeInput</c>.
/// </para>
/// <para>
/// XAML that .NET MAUI turns into C# while the application is built (<c>MauiXamlInflator</c> set to
/// <c>SourceGen</c>) does not see the property: two source generators do not see what the other
/// writes. XAML compiled by XamlC or read at run time, .NET MAUI's defaults, does, and so does any
/// XAML when the picker is in a class library of its own.
/// </para>
/// <para>
/// An implicit style passes a derived picker by in .NET MAUI, so a style the application wrote for
/// the picker it derives from does not reach it, unless that style says
/// <c>ApplyToDerivedTypes="True"</c>.
/// </para>
/// </remarks>
/// <example>
/// <code lang="csharp"><![CDATA[
/// using SumireCraft.Maui.Controls;
///
/// [DateTimeInputTime(nameof(SelectedTime))]
/// public partial class AbcVendorTimePicker : VendorTimePicker
/// {
///     partial void OnCreated()
///     {
///         DisplayFormat = "HH:mm";
///     }
/// }
/// ]]></code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DateTimeInputTimeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeInputTimeAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the picker's time property.</param>
    public DateTimeInputTimeAttribute(string propertyName) => PropertyName = propertyName;

    /// <summary>
    /// Gets the name of the picker's time property.
    /// </summary>
    public string PropertyName { get; }
}
