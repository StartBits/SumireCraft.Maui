using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.UnitTests.Badges;

// What the control builds and how its properties reach it. Where the badge then lands on a screen
// is measured by the UI tests.
public class SmrBadgeViewTests
{
    [Fact]
    public void TheBadge_LiesOverTheContentRatherThanBesideIt()
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button() };

        var root = Root(badge);

        Assert.Equal(2, root.Children.Count);
        Assert.IsType<ContentPresenter>(root.Children[0]);
        Assert.Same(Corner(badge), root.Children[1]);
    }

    // The layout the badge sits in follows what it marks, so that the corner it is put in is the
    // content's corner rather than the corner of whatever room the control was given. The control
    // itself stretches like any other view; inside it the layout takes the place the content would
    // have taken.

    [Fact]
    public void AStretchingContent_LetsTheLayoutStretchWithIt()
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button() };

        Assert.Equal(LayoutAlignment.Fill, Root(badge).HorizontalOptions.Alignment);
        Assert.Equal(LayoutAlignment.Fill, Root(badge).VerticalOptions.Alignment);
    }

    [Fact]
    public void NoContent_LeavesTheLayoutStretching()
    {
        var badge = new SmrBadgeView { Text = "7" };

        Assert.Equal(LayoutAlignment.Fill, Root(badge).HorizontalOptions.Alignment);
        Assert.Equal(LayoutAlignment.Fill, Root(badge).VerticalOptions.Alignment);
    }

    /// <remarks>
    /// .NET MAUI centres a view that is told to fill but has a size of its own, and the layout has to
    /// end up where that view does.
    /// </remarks>
    [Fact]
    public void AContentOfASetWidth_DrawsTheLayoutInToTheMiddle()
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button { WidthRequest = 160 } };

        Assert.Equal(LayoutAlignment.Center, Root(badge).HorizontalOptions.Alignment);
        Assert.Equal(LayoutAlignment.Fill, Root(badge).VerticalOptions.Alignment);
    }

    [Fact]
    public void AContentOfASetHeight_DrawsTheLayoutInToTheMiddle()
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button { HeightRequest = 72 } };

        Assert.Equal(LayoutAlignment.Fill, Root(badge).HorizontalOptions.Alignment);
        Assert.Equal(LayoutAlignment.Center, Root(badge).VerticalOptions.Alignment);
    }

    [Fact]
    public void AContentWithAMaximumSize_DrawsTheLayoutInToTheMiddle()
    {
        var badge = new SmrBadgeView
        {
            Text = "7",
            Content = new Button { MaximumWidthRequest = 200, MaximumHeightRequest = 80 },
        };

        Assert.Equal(LayoutAlignment.Center, Root(badge).HorizontalOptions.Alignment);
        Assert.Equal(LayoutAlignment.Center, Root(badge).VerticalOptions.Alignment);
    }

    [Theory]
    [InlineData(LayoutAlignment.Start)]
    [InlineData(LayoutAlignment.Center)]
    [InlineData(LayoutAlignment.End)]
    public void AContentAlignedSomewhere_TakesTheLayoutThere(LayoutAlignment alignment)
    {
        var options = new LayoutOptions(alignment, expands: false);
        var badge = new SmrBadgeView
        {
            Text = "7",
            Content = new Button { WidthRequest = 160, HorizontalOptions = options, VerticalOptions = options },
        };

        Assert.Equal(alignment, Root(badge).HorizontalOptions.Alignment);
        Assert.Equal(alignment, Root(badge).VerticalOptions.Alignment);
    }

    [Fact]
    public void AContentThatChangesHowItIsPlaced_TakesTheLayoutAlong()
    {
        var content = new Button();
        var badge = new SmrBadgeView { Text = "7", Content = content };

        content.WidthRequest = 160;
        Assert.Equal(LayoutAlignment.Center, Root(badge).HorizontalOptions.Alignment);

        content.HorizontalOptions = LayoutOptions.End;
        Assert.Equal(LayoutAlignment.End, Root(badge).HorizontalOptions.Alignment);

        content.VerticalOptions = LayoutOptions.Start;
        Assert.Equal(LayoutAlignment.Start, Root(badge).VerticalOptions.Alignment);
    }

    [Fact]
    public void ReplacingTheContent_FollowsTheNewOneAndForgetsTheOld()
    {
        var old = new Button();
        var badge = new SmrBadgeView { Text = "7", Content = old };

        badge.Content = new Button { HorizontalOptions = LayoutOptions.Start };
        old.HorizontalOptions = LayoutOptions.End;

        Assert.Equal(LayoutAlignment.Start, Root(badge).HorizontalOptions.Alignment);
    }

    /// <remarks>
    /// A view taken out of a badge can live on somewhere else, and must not keep the badge alive
    /// there by still telling it about every change.
    /// </remarks>
    [Fact]
    public void ReplacedContent_DoesNotKeepTheBadgeAlive()
    {
        var old = new Button();

        var badge = BadgeThatLetGoOf(old);
        ForceCollection();

        Assert.False(badge.TryGetTarget(out _));
        GC.KeepAlive(old);
    }

    [Fact]
    public void TheBuiltInBadge_SaysItsText()
    {
        var badge = new SmrBadgeView { Text = "7" };

        Assert.Equal("7", Label(badge).Text);
        Assert.True(Corner(badge).IsVisible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TheBuiltInBadge_IsNotDrawnWithNothingToSay(string? text)
    {
        var badge = new SmrBadgeView { Text = "7" };

        badge.Text = text;

        Assert.False(Corner(badge).IsVisible);
    }

    [Fact]
    public void IsBadgeVisible_HidesTheBadgeWhateverItSays()
    {
        var badge = new SmrBadgeView { Text = "7" };

        badge.IsBadgeVisible = false;

        Assert.False(Corner(badge).IsVisible);
    }

    [Theory]
    [InlineData(BadgePlacement.TopEnd, false, true)]
    [InlineData(BadgePlacement.TopStart, true, true)]
    [InlineData(BadgePlacement.BottomEnd, false, false)]
    [InlineData(BadgePlacement.BottomStart, true, false)]
    public void ThePlacement_PutsTheBadgeInItsCorner(BadgePlacement placement, bool start, bool top)
    {
        var badge = new SmrBadgeView { Text = "7", Placement = placement };

        var corner = Corner(badge);

        Assert.Equal(start ? LayoutOptions.Start : LayoutOptions.End, corner.HorizontalOptions);
        Assert.Equal(top ? LayoutOptions.Start : LayoutOptions.End, corner.VerticalOptions);
    }

    [Fact]
    public void ANewBadge_SitsInTheTopEndCorner()
    {
        var corner = Corner(new SmrBadgeView { Text = "7" });

        Assert.Equal(LayoutOptions.End, corner.HorizontalOptions);
        Assert.Equal(LayoutOptions.Start, corner.VerticalOptions);
    }

    /// <remarks>
    /// A margin on the sides of the corner rather than a translation. The layout mirrors a margin
    /// together with the alignment it belongs to when the flow runs right to left, on every
    /// platform; a translation is mirrored on Windows and left alone on Android. Where the badge
    /// then lands on a screen, in either direction, is for the UI tests.
    /// </remarks>
    [Theory]
    [InlineData(BadgePlacement.TopEnd, 0, -8, -8, 0)]
    [InlineData(BadgePlacement.TopStart, 8, -8, 0, 0)]
    [InlineData(BadgePlacement.BottomEnd, 0, 0, -8, 8)]
    [InlineData(BadgePlacement.BottomStart, 8, 0, 0, 8)]
    public void TheOffset_IsAMarginOnTheSidesOfTheCorner(
        BadgePlacement placement, double left, double top, double right, double bottom)
    {
        var badge = new SmrBadgeView { Text = "7", Placement = placement };

        badge.Offset = new Point(8, -8);

        var corner = Corner(badge);
        Assert.Equal(new Thickness(left, top, right, bottom), corner.Margin);
        Assert.Equal(0, corner.TranslationX);
        Assert.Equal(0, corner.TranslationY);
    }

    // Where the badge is laid out, measured with stand-ins that report a size for the content and
    // for the badge. The content is 100 by 40 and the badge 20 by 12 unless a test says otherwise,
    // so a quarter of the badge is 5 across and 3 down.

    /// <remarks>
    /// A quarter out and three quarters over, the way Material 3 places a badge with text: 4dp of
    /// a 16dp badge hang out past each edge of its corner.
    /// </remarks>
    [Theory]
    [InlineData(BadgePlacement.TopEnd, 85, -3)]
    [InlineData(BadgePlacement.TopStart, -5, -3)]
    [InlineData(BadgePlacement.BottomEnd, 85, 31)]
    [InlineData(BadgePlacement.BottomStart, -5, 31)]
    public void TheBadge_HangsAQuarterOutPastItsCorner(BadgePlacement placement, double x, double y)
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button(), Placement = placement };

        LayOut(badge, new Size(100, 40), new Size(20, 12));

        Assert.Equal(new Rect(x, y, 20, 12), Corner(badge).Frame);
    }

    /// <remarks>
    /// Out past the corner by more than the badge is big, and in from it by more than that, are
    /// both here: the badge's own size must not be lost under a margin larger than it.
    /// </remarks>
    [Theory]
    [InlineData(BadgePlacement.TopEnd, 8, -8, 93, -11)]
    [InlineData(BadgePlacement.TopEnd, 30, -30, 115, -33)]
    [InlineData(BadgePlacement.TopEnd, -30, 30, 55, 27)]
    [InlineData(BadgePlacement.BottomStart, 3, 4, -2, 35)]
    public void TheOffset_MovesTheBadgeOnFromThere(
        BadgePlacement placement, double offsetX, double offsetY, double x, double y)
    {
        var badge = new SmrBadgeView
        {
            Text = "7",
            Content = new Button(),
            Placement = placement,
            Offset = new Point(offsetX, offsetY),
        };

        LayOut(badge, new Size(100, 40), new Size(20, 12));

        Assert.Equal(new Rect(x, y, 20, 12), Corner(badge).Frame);
    }

    /// <remarks>
    /// A badge wider than what it marks still takes its own size, and the whole thing is still the
    /// size of what it marks, so a count growing never pushes anything around it.
    /// </remarks>
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-50, 50)]
    public void TheBadge_NeverChangesTheSizeOfTheWhole(double offsetX, double offsetY)
    {
        var badge = new SmrBadgeView { Text = "99+", Content = new Button(), Offset = new Point(offsetX, offsetY) };

        var size = LayOut(badge, new Size(10, 10), new Size(40, 20));

        Assert.Equal(new Size(10, 10), size);
        Assert.Equal(new Size(40, 20), Corner(badge).Frame.Size);
    }

    [Fact]
    public void TheContent_FillsTheWhole()
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button() };

        LayOut(badge, new Size(100, 40), new Size(20, 12));

        Assert.Equal(new Rect(0, 0, 100, 40), ((View)Root(badge).Children[0]).Frame);
    }

    [Fact]
    public void TheBadgeShadow_IsCastByTheBadgeAndCanBeTakenAway()
    {
        var shadow = new Shadow { Radius = 8 };
        var badge = new SmrBadgeView { Text = "7", BadgeShadow = shadow };

        Assert.Same(shadow, Corner(badge).Shadow);
        Assert.Null(badge.Shadow);

        badge.BadgeShadow = null;

        Assert.Null(Corner(badge).Shadow);
    }

    [Fact]
    public void TheDefaultLook_IsAWhiteOnRedPill()
    {
        var badge = new SmrBadgeView { Text = "7" };

        var pill = Assert.IsType<Border>(Corner(badge));
        var label = Label(badge);

        Assert.Equal(Color.FromArgb("#D32F2F"), Assert.IsType<SolidColorBrush>(pill.Background).Color);
        Assert.Null(pill.Stroke);
        Assert.Equal(0, pill.StrokeThickness);
        Assert.Equal(new Thickness(6, 2), pill.Padding);
        Assert.Equal(new CornerRadius(12), Assert.IsType<RoundRectangle>(pill.StrokeShape).CornerRadius);
        Assert.Equal(Colors.White, label.TextColor);
        Assert.Equal(12, label.FontSize);
        Assert.Equal(FontAttributes.Bold, label.FontAttributes);
        Assert.Equal(string.Empty, label.FontFamily);
    }

    [Fact]
    public void TheLook_IsWhatTheBadgeIsTold()
    {
        var fill = new SolidColorBrush(Colors.MidnightBlue);
        var stroke = new SolidColorBrush(Colors.White);
        var badge = new SmrBadgeView { Text = "NEW" };

        badge.Fill = fill;
        badge.Stroke = stroke;
        badge.StrokeThickness = 2;
        badge.CornerRadius = new CornerRadius(12, 15, 18, 20);
        badge.BadgePadding = new Thickness(4);
        badge.TextColor = Colors.Yellow;
        badge.FontSize = 20;
        badge.FontAttributes = FontAttributes.Italic;
        badge.FontFamily = "OpenSansRegular";

        var pill = Assert.IsType<Border>(Corner(badge));
        var label = Label(badge);

        Assert.Same(fill, pill.Background);
        Assert.Same(stroke, pill.Stroke);
        Assert.Equal(2, pill.StrokeThickness);
        Assert.Equal(new Thickness(4), pill.Padding);
        Assert.Equal(new CornerRadius(12, 15, 18, 20), Assert.IsType<RoundRectangle>(pill.StrokeShape).CornerRadius);
        Assert.Equal(Colors.Yellow, label.TextColor);
        Assert.Equal(20, label.FontSize);
        Assert.Equal(FontAttributes.Italic, label.FontAttributes);
        Assert.Equal("OpenSansRegular", label.FontFamily);
    }

    [Fact]
    public void TheLook_DoesNotSpillOntoTheWholeBadgedThing()
    {
        var badge = new SmrBadgeView
        {
            Text = "7",
            Fill = new SolidColorBrush(Colors.MidnightBlue),
            BadgePadding = new Thickness(4),
        };

        Assert.NotEqual(Colors.MidnightBlue, (badge.Background as SolidColorBrush)?.Color);
        Assert.Equal(new Thickness(0), badge.Padding);
    }

    [Fact]
    public void TheText_ReadsLeftToRightByDefault()
    {
        var badge = new SmrBadgeView { Text = "99+" };

        Assert.Equal(FlowDirection.LeftToRight, badge.TextFlowDirection);
        Assert.Equal(FlowDirection.LeftToRight, Label(badge).FlowDirection);
    }

    /// <remarks>
    /// The corner turns with the page and the text does not: right to left, "99+" would otherwise
    /// be laid out as "+99", the plus being a neutral character that follows the paragraph.
    /// </remarks>
    [Fact]
    public void InARightToLeftPage_TheCornerTurnsAndTheTextDoesNot()
    {
        var badge = new SmrBadgeView { Text = "99+", FlowDirection = FlowDirection.RightToLeft };

        Assert.Equal(FlowDirection.RightToLeft, Effective(Root(badge)));
        Assert.Equal(FlowDirection.LeftToRight, Effective(Label(badge)));
    }

    [Fact]
    public void TextFlowDirection_MatchParent_LetsTheTextFollowThePage()
    {
        var badge = new SmrBadgeView { Text = "99+", FlowDirection = FlowDirection.RightToLeft };

        badge.TextFlowDirection = FlowDirection.MatchParent;

        Assert.Equal(FlowDirection.MatchParent, Label(badge).FlowDirection);
        Assert.Equal(FlowDirection.RightToLeft, Effective(Label(badge)));
    }

    [Fact]
    public void TextFlowDirection_RightToLeft_TurnsTheTextInALeftToRightPage()
    {
        var badge = new SmrBadgeView { Text = "99+" };

        badge.TextFlowDirection = FlowDirection.RightToLeft;

        Assert.Equal(FlowDirection.RightToLeft, Effective(Label(badge)));
        Assert.Equal(FlowDirection.LeftToRight, Effective(Root(badge)));
    }

    // On Windows a view with a background takes a press for itself, so a pill over a button's corner
    // left that corner dead. Android passes a press on through a view that does not answer it.

    [Fact]
    public void TheBuiltInBadge_LetsAPressThroughToWhatItMarks()
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button() };

        Assert.True(Corner(badge).InputTransparent);
    }

    [Fact]
    public void ATemplatedBadge_TakesPressesAsItsTemplateSays()
    {
        var badge = new SmrBadgeView { Content = new Button(), BadgeTemplate = Dot() };

        Assert.False(Corner(badge).InputTransparent);

        badge.BadgeTemplate = new DataTemplate(() => new BoxView { InputTransparent = true });

        Assert.True(Corner(badge).InputTransparent);
    }

    [Fact]
    public void ATemplatedBadge_KeepsItsOwnFlowDirection()
    {
        var badge = new SmrBadgeView
        {
            FlowDirection = FlowDirection.RightToLeft,
            TextFlowDirection = FlowDirection.LeftToRight,
            BadgeTemplate = new DataTemplate(() => new Label { Text = "●" }),
        };

        Assert.Equal(FlowDirection.MatchParent, Corner(badge).FlowDirection);
    }

    [Fact]
    public void ABadgeTemplate_PutsItsViewInTheCornerInsteadOfTheLabel()
    {
        var badge = new SmrBadgeView { Text = "7", Content = new Button() };

        badge.BadgeTemplate = Dot();

        Assert.IsType<BoxView>(Corner(badge));
        Assert.Equal(2, Root(badge).Children.Count);
        Assert.Empty(Descendants(badge).OfType<Label>());
    }

    [Fact]
    public void ATemplatedBadge_IsPlacedLikeAnyOther()
    {
        var shadow = new Shadow();
        var badge = new SmrBadgeView
        {
            BadgeTemplate = Dot(),
            Placement = BadgePlacement.BottomStart,
            Offset = new Point(3, 4),
            BadgeShadow = shadow,
        };

        var corner = Corner(badge);

        Assert.Equal(LayoutOptions.Start, corner.HorizontalOptions);
        Assert.Equal(LayoutOptions.End, corner.VerticalOptions);
        Assert.Equal(new Thickness(3, 0, 0, -4), corner.Margin);
        Assert.Same(shadow, corner.Shadow);
    }

    [Fact]
    public void ATemplatedBadge_IsShownWithoutTextAndHiddenByIsBadgeVisibleAlone()
    {
        var badge = new SmrBadgeView { BadgeTemplate = Dot() };

        Assert.True(Corner(badge).IsVisible);

        badge.IsBadgeVisible = false;

        Assert.False(Corner(badge).IsVisible);
    }

    [Fact]
    public void ATemplatedBadge_KeepsTheLookItWasMadeWith()
    {
        var badge = new SmrBadgeView { BadgeTemplate = Dot() };

        badge.Fill = new SolidColorBrush(Colors.MidnightBlue);
        badge.Text = "7";

        Assert.Equal(Colors.Red, Assert.IsType<BoxView>(Corner(badge)).Color);
    }

    [Fact]
    public void ClearingTheTemplate_BringsTheLabelBackAsItWasDescribed()
    {
        var badge = new SmrBadgeView
        {
            Text = "7",
            TextColor = Colors.Yellow,
            BadgeTemplate = Dot(),
        };

        badge.BadgeTemplate = null;

        Assert.IsType<Border>(Corner(badge));
        Assert.Equal("7", Label(badge).Text);
        Assert.Equal(Colors.Yellow, Label(badge).TextColor);
        Assert.Equal(2, Root(badge).Children.Count);
    }

    [Fact]
    public void ATemplateThatMakesNoView_IsRejected()
    {
        var badge = new SmrBadgeView();

        Assert.Throws<InvalidOperationException>(() =>
            badge.BadgeTemplate = new DataTemplate(() => new SolidColorBrush(Colors.Red)));
    }

    [Fact]
    public void ADerivedBadge_DrawsWhatItsOverrideMakes()
    {
        var badge = new AbcBadgeView { Text = "7" };

        var corner = Assert.IsType<BoxView>(Corner(badge));

        Assert.Equal(Colors.Green, corner.Color);
        Assert.True(corner.IsVisible);
    }

    [Fact]
    public void ADerivedBadge_StillGivesWayToABadgeTemplate()
    {
        var badge = new AbcBadgeView { BadgeTemplate = Dot() };

        Assert.Equal(Colors.Red, Assert.IsType<BoxView>(Corner(badge)).Color);
    }

    [Fact]
    public void ADerivedBadge_ChangesTheDefaultsInItsConstructor()
    {
        var badge = new AbcCornerBadgeView { Text = "7" };

        Assert.Equal(LayoutOptions.Start, Corner(badge).HorizontalOptions);
        Assert.Equal(Colors.Yellow, Label(badge).TextColor);
    }

    private static DataTemplate Dot() => new(() => new BoxView { Color = Colors.Red });

    /// <summary>
    /// Wraps <paramref name="content"/> in a badge, swaps it out for another view, and lets go of
    /// the badge. Kept in its own non-inlined method so no local in the caller keeps it alive.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<SmrBadgeView> BadgeThatLetGoOf(View content)
    {
        var badge = new SmrBadgeView { Text = "7", Content = content };
        badge.Content = new Button();

        return new WeakReference<SmrBadgeView>(badge);
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Lays the badge out the way a platform would, in exactly the room the content needs, with the
    /// content and the badge each reporting the size given here.
    /// </summary>
    /// <returns>The size the whole thing asked for.</returns>
    private static Size LayOut(SmrBadgeView badge, Size content, Size corner)
    {
        var root = Root(badge);
        var presenter = (View)root.Children[0];

        // The presenter and what it presents both report the content's size, so that this holds
        // whichever of them the layout ends up asking.
        presenter.Handler = new SizedHandler(content);
        badge.Content.Handler = new SizedHandler(content);
        Corner(badge).Handler = new SizedHandler(corner);

        var layout = (ICrossPlatformLayout)root;
        var size = layout.CrossPlatformMeasure(content.Width, content.Height);
        layout.CrossPlatformArrange(new Rect(Point.Zero, size));

        return size;
    }

    /// <summary>
    /// The layout the control template made, holding the content and the badge.
    /// </summary>
    private static Layout Root(SmrBadgeView badge) =>
        Assert.Single(((IVisualTreeElement)badge).GetVisualChildren().OfType<Layout>());

    /// <summary>
    /// Whatever sits in the corner: added last, so that it lies over the content.
    /// </summary>
    private static View Corner(SmrBadgeView badge) => (View)Root(badge).Children[^1];

    /// <summary>
    /// Which way an element actually runs, once its own setting and its parents' are taken together.
    /// </summary>
    private static FlowDirection Effective(VisualElement element) =>
        ((IVisualElementController)element).EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.RightToLeft)
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;

    private static Label Label(SmrBadgeView badge) =>
        Assert.Single(Descendants(badge).OfType<Label>());

    private static IEnumerable<IVisualTreeElement> Descendants(SmrBadgeView badge) =>
        badge.GetVisualTreeDescendants();

    /// <summary>
    /// Stands in for a platform: says that whatever it is asked to draw is the size it was given,
    /// or as much of it as the room allows, the way a label squeezes into a narrow column.
    /// </summary>
    private sealed class SizedHandler(Size size) : ViewHandler<IView, object>(new PropertyMapper<IView>())
    {
        public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
            new(Math.Min(size.Width, widthConstraint), Math.Min(size.Height, heightConstraint));

        protected override object CreatePlatformView() => new();
    }

    private sealed class AbcBadgeView : SmrBadgeView
    {
        protected override View CreateDefaultBadge() => new BoxView { Color = Colors.Green };
    }

    private sealed class AbcCornerBadgeView : SmrBadgeView
    {
        public AbcCornerBadgeView()
        {
            Placement = BadgePlacement.TopStart;
            TextColor = Colors.Yellow;
        }
    }
}
