namespace SumireCraft.Maui.Controls;

/// <summary>
/// The styles a <see cref="SmrField"/>'s default template starts from.
/// </summary>
internal sealed partial class FieldDefaults : ResourceDictionary
{
    /// <summary>
    /// The one instance every field takes its styles from. A style can serve many views at once.
    /// </summary>
    /// <remarks>
    /// Written as an empty collection, as the analyzers prefer for a dictionary, but not empty: the
    /// constructor fills it from the XAML.
    /// </remarks>
    public static readonly FieldDefaults Instance = [];

    public FieldDefaults() => InitializeComponent();

    /// <summary>
    /// Gets every style here. Each is kept under a key for the field to find it by, and applies
    /// through its class once the field adds it to its own resources.
    /// </summary>
    public IEnumerable<Style> Styles => [Get("Title"), Get("Error"), Get("ErrorLight"), Get("ErrorDark")];

    private Style Get(string key) => (Style)this[key];
}
