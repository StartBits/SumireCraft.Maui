using System.Runtime.CompilerServices;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// Draws an input -- an <see cref="Entry"/>, <see cref="Editor"/>, <see cref="Picker"/>,
/// <see cref="DatePicker"/> or <see cref="TimePicker"/> -- as a line under its text instead of the
/// platform's own box.
/// </summary>
/// <remarks>
/// <para>
/// The attached properties are the input's own, so a style, a binding or a visual state sets them,
/// and each input keeps its own even when one style reaches many. <c>IsEnabled</c> draws the line;
/// set back to <see langword="false"/>, the input has the platform's box again.
/// </para>
/// <para>
/// The line is <c>Color</c>, or with none the text's colour, <c>TextColor</c>, or with neither the
/// platform's own text colour, and follows them as they change. It is <c>Thickness</c> thick, and
/// <c>FocusedThickness</c> while the input has the focus. A visual state for <c>Focused</c> that
/// sets <c>Color</c> gives the focus a colour of its own; the focused thickness is
/// <c>FocusedThickness</c> wherever it is set.
/// </para>
/// <para>
/// An <see cref="Editor"/> has its line across the bottom of the whole box. A background colour of
/// the input's own stays, with the line drawn on it. A view that is not one of the five is left
/// alone, and so are a <see cref="DatePicker"/> and a <see cref="TimePicker"/> on Mac Catalyst,
/// where .NET MAUI shows the system's date picker, a compact pill whose look is the system's own.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <Entry Text="{Binding Name}" smr:SmrUnderline.IsEnabled="True" />
///
/// <Style TargetType="Entry">
///     <Setter Property="smr:SmrUnderline.IsEnabled" Value="True" />
///     <Setter Property="smr:SmrUnderline.Color" Value="Teal" />
/// </Style>
/// ]]></code>
/// </example>
public static class SmrUnderline
{
    /// <summary>
    /// Backs the IsEnabled attached property.
    /// </summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.CreateAttached(
        "IsEnabled", typeof(bool), typeof(SmrUnderline), false, propertyChanged: OnIsEnabledChanged);

    /// <summary>
    /// Backs the Color attached property.
    /// </summary>
    public static readonly BindableProperty ColorProperty = BindableProperty.CreateAttached(
        "Color", typeof(Color), typeof(SmrUnderline), null, propertyChanged: OnLookChanged);

    /// <summary>
    /// Backs the Thickness attached property.
    /// </summary>
    public static readonly BindableProperty ThicknessProperty = BindableProperty.CreateAttached(
        "Thickness", typeof(double), typeof(SmrUnderline), 1d, validateValue: IsThickness, propertyChanged: OnLookChanged);

    /// <summary>
    /// Backs the FocusedThickness attached property.
    /// </summary>
    public static readonly BindableProperty FocusedThicknessProperty = BindableProperty.CreateAttached(
        "FocusedThickness", typeof(double), typeof(SmrUnderline), 2d, validateValue: IsThickness, propertyChanged: OnLookChanged);

    // The inputs underlined now, each with what draws it. Held weakly, so that an underlined input is
    // let go of with its page; what draws it goes with it.
    private static readonly ConditionalWeakTable<View, UnderlineAttachment> Attachments = [];

    /// <summary>Gets whether the input is underlined.</summary>
    /// <param name="view">The input.</param>
    /// <returns>Whether it is underlined.</returns>
    public static bool GetIsEnabled(BindableObject view) => (bool)view.GetValue(IsEnabledProperty);

    /// <summary>Sets whether the input is underlined.</summary>
    /// <param name="view">The input.</param>
    /// <param name="value">Whether to underline it.</param>
    public static void SetIsEnabled(BindableObject view, bool value) => view.SetValue(IsEnabledProperty, value);

    /// <summary>Gets the line's colour of its own.</summary>
    /// <param name="view">The input.</param>
    /// <returns>The colour, or <see langword="null"/> for the text's.</returns>
    public static Color? GetColor(BindableObject view) => (Color?)view.GetValue(ColorProperty);

    /// <summary>Sets the line's colour of its own.</summary>
    /// <param name="view">The input.</param>
    /// <param name="value">The colour, or <see langword="null"/> for the text's.</param>
    public static void SetColor(BindableObject view, Color? value) => view.SetValue(ColorProperty, value);

    /// <summary>Gets the line's thickness.</summary>
    /// <param name="view">The input.</param>
    /// <returns>The thickness.</returns>
    public static double GetThickness(BindableObject view) => (double)view.GetValue(ThicknessProperty);

    /// <summary>Sets the line's thickness.</summary>
    /// <param name="view">The input.</param>
    /// <param name="value">The thickness.</param>
    public static void SetThickness(BindableObject view, double value) => view.SetValue(ThicknessProperty, value);

    /// <summary>Gets the line's thickness while the input has the focus.</summary>
    /// <param name="view">The input.</param>
    /// <returns>The thickness.</returns>
    public static double GetFocusedThickness(BindableObject view) => (double)view.GetValue(FocusedThicknessProperty);

    /// <summary>Sets the line's thickness while the input has the focus.</summary>
    /// <param name="view">The input.</param>
    /// <param name="value">The thickness.</param>
    public static void SetFocusedThickness(BindableObject view, double value) => view.SetValue(FocusedThicknessProperty, value);

    /// <summary>
    /// The line the input is drawn with now, or <see langword="null"/> when it is not underlined.
    /// </summary>
    internal static UnderlineLook? LookOf(View view) =>
        Attachments.TryGetValue(view, out var attachment) ? attachment.Look : null;

    // A line no thinner than nothing, and a number.
    private static bool IsThickness(BindableObject bindable, object value) => value is double d && d >= 0 && double.IsFinite(d);

    // The views it draws: the inputs whose platform control has a box of its own to take away.
    private static bool IsInput(View view) => view is Entry or Editor or Picker or DatePicker or TimePicker;

    private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View view || !IsInput(view))
        {
            return;
        }

        if ((bool)newValue)
        {
            if (!Attachments.TryGetValue(view, out _))
            {
                Attachments.Add(view, new UnderlineAttachment(view));
            }
        }
        else if (Attachments.TryGetValue(view, out var attachment))
        {
            attachment.Detach();
            Attachments.Remove(view);
        }
    }

    private static void OnLookChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is View view && Attachments.TryGetValue(view, out var attachment))
        {
            attachment.Refresh(always: true);
        }
    }
}
