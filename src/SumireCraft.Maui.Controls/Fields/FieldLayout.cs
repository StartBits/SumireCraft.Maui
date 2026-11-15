namespace SumireCraft.Maui.Controls;

/// <summary>
/// The root of a <see cref="SmrField"/>'s default template: the title, the content and the error,
/// one to a row.
/// </summary>
/// <remarks>
/// Title and error take the height of their text and the content whatever is left, so a field given
/// a height of its own grows its content. The rows have no spacing between them, which would stay
/// behind a title that is not there; the labels' margins space them instead.
/// </remarks>
internal sealed class FieldLayout : Grid
{
    /// <summary>
    /// The style class the title carries for the field's own defaults.
    /// </summary>
    internal const string TitleDefaultClass = "SmrFieldTitleDefault";

    /// <summary>
    /// The style class the error carries for the field's own defaults.
    /// </summary>
    internal const string ErrorDefaultClass = "SmrFieldErrorDefault";

    /// <summary>
    /// The style class the error carries for its colour in the light theme.
    /// </summary>
    internal const string ErrorLightClass = "SmrFieldErrorLightDefault";

    /// <summary>
    /// The style class the error carries for its colour in the dark theme.
    /// </summary>
    internal const string ErrorDarkClass = "SmrFieldErrorDarkDefault";

    private readonly Span _titleSpan = new();
    private readonly Span _markSpan = new();
    private readonly FormattedString _markedTitle;

    private string? _markStyleKey;
    private string? _titleClass;
    private string? _errorClass;
    private AppTheme _theme;

    public FieldLayout()
    {
        RowDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)];
        RowSpacing = 0;

        TitleLabel = new Label();
        ErrorLabel = new Label();
        _markedTitle = new FormattedString { Spans = { _titleSpan, _markSpan } };

        // The application raises this through a weak event manager, so a layout listening to it is
        // still let go of with its page.
        var application = Application.Current;
        application?.RequestedThemeChanged += OnThemeChanged;

        _theme = application?.RequestedTheme ?? AppTheme.Light;
        ShowClasses();

        var presenter = new ContentPresenter();
        SetRow((IView)presenter, 1);
        SetRow((IView)ErrorLabel, 2);

        Add(TitleLabel);
        Add(presenter);
        Add(ErrorLabel);
    }

    /// <summary>
    /// Gets the label that shows the title.
    /// </summary>
    public Label TitleLabel { get; }

    /// <summary>
    /// Gets the label that shows the error.
    /// </summary>
    public Label ErrorLabel { get; }

    /// <summary>
    /// Gives the labels the style classes an application styles them through, after the field's own
    /// defaults. Null or empty gives a label none.
    /// </summary>
    public void UseClasses(string? titleClass, string? errorClass)
    {
        if (titleClass == _titleClass && errorClass == _errorClass)
        {
            return;
        }

        _titleClass = titleClass;
        _errorClass = errorClass;
        ShowClasses();
    }

    /// <summary>
    /// Shows the title, with a mark after it when one is given. The mark is a span of its own, so that
    /// a style can reach it alone, and it follows the title's text so that it stays right after the
    /// last word of a title that wraps.
    /// </summary>
    public void ShowTitle(string? text, string? mark)
    {
        if (string.IsNullOrEmpty(mark))
        {
            TitleLabel.FormattedText = null;
            TitleLabel.Text = text;
            return;
        }

        _titleSpan.Text = text;
        _markSpan.Text = mark;
        TitleLabel.FormattedText = _markedTitle;
    }

    /// <summary>
    /// Has the mark take the style found under the key, the way a dynamic resource is found. Null or
    /// empty leaves it looking like the title.
    /// </summary>
    public void UseMarkStyle(string? key)
    {
        if (key == _markStyleKey)
        {
            return;
        }

        _markStyleKey = key;

        if (string.IsNullOrEmpty(key))
        {
            _markSpan.RemoveDynamicResource(Span.StyleProperty);
            _markSpan.ClearValue(Span.StyleProperty);
        }
        else
        {
            _markSpan.SetDynamicResource(Span.StyleProperty, key);
        }
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        _theme = e.RequestedTheme;
        ShowClasses();
    }

    // The default classes come first: of two class styles that set the same property, the later
    // class wins, so the application's class replaces the field's own defaults.
    private void ShowClasses()
    {
        TitleLabel.StyleClass = With(_titleClass, TitleDefaultClass);
        ErrorLabel.StyleClass = With(_errorClass, ErrorDefaultClass, _theme == AppTheme.Dark ? ErrorDarkClass : ErrorLightClass);
    }

    private static string[] With(string? last, params string[] defaults) =>
        string.IsNullOrEmpty(last) ? defaults : [.. defaults, last];
}
