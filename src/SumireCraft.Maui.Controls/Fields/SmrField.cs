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
/// A field marked <see cref="IsRequired"/> puts <see cref="RequiredMark"/> after its title. The mark
/// looks like the title unless a style for <c>Span</c> under the key <c>SmrFieldRequiredMark</c> says
/// otherwise (<see cref="RequiredMarkStyleKey"/>), and a screen reader reads it as drawn unless
/// <see cref="RequiredMarkDescription"/> gives the words to read in its place.
/// </para>
/// <para>
/// The title and the error are labels with the style classes <c>SmrFieldTitle</c> and
/// <c>SmrFieldError</c>, names which <see cref="TitleStyleClass"/> and <see cref="ErrorStyleClass"/>
/// change. A style for either class in App.xaml restyles every field, and one in a
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
/// <smr:SmrField Title="Name" ErrorText="{Binding NameError}"
///               IsRequired="True" RequiredMarkDescription="required">
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

    /// <summary>
    /// Backs the <see cref="IsRequired"/> property.
    /// </summary>
    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(SmrField),
        false,
        propertyChanged: (bindable, _, _) => ((SmrField)bindable).Refresh());

    /// <summary>
    /// Backs the <see cref="RequiredMark"/> property.
    /// </summary>
    public static readonly BindableProperty RequiredMarkProperty = BindableProperty.Create(
        nameof(RequiredMark),
        typeof(string),
        typeof(SmrField),
        "*",
        propertyChanged: (bindable, _, _) => ((SmrField)bindable).Refresh());

    /// <summary>
    /// Backs the <see cref="RequiredMarkDescription"/> property.
    /// </summary>
    public static readonly BindableProperty RequiredMarkDescriptionProperty = BindableProperty.Create(
        nameof(RequiredMarkDescription),
        typeof(string),
        typeof(SmrField),
        propertyChanged: (bindable, _, _) => ((SmrField)bindable).Refresh());

    /// <summary>
    /// Backs the <see cref="RequiredMarkStyleKey"/> property.
    /// </summary>
    public static readonly BindableProperty RequiredMarkStyleKeyProperty = BindableProperty.Create(
        nameof(RequiredMarkStyleKey),
        typeof(string),
        typeof(SmrField),
        "SmrFieldRequiredMark",
        propertyChanged: (bindable, _, _) => ((SmrField)bindable).Refresh());

    /// <summary>
    /// Backs the <see cref="TitleStyleClass"/> property.
    /// </summary>
    public static readonly BindableProperty TitleStyleClassProperty = BindableProperty.Create(
        nameof(TitleStyleClass),
        typeof(string),
        typeof(SmrField),
        "SmrFieldTitle",
        propertyChanged: (bindable, _, _) => ((SmrField)bindable).Refresh());

    /// <summary>
    /// Backs the <see cref="ErrorStyleClass"/> property.
    /// </summary>
    public static readonly BindableProperty ErrorStyleClassProperty = BindableProperty.Create(
        nameof(ErrorStyleClass),
        typeof(string),
        typeof(SmrField),
        "SmrFieldError",
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

    /// <summary>
    /// Gets or sets whether the field is required, which puts <see cref="RequiredMark"/> after its
    /// title. A field without a title shows no mark. <see langword="false"/> by default.
    /// </summary>
    /// <remarks>
    /// The field only shows the mark; whether the input is there is the application's to check, and
    /// its error to give through <see cref="ErrorText"/>. The default template shows the mark; a
    /// template of the application's own shows it for itself, from this and <see cref="RequiredMark"/>,
    /// and the other properties about the mark and the style classes reach the default template
    /// alone.
    /// </remarks>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the mark a required field puts after its title, <c>*</c> by default. It follows the
    /// title as it is, with nothing between them, so a mark that wants a space before it says so:
    /// <c>" (required)"</c>. Null or empty shows none.
    /// </summary>
    public string? RequiredMark
    {
        get => (string?)GetValue(RequiredMarkProperty);
        set => SetValue(RequiredMarkProperty, value);
    }

    /// <summary>
    /// Gets or sets what a screen reader reads in place of the mark, after the title and a space: with
    /// <c>"required"</c>, a field titled Name is read as "Name required". Null by default, which leaves
    /// the title read as it is drawn, mark and all; most screen readers read <c>*</c> as "star" or
    /// "asterisk". The words are the application's, in its own language.
    /// </summary>
    public string? RequiredMarkDescription
    {
        get => (string?)GetValue(RequiredMarkDescriptionProperty);
        set => SetValue(RequiredMarkDescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the resource key of the style for the mark, <c>SmrFieldRequiredMark</c> by default.
    /// Null or empty leaves the mark looking like the title.
    /// </summary>
    /// <remarks>
    /// The mark is a <c>Span</c> after the title's text, and a span takes no style class in .NET MAUI,
    /// so its look is a style for <c>Span</c> under this key, in App.xaml or a page's resources. It is
    /// found the way a dynamic resource is: the nearest one wins, and one added later still reaches
    /// it. With no such style the mark looks like the title it follows.
    /// </remarks>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Style x:Key="SmrFieldRequiredMark" TargetType="Span">
    ///     <Setter Property="TextColor" Value="Red" />
    ///     <Setter Property="FontAttributes" Value="Bold" />
    /// </Style>
    /// ]]></code>
    /// </example>
    public string? RequiredMarkStyleKey
    {
        get => (string?)GetValue(RequiredMarkStyleKeyProperty);
        set => SetValue(RequiredMarkStyleKeyProperty, value);
    }

    /// <summary>
    /// Gets or sets the style class an application styles the title through, <c>SmrFieldTitle</c> by
    /// default. Null or empty gives the title none, and leaves it as the field draws it by default.
    /// </summary>
    /// <remarks>
    /// A type of the application's own derived from the field can set this in its constructor, so that
    /// the application's styles name its own class and no Smr name is left in them. A style for
    /// <see cref="SmrField"/> that sets it reaches fields of that type alone, as an implicit style
    /// does, not those of a derived type, unless it sets <c>ApplyToDerivedTypes</c>.
    /// </remarks>
    public string? TitleStyleClass
    {
        get => (string?)GetValue(TitleStyleClassProperty);
        set => SetValue(TitleStyleClassProperty, value);
    }

    /// <summary>
    /// Gets or sets the style class an application styles the error through, <c>SmrFieldError</c> by
    /// default. Null or empty gives the error none, and leaves it as the field draws it by default.
    /// </summary>
    /// <remarks>
    /// Named the way <see cref="TitleStyleClass"/> is.
    /// </remarks>
    public string? ErrorStyleClass
    {
        get => (string?)GetValue(ErrorStyleClassProperty);
        set => SetValue(ErrorStyleClassProperty, value);
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

        _layout.UseClasses(TitleStyleClass, ErrorStyleClass);
        _layout.UseMarkStyle(RequiredMarkStyleKey);

        var title = string.IsNullOrEmpty(Title) ? null : Title;
        var mark = IsRequired && title is not null && !string.IsNullOrEmpty(RequiredMark) ? RequiredMark : null;
        _layout.ShowTitle(Kept(title, ReserveTitleSpace), mark);
        Reveal(_layout.TitleLabel, title is not null, ReserveTitleSpace);
        Describe(_layout.TitleLabel, title, mark is null ? null : RequiredMarkDescription);

        var error = HasError ? ErrorText : null;
        _layout.ErrorLabel.Text = Kept(error, ReserveErrorSpace);
        Reveal(_layout.ErrorLabel, error is not null, ReserveErrorSpace);
    }

    // The text, or with none, a blank line when its height is to be kept and nothing otherwise. A
    // blank line, not an empty one: an empty label measures no height at all.
    private static string? Kept(string? text, bool keepLine) => text ?? (keepLine ? " " : null);

    // A label shows while it has text or keeps a line. The blank line is there for its height, and a
    // screen reader is not to stop on it.
    private static void Reveal(Label label, bool hasText, bool keepLine)
    {
        label.IsVisible = hasText || keepLine;
        AutomationProperties.SetIsInAccessibleTree(label, hasText);
    }

    // What a screen reader reads of the title: as drawn, unless the application has said what to read
    // in the mark's place.
    private static void Describe(Label label, string? title, string? markDescription)
    {
        if (title is null || string.IsNullOrEmpty(markDescription))
        {
            label.ClearValue(SemanticProperties.DescriptionProperty);
        }
        else
        {
            SemanticProperties.SetDescription(label, $"{title} {markDescription}");
        }
    }
}
