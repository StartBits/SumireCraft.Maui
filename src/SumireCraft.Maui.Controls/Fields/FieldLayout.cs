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

    /// <summary>
    /// The style class the title carries for an application to style.
    /// </summary>
    internal const string TitleClass = "SmrFieldTitle";

    /// <summary>
    /// The style class the error carries for an application to style.
    /// </summary>
    internal const string ErrorClass = "SmrFieldError";

    public FieldLayout()
    {
        RowDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)];
        RowSpacing = 0;

        // The default classes come first: of two class styles that set the same property, the later
        // class wins, so the application's class replaces the field's own defaults.
        TitleLabel = new Label { StyleClass = [TitleDefaultClass, TitleClass] };
        ErrorLabel = new Label();

        // The application raises this through a weak event manager, so a layout listening to it is
        // still let go of with its page.
        var application = Application.Current;
        application?.RequestedThemeChanged += OnThemeChanged;

        ShowTheme(application?.RequestedTheme ?? AppTheme.Light);

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

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => ShowTheme(e.RequestedTheme);

    private void ShowTheme(AppTheme theme) =>
        ErrorLabel.StyleClass = [ErrorDefaultClass, theme == AppTheme.Dark ? ErrorDarkClass : ErrorLightClass, ErrorClass];
}
