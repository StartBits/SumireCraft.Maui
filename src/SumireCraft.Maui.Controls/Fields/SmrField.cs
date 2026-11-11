namespace SumireCraft.Maui.Controls;

/// <summary>
/// One field of a form: a title above whatever takes the input, and an error below it.
/// </summary>
/// <remarks>
/// <para>
/// What goes inside is the control that takes the input, an <c>Entry</c>, a <c>Picker</c> or any
/// other view. The field reads no validation of its own: the error is whatever text
/// <see cref="ErrorText"/> is given, and <see cref="HasError"/> says whether there is one, for a
/// trigger on the content to follow.
/// </para>
/// <para>
/// The title and the error take the height of their text and the content whatever is left, so a
/// field given a height of its own grows its content. A missing title leaves no row behind, unless
/// <see cref="ReserveTitleSpace"/> keeps a line for it, so that a field without a title lines up with
/// a titled one beside it. A missing error keeps its line by default, so that an error coming and
/// going does not push what lies below the field up and down; <see cref="ReserveErrorSpace"/> set to
/// <see langword="false"/> lets the field close up instead.
/// </para>
/// <para>
/// The title and the error are labels with the style classes <c>SmrFieldTitle</c> and
/// <c>SmrFieldError</c>. A style for either class in App.xaml restyles every field, and one in a
/// page's resources the fields on that page; whatever such a style leaves unset keeps the field's own
/// default. A page's style for a class takes the place of App.xaml's for that class altogether, as
/// with any style in .NET MAUI, so what App.xaml set and the page did not falls back to the field's
/// default too. By default the error is Material 3's error red, in the light theme and the dark. An
/// application that stays light while the device is dark gets the dark theme's paler red, unless it
/// sets <c>UserAppTheme</c> to light or writes the colour plainly in a <c>SmrFieldError</c> style.
/// A template of the application's own takes the place of all of this, as with any
/// <see cref="SmrTemplatedView"/>.
/// </para>
/// <para>
/// A screen reader reads the title, the content and the error in the order they are drawn, and does
/// not stop on the blank line kept for an error. The field writes nothing on the content, so a name
/// or a hint the application gives it stays the application's. An error that appears is read when
/// it is reached, not announced the moment it appears. A template of the application's own is read
/// as it is drawn.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <smr:SmrField Title="Name" ErrorText="{Binding NameError}">
///     <Entry Text="{Binding Name}" />
/// </smr:SmrField>
///
/// <!-- In App.xaml or a page's resources -->
/// <Style TargetType="Label" Class="SmrFieldError">
///     <Setter Property="TextColor" Value="DarkOrange" />
/// </Style>
/// ]]></code>
/// </example>
public class SmrField : SmrTemplatedView
{
    /// <summary>
    /// Backs the <see cref="Title"/> property.
    /// </summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(SmrField),
        propertyChanged: (bindable, _, _) => ((SmrField)bindable).Refresh());

    /// <summary>
    /// Backs the <see cref="ErrorText"/> property.
    /// </summary>
    public static readonly BindableProperty ErrorTextProperty = BindableProperty.Create(
        nameof(ErrorText),
        typeof(string),
        typeof(SmrField),
        propertyChanged: OnErrorTextChanged);

    private static readonly BindablePropertyKey HasErrorPropertyKey = BindableProperty.CreateReadOnly(
        nameof(HasError),
        typeof(bool),
        typeof(SmrField),
        false);

    /// <summary>
    /// Backs the <see cref="HasError"/> property.
    /// </summary>
    public static readonly BindableProperty HasErrorProperty = HasErrorPropertyKey.BindableProperty;

    /// <summary>
    /// Backs the <see cref="ReserveErrorSpace"/> property.
    /// </summary>
    public static readonly BindableProperty ReserveErrorSpaceProperty = BindableProperty.Create(
        nameof(ReserveErrorSpace),
        typeof(bool),
        typeof(SmrField),
        true,
        propertyChanged: (bindable, _, _) => ((SmrField)bindable).Refresh());

    /// <summary>
    /// Backs the <see cref="ReserveTitleSpace"/> property.
    /// </summary>
    public static readonly BindableProperty ReserveTitleSpaceProperty = BindableProperty.Create(
        nameof(ReserveTitleSpace),
        typeof(bool),
        typeof(SmrField),
        false,
        propertyChanged: (bindable, _, _) => ((SmrField)bindable).Refresh());

    private static readonly ControlTemplate DefaultTemplate = new(() => new FieldLayout());

    private FieldLayout? _layout;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmrField"/> class.
    /// </summary>
    public SmrField()
        : base(DefaultTemplate)
    {
        foreach (var style in FieldDefaults.Instance.Styles)
        {
            AddDefault(style);
        }
    }

    /// <summary>
    /// Gets or sets the title shown above the content. Null or empty shows no title, and leaves no row
    /// for it unless <see cref="ReserveTitleSpace"/> keeps a blank line.
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the error shown below the content. Null or empty means there is none.
    /// </summary>
    public string? ErrorText
    {
        get => (string?)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>
    /// Gets whether <see cref="ErrorText"/> holds an error, for a trigger on the content to follow.
    /// </summary>
    public bool HasError => (bool)GetValue(HasErrorProperty);

    /// <summary>
    /// Gets or sets whether the error keeps a line's height when there is none, so that an error coming
    /// and going does not move what lies below. <see langword="true"/> by default.
    /// </summary>
    public bool ReserveErrorSpace
    {
        get => (bool)GetValue(ReserveErrorSpaceProperty);
        set => SetValue(ReserveErrorSpaceProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the title keeps a line's height when there is none, so that a field
    /// without one lines up with a titled field beside it. <see langword="false"/> by default.
    /// </summary>
    public bool ReserveTitleSpace
    {
        get => (bool)GetValue(ReserveTitleSpaceProperty);
        set => SetValue(ReserveTitleSpaceProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _layout = ((IVisualTreeElement)this).GetVisualChildren() is [FieldLayout layout, ..] ? layout : null;
        Refresh();
    }

    private static void OnErrorTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var field = (SmrField)bindable;
        field.SetValue(HasErrorPropertyKey, !string.IsNullOrEmpty((string?)newValue));
        field.Refresh();
    }

    // Shows the title and the error in the default template. A template of the application's own
    // shows them for itself.
    private void Refresh()
    {
        if (_layout is null)
        {
            return;
        }

        var title = Title;
        Show(_layout.TitleLabel, string.IsNullOrEmpty(title) ? null : title, ReserveTitleSpace);
        Show(_layout.ErrorLabel, HasError ? ErrorText : null, ReserveErrorSpace);
    }

    // Shows the text, or with none, a blank line when its height is to be kept and nothing
    // otherwise. A blank line, not an empty one: an empty label measures no height at all. The blank
    // line is there for its height, and a screen reader is not to stop on it.
    private static void Show(Label label, string? text, bool keepLine)
    {
        label.Text = text ?? (keepLine ? " " : null);
        label.IsVisible = text is not null || keepLine;
        AutomationProperties.SetIsInAccessibleTree(label, text is not null);
    }
}
