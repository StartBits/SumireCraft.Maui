using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.Sample.Wrapping;

/// <summary>
/// A field with its entry built in, as an application called Abc would write it: its pages name
/// neither the field nor the entry inside, and its styles name its own classes and keys, not the
/// library's.
/// </summary>
/// <remarks>
/// The entry is the field's own, so a page reaches it only through what the field passes on. Here
/// that is <see cref="Text"/>; a placeholder or a keyboard would be one more property each, passed on
/// the same way.
/// </remarks>
public class AbcEntryField : SmrField
{
    /// <summary>
    /// Backs the <see cref="Text"/> property.
    /// </summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(AbcEntryField),
        defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Initializes a new instance of the <see cref="AbcEntryField"/> class.
    /// </summary>
    public AbcEntryField()
    {
        TitleStyleClass = "AbcEntryFieldTitle";
        ErrorStyleClass = "AbcEntryFieldError";
        RequiredMarkStyleKey = "AbcEntryFieldRequiredMark";

        var entry = new Entry();
        entry.SetBinding(Entry.TextProperty, static (AbcEntryField field) => field.Text, BindingMode.TwoWay, source: this);
        Content = entry;
    }

    /// <summary>
    /// Gets or sets the text in the entry. Bound both ways by default, as an entry's own text is.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
