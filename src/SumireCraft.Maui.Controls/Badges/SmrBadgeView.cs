using Microsoft.Maui.Controls.Shapes;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// Puts a small badge over one corner of whatever it wraps.
/// </summary>
/// <remarks>
/// <para>
/// What goes inside is the thing being badged, and everything this control declares describes the
/// badge itself. The properties it inherits keep their usual meaning: <c>HorizontalOptions</c>
/// places the whole badged thing in its own parent, and <c>Shadow</c> is the shadow the whole
/// badged thing casts. Use <see cref="Placement"/> and <see cref="BadgeShadow"/> for the badge.
/// A badge can be shown with nothing inside as well, but it then has no room of its own: it hangs
/// from the corner of an empty space, wherever the layout puts that, and is drawn over whatever
/// lies around it. Placing a badge shown on its own is up to the application, with
/// <see cref="Offset"/> or the layout around it.
/// </para>
/// <para>
/// The corner is the content's corner, not the corner of whatever room this control was given.
/// A button of a set width in a column, or an image centred under a heading, keeps its badge on
/// it, because the badge follows wherever the content's own alignment and size put it.
/// </para>
/// <para>
/// The badge sits over the corner with a quarter of it hanging out past each edge, where
/// Material 3 puts a badge with text. The whole badged thing is still the size of the content
/// alone, which keeps a growing count from pushing anything around it, but the badge needs room to
/// hang into: whatever clips the content, such as the edge of a scroll view or of a cell in a
/// collection view, clips the badge with it.
/// </para>
/// <para>
/// A view laid out after this one, beside it in the same layout, is drawn over the part of the
/// badge that reaches it. Leaving space between them is the plain way out; giving this control a
/// higher <c>ZIndex</c> than its neighbour draws it, badge and all, over the neighbour instead.
/// <c>ZIndex</c> orders the children of one layout only, so it is set on this control where it
/// sits among them, not on the badge.
/// </para>
/// <para>
/// A badge with nothing to say is not drawn. <see cref="Text"/> is whatever should appear, so
/// deciding that ninety-nine hundred should read "99+", or that a zero should show nothing at all,
/// belongs to whoever works out the number rather than to this.
/// <see cref="Badges.BadgeCounter"/> is one such decision, ready made.
/// </para>
/// <para>
/// The badge is a rounded, filled label unless <see cref="BadgeTemplate"/> says otherwise, and the
/// properties that describe that label -- <see cref="Text"/>, <see cref="Fill"/>,
/// <see cref="Stroke"/>, the font and the rest -- apply to it alone. Whatever the badge is drawn
/// with, <see cref="Placement"/>, <see cref="Offset"/>, <see cref="BadgeShadow"/> and
/// <see cref="IsBadgeVisible"/> still place it, cast its shadow and show or hide it.
/// </para>
/// <para>
/// The control template is this control's own. Replacing it takes the badge away with it and
/// leaves every property here without effect; <see cref="BadgeTemplate"/> is how a badge is made to
/// look different.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <smr:SmrBadgeView Text="1" Placement="TopEnd">
///     <Button Text="Inbox" />
/// </smr:SmrBadgeView>
/// ]]></code>
/// </example>
public partial class SmrBadgeView : ContentView
{
    /// <summary>
    /// Backs the <see cref="Text"/> property.
    /// </summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SmrBadgeView),
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="Placement"/> property.
    /// </summary>
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
        nameof(Placement),
        typeof(BadgePlacement),
        typeof(SmrBadgeView),
        defaultValue: BadgePlacement.TopEnd,
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="Offset"/> property.
    /// </summary>
    public static readonly BindableProperty OffsetProperty = BindableProperty.Create(
        nameof(Offset),
        typeof(Point),
        typeof(SmrBadgeView),
        defaultValue: Point.Zero,
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="IsBadgeVisible"/> property.
    /// </summary>
    public static readonly BindableProperty IsBadgeVisibleProperty = BindableProperty.Create(
        nameof(IsBadgeVisible),
        typeof(bool),
        typeof(SmrBadgeView),
        defaultValue: true,
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="BadgeTemplate"/> property.
    /// </summary>
    public static readonly BindableProperty BadgeTemplateProperty = BindableProperty.Create(
        nameof(BadgeTemplate),
        typeof(DataTemplate),
        typeof(SmrBadgeView),
        propertyChanged: OnBadgeTemplateChanged);

    /// <summary>
    /// Backs the <see cref="Fill"/> property.
    /// </summary>
    /// <remarks>
    /// Made per badge rather than shared. A brush is an element in .NET MAUI, so one handed to
    /// every badge in an application would be an element with a great many parents. A static
    /// field would also have to be declared above this one to be anything but null by the time
    /// this line runs, which is a trap not worth leaving lying around.
    /// </remarks>
    public static readonly BindableProperty FillProperty = BindableProperty.Create(
        nameof(Fill),
        typeof(Brush),
        typeof(SmrBadgeView),
        defaultValueCreator: _ => new SolidColorBrush(Color.FromArgb(DefaultFillColor)),
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="Stroke"/> property.
    /// </summary>
    public static readonly BindableProperty StrokeProperty = BindableProperty.Create(
        nameof(Stroke),
        typeof(Brush),
        typeof(SmrBadgeView),
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="StrokeThickness"/> property.
    /// </summary>
    public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(
        nameof(StrokeThickness),
        typeof(double),
        typeof(SmrBadgeView),
        defaultValue: 0d,
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(SmrBadgeView),
        defaultValue: new CornerRadius(12),
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="TextColor"/> property.
    /// </summary>
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(SmrBadgeView),
        defaultValue: Colors.White,
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="FontSize"/> property.
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(SmrBadgeView),
        defaultValue: 12d,
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="FontAttributes"/> property.
    /// </summary>
    public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
        nameof(FontAttributes),
        typeof(FontAttributes),
        typeof(SmrBadgeView),
        defaultValue: FontAttributes.Bold,
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="FontFamily"/> property.
    /// </summary>
    public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
        nameof(FontFamily),
        typeof(string),
        typeof(SmrBadgeView),
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="TextFlowDirection"/> property.
    /// </summary>
    public static readonly BindableProperty TextFlowDirectionProperty = BindableProperty.Create(
        nameof(TextFlowDirection),
        typeof(FlowDirection),
        typeof(SmrBadgeView),
        defaultValue: FlowDirection.LeftToRight,
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="BadgePadding"/> property.
    /// </summary>
    public static readonly BindableProperty BadgePaddingProperty = BindableProperty.Create(
        nameof(BadgePadding),
        typeof(Thickness),
        typeof(SmrBadgeView),
        defaultValue: new Thickness(6, 2),
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// Backs the <see cref="BadgeShadow"/> property.
    /// </summary>
    public static readonly BindableProperty BadgeShadowProperty = BindableProperty.Create(
        nameof(BadgeShadow),
        typeof(Shadow),
        typeof(SmrBadgeView),
        propertyChanged: OnBadgeChanged);

    /// <summary>
    /// The red a notification count is usually drawn in. Dark enough to carry white text.
    /// </summary>
    private const string DefaultFillColor = "#D32F2F";

    /// <summary>
    /// The layout the content and the badge share, once the template has been applied.
    /// </summary>
    private BadgeLayout? _root;

    /// <summary>
    /// Whatever sits in the corner: the built-in label, or what <see cref="BadgeTemplate"/> made.
    /// </summary>
    private View? _badge;

    // The parts of the built-in label, left null while the badge is drawn some other way.
    private Border? _pill;
    private Label? _label;
    private RoundRectangle? _shape;

    /// <summary>
    /// The content whose placement the layout is following, so it can be let go of when the content
    /// is replaced.
    /// </summary>
    private View? _followed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmrBadgeView"/> class.
    /// </summary>
    public SmrBadgeView() => ControlTemplate = new ControlTemplate(BuildRoot);

    /// <summary>
    /// Gets or sets what the badge says. The built-in badge is not drawn with nothing to say.
    /// </summary>
    /// <remarks>
    /// A badge drawn from <see cref="BadgeTemplate"/> shows whatever its template shows, and is
    /// hidden by <see cref="IsBadgeVisible"/> alone.
    /// </remarks>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the badge may be shown at all. <see langword="true"/> by default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the one switch that hides a badge drawn from <see cref="BadgeTemplate"/>, which has
    /// no text to fall silent. The built-in badge is also hidden while <see cref="Text"/> is empty.
    /// </para>
    /// <para>
    /// Named apart from <c>IsVisible</c>, which shows or hides the whole badged thing.
    /// <c>BadgeCounter.IsVisible</c> is meant to be bound here.
    /// </para>
    /// </remarks>
    public bool IsBadgeVisible
    {
        get => (bool)GetValue(IsBadgeVisibleProperty);
        set => SetValue(IsBadgeVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets what the badge is drawn with instead of the built-in label, or
    /// <see langword="null"/> for the label.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A template rather than a view, so that one style can give every badge in an application the
    /// same look: a view set once would be a single element wanted in every badge at the same time.
    /// Its root must be a <see cref="View"/>.
    /// </para>
    /// <para>
    /// The root's <c>HorizontalOptions</c>, <c>VerticalOptions</c>, <c>Margin</c>, <c>Shadow</c>
    /// and <c>IsVisible</c> belong to this control, which sets them from <see cref="Placement"/>,
    /// <see cref="Offset"/>, <see cref="BadgeShadow"/> and <see cref="IsBadgeVisible"/>. A template
    /// that wants them for itself wraps its content in a layout and sets them there.
    /// </para>
    /// <para>
    /// The built-in badge lets a press through to what it marks; a templated one takes presses as
    /// its template says. On Windows a view with a background can keep a press for itself, so a
    /// template that should not leave the corner under it dead sets <c>InputTransparent</c> on its
    /// root.
    /// </para>
    /// </remarks>
    public DataTemplate? BadgeTemplate
    {
        get => (DataTemplate?)GetValue(BadgeTemplateProperty);
        set => SetValue(BadgeTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets which corner the badge sits over. The default is
    /// <see cref="BadgePlacement.TopEnd"/>.
    /// </summary>
    public BadgePlacement Placement
    {
        get => (BadgePlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets how far to move the badge from that corner. The default is no distance at all,
    /// which leaves a quarter of the badge hanging out past each edge of the corner.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A positive X goes towards the end and a positive Y goes down, whatever the corner. The end
    /// is the right in a left-to-right layout and the left in a right-to-left one, so the same
    /// offset turns with the corner it is measured from: a positive X and a negative Y nudge a
    /// badge out past a top-end corner in either direction.
    /// </para>
    /// <para>
    /// Moving the badge never makes the whole thing bigger, in whichever direction and however
    /// far.
    /// </para>
    /// </remarks>
    public Point Offset
    {
        get => (Point)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets what the badge is filled with. The default is a red an application is unlikely
    /// to be surprised by.
    /// </summary>
    /// <remarks>
    /// Named for the shape rather than the view, because <c>Background</c> already means the
    /// background of the whole badged thing.
    /// </remarks>
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// Gets or sets what the badge is outlined with. Nothing by default.
    /// </summary>
    /// <remarks>
    /// An outline in the colour of whatever is behind the badge is how a badge is kept legible
    /// over a busy background.
    /// </remarks>
    public Brush? Stroke
    {
        get => (Brush?)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>
    /// Gets or sets how thick that outline is. The default is zero, so there is none.
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets how rounded the badge's corners are. The default rounds all four enough to
    /// make a short badge a pill.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour of the badge's text. White by default.
    /// </summary>
    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the badge's text.
    /// </summary>
    [System.ComponentModel.TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the badge's text is bold or italic. Bold by default, because a badge
    /// is small and has to be read at a glance.
    /// </summary>
    public FontAttributes FontAttributes
    {
        get => (FontAttributes)GetValue(FontAttributesProperty);
        set => SetValue(FontAttributesProperty, value);
    }

    /// <summary>
    /// Gets or sets the font the badge's text is set in.
    /// </summary>
    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets which way the badge's text runs. Left to right by default, even on a
    /// right-to-left page.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The corner the badge sits over turns with the page, and the text does not. Laid out right to
    /// left, "99+" would read "+99", because a plus sign at the end of a right-to-left paragraph
    /// moves to its other end. A word in a right-to-left script still reads as it should in a
    /// left-to-right badge; only the punctuation at its ends could move.
    /// </para>
    /// <para>
    /// <see cref="FlowDirection.MatchParent"/> lets the text run whichever way the page does. A
    /// badge drawn from <see cref="BadgeTemplate"/> runs as its template says.
    /// </para>
    /// </remarks>
    public FlowDirection TextFlowDirection
    {
        get => (FlowDirection)GetValue(TextFlowDirectionProperty);
        set => SetValue(TextFlowDirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the space between the badge's text and its edge.
    /// </summary>
    /// <remarks>
    /// Named apart from <c>Padding</c>, which is the space inside the whole badged thing.
    /// </remarks>
    public Thickness BadgePadding
    {
        get => (Thickness)GetValue(BadgePaddingProperty);
        set => SetValue(BadgePaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow the badge casts. Nothing by default.
    /// </summary>
    /// <remarks>
    /// Named apart from <c>Shadow</c>, which is the shadow cast by the whole badged thing.
    /// </remarks>
    public Shadow? BadgeShadow
    {
        get => (Shadow?)GetValue(BadgeShadowProperty);
        set => SetValue(BadgeShadowProperty, value);
    }

    /// <summary>
    /// Makes the badge drawn when <see cref="BadgeTemplate"/> is not set.
    /// </summary>
    /// <returns>The view to put in the corner.</returns>
    /// <remarks>
    /// <para>
    /// Override this to give a derived badge a look of its own while leaving
    /// <see cref="BadgeTemplate"/> free for whoever uses it. What the base method makes is the
    /// rounded label that <see cref="Text"/>, <see cref="Fill"/> and the other appearance
    /// properties describe, and those properties keep working on it for as long as it is part of
    /// what the override returns. An override that makes something else of its own is left
    /// alone by them, and is shown or hidden by <see cref="IsBadgeVisible"/> only.
    /// </para>
    /// <para>
    /// The rounded label lets a press through to what it marks. A view of the override's own takes
    /// presses as it was made to, and on Windows one with a background can keep them for itself, so
    /// an override that should not leave the corner under it dead sets <c>InputTransparent</c> on
    /// what it returns.
    /// </para>
    /// <para>
    /// This can be called while the control is still being constructed, before a derived
    /// constructor's body has run, and is called again whenever <see cref="BadgeTemplate"/> is
    /// cleared. Do not rely on state that a derived constructor sets.
    /// </para>
    /// </remarks>
    protected virtual View CreateDefaultBadge()
    {
        _shape = new RoundRectangle();

        _label = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };

        // Transparent to input, so that pressing the badge presses what it marks. Windows otherwise
        // gives the press to the pill, which has a background, and the corner under it goes dead.
        _pill = new Border
        {
            Content = _label,
            StrokeShape = _shape,
            InputTransparent = true,
        };

        return _pill;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == ContentProperty.PropertyName)
        {
            Follow(Content);
        }
    }

    private static void OnBadgeChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((SmrBadgeView)bindable).Apply();

    /// <summary>
    /// Says what decides where the content ends up: whatever it is aligned to, and whether it has
    /// a size of its own.
    /// </summary>
    private static bool AffectsPlacement(string? propertyName) =>
        propertyName == HorizontalOptionsProperty.PropertyName
        || propertyName == VerticalOptionsProperty.PropertyName
        || propertyName == WidthRequestProperty.PropertyName
        || propertyName == HeightRequestProperty.PropertyName
        || propertyName == MaximumWidthRequestProperty.PropertyName
        || propertyName == MaximumHeightRequestProperty.PropertyName;

    /// <summary>
    /// Where .NET MAUI actually puts a view aligned this way.
    /// </summary>
    /// <remarks>
    /// A view told to fill that has a size of its own, or a limit on it, cannot fill, and .NET MAUI
    /// centres it in the room it was given instead.
    /// </remarks>
    private static LayoutOptions Effective(LayoutOptions options, double request, double maximum) =>
        options.Alignment == LayoutAlignment.Fill && (request >= 0 || !double.IsPositiveInfinity(maximum))
            ? LayoutOptions.Center
            : new LayoutOptions(options.Alignment, expands: false);

    /// <summary>
    /// Starts following a new content, and stops following the one before it.
    /// </summary>
    private void Follow(View? content)
    {
        _followed?.PropertyChanged -= OnFollowedPropertyChanged;
        _followed = content;
        content?.PropertyChanged += OnFollowedPropertyChanged;

        PlaceRoot();
    }

    private void OnFollowedPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (AffectsPlacement(e.PropertyName))
        {
            PlaceRoot();
        }
    }

    /// <summary>
    /// Puts the layout where the content would have gone, at the content's size, so that the
    /// corners the badge is placed on are the content's corners.
    /// </summary>
    /// <remarks>
    /// This control stretches like any other view, and the corners of the room it was given are
    /// not the content's whenever the content takes less of it: a button of a set width in a
    /// column, an image centred under a heading. The layout takes over the content's own
    /// alignment, and the content then fills the layout exactly.
    /// </remarks>
    private void PlaceRoot()
    {
        if (_root is null)
        {
            return;
        }

        if (_followed is not { } content)
        {
            _root.HorizontalOptions = LayoutOptions.Fill;
            _root.VerticalOptions = LayoutOptions.Fill;

            return;
        }

        _root.HorizontalOptions = Effective(content.HorizontalOptions, content.WidthRequest, content.MaximumWidthRequest);
        _root.VerticalOptions = Effective(content.VerticalOptions, content.HeightRequest, content.MaximumHeightRequest);
    }

    private static void OnBadgeTemplateChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((SmrBadgeView)bindable).PlaceBadge();

    /// <summary>
    /// Builds a presenter for whatever is being badged, with the badge over it.
    /// </summary>
    /// <remarks>
    /// A template rather than a constructed tree, because the content belongs to whoever wrote the
    /// XAML and a <see cref="ContentPresenter"/> is how .NET MAUI lends it out. The parts are kept
    /// so that a property change can be pushed straight at them; binding each one to the templated
    /// parent would say the same thing at more length and through reflection.
    /// </remarks>
    private BadgeLayout BuildRoot()
    {
        _root = new BadgeLayout { Children = { new ContentPresenter() } };
        PlaceRoot();
        PlaceBadge();

        return _root;
    }

    /// <summary>
    /// Puts a freshly made badge in the corner, taking out whichever one was there before.
    /// </summary>
    private void PlaceBadge()
    {
        if (_root is null)
        {
            return;
        }

        if (_badge is not null)
        {
            _root.Children.Remove(_badge);
        }

        // Forgotten before anything is made, so that a badge drawn another way is never mistaken
        // for the built-in one.
        _pill = null;
        _label = null;
        _shape = null;

        _badge = BadgeTemplate is { } template ? FromTemplate(template) : CreateDefaultBadge();

        // Added after the presenter, so the badge lies over the content rather than beside it.
        _root.Children.Add(_badge);
        Apply();
    }

    private static View FromTemplate(DataTemplate template) =>
        template.CreateContent() as View
        ?? throw new InvalidOperationException(
            $"{nameof(BadgeTemplate)} must make a {nameof(View)} to put in the corner.");

    private void Apply()
    {
        if (_badge is null)
        {
            return;
        }

        ApplyPlacement(_badge);

        if (_pill is not null && _label is not null && _shape is not null)
        {
            ApplyLook(_pill, _label, _shape);
        }
    }

    /// <summary>
    /// Places, shadows and shows or hides the badge, whatever it is drawn with.
    /// </summary>
    private void ApplyPlacement(View badge)
    {
        // The built-in badge has nothing to show without text, and takes up no room then either,
        // so a layout does not shift when a count falls to nothing. Anything else speaks for itself.
        badge.IsVisible = IsBadgeVisible && (_pill is null || !string.IsNullOrEmpty(Text));

        // Null is how a shadow is taken away again, whatever the annotation on the property says.
        badge.Shadow = BadgeShadow!;

        var start = Placement is BadgePlacement.TopStart or BadgePlacement.BottomStart;
        var top = Placement is BadgePlacement.TopStart or BadgePlacement.TopEnd;

        // Start and end rather than left and right: the layout mirrors these for a right-to-left
        // flow direction, which is the whole reason the placements are named that way.
        badge.HorizontalOptions = start ? LayoutOptions.Start : LayoutOptions.End;
        badge.VerticalOptions = top ? LayoutOptions.Start : LayoutOptions.End;

        // A margin on the sides of the corner rather than a translation. The layout mirrors a
        // margin along with the alignment it belongs to when the flow runs right to left, on every
        // platform, so the offset turns with the corner; a translation is mirrored on Windows and
        // left as it is on Android. The layout hangs the badge a quarter out past the corner, and this moves it
        // on from there.
        var offset = Offset;
        badge.Margin = new Thickness(
            left: start ? offset.X : 0,
            top: top ? offset.Y : 0,
            right: start ? 0 : -offset.X,
            bottom: top ? 0 : -offset.Y);
    }

    /// <summary>
    /// Dresses the built-in badge. A badge drawn any other way is left as it was made.
    /// </summary>
    private void ApplyLook(Border pill, Label label, RoundRectangle shape)
    {
        label.Text = Text;
        label.TextColor = TextColor;
        label.FontSize = FontSize;
        label.FontAttributes = FontAttributes;
        // Empty rather than null: .NET MAUI treats both as "whatever the platform uses", and the
        // property it is being assigned to does not admit null.
        label.FontFamily = FontFamily ?? string.Empty;
        // On the label alone, so that the pill and the corner it sits over still turn with the page.
        label.FlowDirection = TextFlowDirection;

        pill.Background = Fill;
        pill.Stroke = Stroke;
        pill.StrokeThickness = StrokeThickness;
        pill.Padding = BadgePadding;
        shape.CornerRadius = CornerRadius;
    }
}
