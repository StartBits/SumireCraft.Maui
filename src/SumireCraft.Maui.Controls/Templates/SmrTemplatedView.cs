using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// A content view whose look is a control template it brings along, and which an application's
/// styles can replace like any other.
/// </summary>
/// <remarks>
/// <para>
/// A derived view hands its template to the constructor and shows it until something else sets
/// <c>ControlTemplate</c>: an implicit style for its type, an explicit style, or the property itself,
/// whether before the view is on a page or after. Clearing the property brings the default back. A
/// style that leaves <c>ControlTemplate</c> alone, such as one that only sets a margin, keeps the
/// default.
/// </para>
/// <para>
/// An implicit style whose <c>TargetType</c> is this class, with <c>ApplyToDerivedTypes</c> on, reaches
/// every templated view at once, the library's and an application's own. One meant for every
/// <c>ContentView</c> reaches a templated view too, but its template does not replace the default,
/// which is the more particular of the two; its other setters still apply.
/// </para>
/// <para>
/// The default is an implicit style the view keeps in its own <c>Resources</c>. It reaches no view
/// placed inside, and a templated view nested in another keeps its own. A dictionary given to the
/// view in place of its own, in code or in XAML, keeps the default too: the view merges it into a
/// dictionary that holds the default, so <c>Resources</c> then returns that dictionary rather than
/// the one given, and the one given is left as it was.
/// </para>
/// <para>
/// A template that shows the view's content puts a <c>ContentPresenter</c> where it goes. A control
/// template builds a new tree for each view it is applied to, so one instance in a static field serves
/// every view.
/// </para>
/// </remarks>
/// <example>
/// <code lang="csharp"><![CDATA[
/// public class AbcCard : SmrTemplatedView
/// {
///     private static readonly ControlTemplate CardTemplate = new(() =>
///         new Border { Padding = 12, Content = new ContentPresenter() });
///
///     public AbcCard()
///         : base(CardTemplate)
///     {
///     }
/// }
/// ]]></code>
/// </example>
public abstract class SmrTemplatedView : TemplatedViewDefaults
{
    /// <summary>
    /// Gives the view the template it shows until an application gives it another.
    /// </summary>
    /// <param name="defaultTemplate">The template the view shows by default.</param>
    /// <exception cref="ArgumentNullException"><paramref name="defaultTemplate"/> is null.</exception>
    protected SmrTemplatedView(ControlTemplate defaultTemplate)
        : base(defaultTemplate)
    {
    }
}

/// <summary>
/// Holds the default template of a <see cref="SmrTemplatedView"/>. Not for applications to use.
/// </summary>
/// <remarks>
/// <para>
/// The default is an implicit style keyed on this type and kept in each view's own resources. Keyed
/// on <see cref="SmrTemplatedView"/> instead, it would be found before an application's own style for
/// <see cref="SmrTemplatedView"/>, which .NET MAUI looks up under the same key further out, and would
/// hide that style altogether. It is public only because <see cref="SmrTemplatedView"/> derives from
/// it; nothing outside this library can.
/// </para>
/// <para>
/// A dictionary given to the view in place of its own would leave the style behind, and a templated
/// view around this one would then lend it its own default. So the view takes such a dictionary in
/// a dictionary of its own that holds the style, and merges the given one into it. The given one is
/// not written to, as it may be shared.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TemplatedViewDefaults : ContentView
{
    private readonly Style _defaultStyle;

    /// <summary>
    /// Keeps the default template where an application's styles can replace it.
    /// </summary>
    /// <param name="defaultTemplate">The template the view shows by default.</param>
    /// <exception cref="ArgumentNullException"><paramref name="defaultTemplate"/> is null.</exception>
    private protected TemplatedViewDefaults(ControlTemplate defaultTemplate)
    {
        ArgumentNullException.ThrowIfNull(defaultTemplate);

        _defaultStyle = new Style(typeof(TemplatedViewDefaults))
        {
            ApplyToDerivedTypes = true,
            Setters = { new Setter { Property = ControlTemplateProperty, Value = defaultTemplate } },
        };
        Resources.Add(_defaultStyle);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName != nameof(Resources))
        {
            return;
        }

        var given = Resources;
        if (given.TryGetValue(typeof(TemplatedViewDefaults).FullName, out var held) && ReferenceEquals(held, _defaultStyle))
        {
            return;
        }

        var own = new ResourceDictionary { _defaultStyle };
        own.MergedDictionaries.Add(given);
        Resources = own;
    }
}
