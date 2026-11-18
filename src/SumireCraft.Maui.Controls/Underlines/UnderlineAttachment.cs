namespace SumireCraft.Maui.Controls;

/// <summary>
/// One underlined input: works out its line as its text colour, its focus and the attached
/// properties change, and hands it to the platform view the input has now.
/// </summary>
/// <remarks>
/// How a platform draws the line is in this type's other part for that platform. Each platform's
/// input is a native control with a box of its own, and nothing in .NET MAUI takes that box away
/// or draws under the text instead, so this is one of the few places the library reaches the
/// platform (Platform-specific code in CLAUDE.md). The plain net10.0 build has no platform, and only
/// works the line out.
/// </remarks>
internal sealed partial class UnderlineAttachment
{
    private readonly View _view;

    private object? _platformView;

    /// <summary>
    /// Starts underlining the input, on the platform view it has now if it has one.
    /// </summary>
    public UnderlineAttachment(View view)
    {
        _view = view;
        _view.PropertyChanged += OnPropertyChanged;
        _view.HandlerChanging += OnHandlerChanging;
        _view.HandlerChanged += OnHandlerChanged;

        // The application raises this through a weak event manager, so listening to it keeps
        // nothing alive.
        if (Application.Current is { } application)
        {
            application.RequestedThemeChanged += OnThemeChanged;
        }

        Look = Compute();
        _platformView = _view.Handler?.PlatformView;
        if (_platformView is not null)
        {
            Apply(_platformView, Look);
        }
    }

    /// <summary>
    /// Gets the line the input is drawn with now.
    /// </summary>
    public UnderlineLook Look { get; private set; }

    /// <summary>
    /// Stops underlining the input, and gives its platform view its own look back.
    /// </summary>
    public void Detach()
    {
        _view.PropertyChanged -= OnPropertyChanged;
        _view.HandlerChanging -= OnHandlerChanging;
        _view.HandlerChanged -= OnHandlerChanged;
        if (Application.Current is { } application)
        {
            application.RequestedThemeChanged -= OnThemeChanged;
        }

        if (_platformView is not null)
        {
            Restore(_platformView);
            _platformView = null;
        }
    }

    /// <summary>
    /// Works the line out again, and draws it if it changed, or whatever it is when told to: a
    /// platform may use an attached property the line itself does not carry, such as the thickness
    /// it will take on focus.
    /// </summary>
    public void Refresh(bool always = false)
    {
        var look = Compute();
        if (look == Look && !always)
        {
            return;
        }

        Look = look;
        if (_platformView is not null)
        {
            Apply(_platformView, look);
        }
    }

    // The colour of its own or the text's, and the thickness for whether the input has the focus.
    private UnderlineLook Compute() => new(
        SmrUnderline.GetColor(_view) ?? (_view as ITextStyle)?.TextColor,
        _view.IsFocused ? SmrUnderline.GetFocusedThickness(_view) : SmrUnderline.GetThickness(_view));

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextStyle.TextColor):
            case nameof(VisualElement.IsFocused):
                Refresh();
                break;
            case nameof(VisualElement.Width):
            case nameof(VisualElement.Height):
            case nameof(VisualElement.Background):
            case nameof(VisualElement.BackgroundColor):
                // What a platform draws the line from may have changed under it.
                if (_platformView is not null)
                {
                    Redraw(_platformView, Look);
                }

                break;
        }
    }

    // A line in the platform's own text colour is that colour in the theme it was drawn in: draw it
    // again in the new one, once the platform has taken the new theme on.
    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (Look.Color is null)
        {
            _view.Dispatcher.Dispatch(() =>
            {
                if (_platformView is not null)
                {
                    Apply(_platformView, Look);
                }
            });
        }
    }

    private void OnHandlerChanging(object? sender, HandlerChangingEventArgs e)
    {
        if (_platformView is not null)
        {
            Restore(_platformView);
            _platformView = null;
        }
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        _platformView = _view.Handler?.PlatformView;
        if (_platformView is not null)
        {
            Apply(_platformView, Look);
        }
    }

    // Each of these is implemented by a platform's part of this type, and left out on plain net10.0,
    // where there is no platform: C# allows that only in a partial method's original form, which
    // takes no accessibility modifier.

    // Draws the line on the platform view, the first time or after it changed.
    partial void Apply(object platformView, UnderlineLook look);

    // Draws the line again after the input changed size or background, as the platform needs.
    partial void Redraw(object platformView, UnderlineLook look);

    // Gives the platform view back the look it had before it was underlined.
    partial void Restore(object platformView);
}
