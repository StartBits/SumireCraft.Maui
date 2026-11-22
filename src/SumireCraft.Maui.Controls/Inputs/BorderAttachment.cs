namespace SumireCraft.Maui.Controls;

/// <summary>
/// One input whose box is the library's: works out its border as its text colour and border
/// properties change, and hands it to the platform view the input has now.
/// </summary>
/// <remarks>
/// How a platform draws the border is in this type's other part for that platform. Each platform's
/// input is a native control with a box of its own, and nothing in .NET MAUI takes that box away or
/// draws another, so this is one of the few places the library reaches the platform
/// (Platform-specific code in CLAUDE.md). The plain net10.0 build has no platform, and only works the
/// border out.
/// </remarks>
internal sealed partial class BorderAttachment
{
    private readonly View _view;

    private object? _platformView;

    /// <summary>
    /// Starts drawing the input's border, on the platform view it has now if it has one.
    /// </summary>
    public BorderAttachment(View view)
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

        Look = InputBorder.LookOf(_view) ?? default;
        _platformView = _view.Handler?.PlatformView;
        if (_platformView is not null)
        {
            Apply(_platformView, Look);
        }
    }

    /// <summary>
    /// Gets the border the input is drawn with now.
    /// </summary>
    public BorderLook Look { get; private set; }

    /// <summary>
    /// Stops drawing the border, and gives the platform view its own box back.
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
    /// Works the border out again, and draws it if it changed.
    /// </summary>
    public void Refresh()
    {
        if (InputBorder.LookOf(_view) is not { } look || look == Look)
        {
            return;
        }

        Look = look;
        if (_platformView is not null)
        {
            Apply(_platformView, look);
        }
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextStyle.TextColor):
                Refresh();
                break;
            case nameof(VisualElement.Width):
            case nameof(VisualElement.Height):
            case nameof(VisualElement.Background):
            case nameof(VisualElement.BackgroundColor):
            case nameof(VisualElement.FlowDirection):
                // What a platform draws the border from may have changed under it.
                if (_platformView is not null)
                {
                    Redraw(_platformView, Look);
                }

                break;
        }
    }

    // A border in the platform's own text colour is that colour in the theme it was drawn in: draw it
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

    // Each of these is implemented by a platform's part of this type where the platform needs it, and
    // left out on plain net10.0, where there is no platform: C# allows that only in a partial method's
    // original form, which takes no accessibility modifier.

    // Draws the border on the platform view, the first time or after it changed.
    partial void Apply(object platformView, BorderLook look);

    // Draws the border again after the input changed size or background, as the platform needs.
    partial void Redraw(object platformView, BorderLook look);

    // Gives the platform view back the box it had before the border was drawn.
    partial void Restore(object platformView);
}
