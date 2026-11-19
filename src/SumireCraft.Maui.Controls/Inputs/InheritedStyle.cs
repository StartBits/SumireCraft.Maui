using System.Runtime.CompilerServices;

namespace SumireCraft.Maui.Controls;

// Brings the style an application wrote for every Entry, Editor, Picker, DatePicker or TimePicker to
// the library's input derived from it. .NET MAUI gives an implicit style to the exact type it names,
// so that style would pass a derived input by.
internal static class InheritedStyle
{
    // The application's style for .NET MAUI's input, found as any dynamic resource is: an implicit
    // style is a resource whose key is its type's full name. A style's own BaseResourceKey would do
    // the looking up, but .NET MAUI follows it for a Style set on the view alone.
    private static readonly BindableProperty ApplicationStyleProperty = BindableProperty.CreateAttached(
        "ApplicationStyle", typeof(Style), typeof(InheritedStyle), null, propertyChanged: OnApplicationStyleChanged);

    // Each input's own dictionary and the hidden type its style is keyed on.
    private static readonly ConditionalWeakTable<VisualElement, Held> Inputs = [];

    // Keeps, in the input's own resources, an implicit style keyed on the hidden type between the
    // input and .NET MAUI's, based on the application's style for .NET MAUI's. Keyed so, it ranks as
    // .NET MAUI would rank a style for a base type: an implicit style for the input's own type takes
    // its place, and a Style set on the input goes on top of it. Keyed on the input's own type, it
    // would be found before the application's style for that type and hide it.
    public static void Bridge(VisualElement input, Type mauiType, Type keyType)
    {
        Inputs.Add(input, new Held(input.Resources, keyType));
        input.SetDynamicResource(ApplicationStyleProperty, mauiType.FullName);
    }

    private static void OnApplicationStyleChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var input = (VisualElement)bindable;

        // An input given a dictionary of the application's in place of its own goes without: that
        // dictionary may be shared, and is not written to. The style for every input of the kind
        // then passes the input by, as it would any derived input. Nothing here is needed for the
        // input to work, so the dictionaries are not merged. A style taken out of its dictionary
        // stays, as on .NET MAUI's own input: .NET MAUI tells a dynamic resource of a new value, and
        // of none taken away.
        if (!Inputs.TryGetValue(input, out var held) || !ReferenceEquals(input.Resources, held.Own)
            || newValue is not Style applicationStyle)
        {
            return;
        }

        input.Resources[held.Key.FullName] = new Style(held.Key)
        {
            ApplyToDerivedTypes = true,
            BasedOn = applicationStyle,
        };
    }

    private sealed record Held(ResourceDictionary Own, Type Key);
}
