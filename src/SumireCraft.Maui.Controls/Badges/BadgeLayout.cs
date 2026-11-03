using Microsoft.Maui.Layouts;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// Lays out what is badged, with its badge over one of its corners and a quarter of it hanging out
/// past each edge.
/// </summary>
/// <remarks>
/// <para>
/// The first child is what is badged, and any after it are badges. The whole thing is the size of
/// what is badged, whatever its badges do, so a count growing or shrinking never moves anything
/// around it.
/// </para>
/// <para>
/// A quarter is where Material 3 puts a badge with text: 4dp of a 16dp badge hang out past each
/// edge of the corner. A badge centred on the corner, as on the web, reads as falling off a
/// rounded button, whose drawn corner lies further in than its frame's.
/// </para>
/// <para>
/// A badge is aligned to a corner of a frame grown by a quarter of the badge's own size on every
/// side. Growing the frame evenly leaves which corner that is to the alignment and margin the badge
/// carries, and so to .NET MAUI, which already mirrors both for a right-to-left flow. A margin
/// alone cannot do this: it is a fixed distance, and a quarter of a badge is not, since the badge
/// widens with what it says.
/// </para>
/// </remarks>
internal sealed class BadgeLayout : Layout
{
    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    private sealed class Manager(BadgeLayout layout) : LayoutManager(layout)
    {
        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            var size = Size.Zero;

            for (var i = 0; i < Layout.Count; i++)
            {
                var child = Layout[i];

                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                if (i == 0)
                {
                    size = child.Measure(widthConstraint, heightConstraint);
                }
                else
                {
                    // Unconstrained, so that a badge wider than what it marks keeps its own size.
                    child.Measure(double.PositiveInfinity, double.PositiveInfinity);
                }
            }

            return new Size(
                ResolveConstraints(widthConstraint, Layout.Width, size.Width, Layout.MinimumWidth, Layout.MaximumWidth),
                ResolveConstraints(heightConstraint, Layout.Height, size.Height, Layout.MinimumHeight, Layout.MaximumHeight));
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            for (var i = 0; i < Layout.Count; i++)
            {
                var child = Layout[i];

                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                if (i == 0)
                {
                    child.Arrange(bounds);
                }
                else
                {
                    // The desired size counts the margin the offset is made of, which is not part
                    // of the badge.
                    var margin = child.Margin;
                    var width = child.DesiredSize.Width - margin.HorizontalThickness;
                    var height = child.DesiredSize.Height - margin.VerticalThickness;

                    child.Arrange(bounds.Inflate(width / 4, height / 4));
                }
            }

            return bounds.Size;
        }
    }
}
