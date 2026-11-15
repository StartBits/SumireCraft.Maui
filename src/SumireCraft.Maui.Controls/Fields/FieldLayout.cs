using System.ComponentModel;

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

    // What a span does not take from its label by itself. .NET MAUI hands a span its label's whole
    // font only when the span's own font is unset, and it never is, since the label's size always
    // fills it in; so a span keeps the label's size and colour but loses its family and attributes.
    // A label and a span share these properties, both taking them from FontElement.
    private static readonly BindableProperty[] Font =
        [Label.FontFamilyProperty, Label.FontAttributesProperty, Label.FontAutoScalingEnabledProperty];

    private readonly Span _titleSpan = new();
    private readonly FormattedString _markedTitle;

    private Span _markSpan = new();

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

        // Both live and die with this layout, so listening to them holds nothing longer than it lives.
        TitleLabel.PropertyChanged += OnTitlePropertyChanged;
        _markSpan.PropertyChanged += OnMarkPropertyChanged;
        ShowTitleFont();

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

        // A style found through a dynamic resource stays on a span after the resource is let go of,
        // and nothing public takes it off again without hiding the next one found. So each key gets
        // a mark of its own, which has never held another key's style.
        _markSpan.PropertyChanged -= OnMarkPropertyChanged;
        var mark = new Span { Text = _markSpan.Text };
        mark.PropertyChanged += OnMarkPropertyChanged;
        _markedTitle.Spans[1] = mark;
        _markSpan = mark;

        if (!string.IsNullOrEmpty(key))
        {
            mark.SetDynamicResource(Span.StyleProperty, key);
        }

        ShowTitleFont();
    }

    // What a style and the styles it is based on set.
    private static HashSet<BindableProperty> SetBy(Style? style)
    {
        var set = new HashSet<BindableProperty>();
        for (; style is not null; style = style.BasedOn)
        {
            foreach (var setter in style.Setters)
            {
                if (setter.Property is { } property)
                {
                    set.Add(property);
                }
            }
        }

        return set;
    }

    private void OnTitlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Array.Exists(Font, property => property.PropertyName == e.PropertyName))
        {
            ShowTitleFont();
        }
    }

    private void OnMarkPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == Span.StyleProperty.PropertyName)
        {
            ShowTitleFont();
        }
    }

    // Hands the title's font to the title's span, and to the mark wherever the mark's style says
    // nothing of it. Where the style does say, the mark's own value is cleared, so the style shows.
    private void ShowTitleFont()
    {
        var styled = SetBy(_markSpan.Style);
        foreach (var property in Font)
        {
            var value = TitleLabel.GetValue(property);
            _titleSpan.SetValue(property, value);

            if (styled.Contains(property))
            {
                _markSpan.ClearValue(property);
            }
            else
            {
                _markSpan.SetValue(property, value);
            }
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
