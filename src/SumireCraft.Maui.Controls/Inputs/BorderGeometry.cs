namespace SumireCraft.Maui.Controls;

// The shape Android and iOS draw a border as: the input's outline, less the same outline taken in by
// each side's thickness, filled between the two. Windows draws the border itself.
internal static class BorderGeometry
{
    // The input's outline, with the corners in the units the bounds are in.
    public static RoundedRect Outline(RectF bounds, CornerRadius radius, double scale) => new(
        bounds,
        new CornerRadius(radius.TopLeft * scale, radius.TopRight * scale, radius.BottomLeft * scale, radius.BottomRight * scale));

    // The outline taken in by each side, its corners rounded less by the thicker of their two sides.
    public static RoundedRect Inside(RectF bounds, Thickness thickness, CornerRadius radius, double scale)
    {
        var (left, top, right, bottom) = (thickness.Left * scale, thickness.Top * scale, thickness.Right * scale, thickness.Bottom * scale);
        var inside = new RectF(
            (float)(bounds.Left + left),
            (float)(bounds.Top + top),
            (float)Math.Max(0, bounds.Width - left - right),
            (float)Math.Max(0, bounds.Height - top - bottom));

        return new RoundedRect(inside, new CornerRadius(
            Math.Max(0, (radius.TopLeft * scale) - Math.Max(left, top)),
            Math.Max(0, (radius.TopRight * scale) - Math.Max(right, top)),
            Math.Max(0, (radius.BottomLeft * scale) - Math.Max(left, bottom)),
            Math.Max(0, (radius.BottomRight * scale) - Math.Max(right, bottom))));
    }

    // The border itself: the outline and the inside, as two figures of one path to be filled even-odd,
    // so that what is filled is what lies between them.
    public static PathF Ring(RectF bounds, BorderLook look, double scale)
    {
        var path = new PathF();
        Append(path, Outline(bounds, look.CornerRadius, scale));
        Append(path, Inside(bounds, look.Thickness, look.CornerRadius, scale));
        return path;
    }

    // The outline alone, which a background colour fills.
    public static PathF Fill(RectF bounds, BorderLook look, double scale)
    {
        var path = new PathF();
        Append(path, Outline(bounds, look.CornerRadius, scale));
        return path;
    }

    private static void Append(PathF path, RoundedRect shape)
    {
        var r = shape.CornerRadius;
        path.AppendRoundedRectangle(shape.Bounds, (float)r.TopLeft, (float)r.TopRight, (float)r.BottomLeft, (float)r.BottomRight);
    }

    // A rectangle with a radius of its own at each corner.
    internal readonly record struct RoundedRect(RectF Bounds, CornerRadius CornerRadius);
}
