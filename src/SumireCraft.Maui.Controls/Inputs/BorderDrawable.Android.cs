using Android.Graphics;
using Android.Graphics.Drawables;
using Microsoft.Maui.Graphics.Platform;
using Paint = Android.Graphics.Paint;
using Path = Android.Graphics.Path;
using RectF = Microsoft.Maui.Graphics.RectF;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// An input's background drawn by the library: the application's background, if it gave one, in the
/// input's outline, and the border over it, in place of the Material line Android draws under an
/// EditText.
/// </summary>
internal sealed class BorderDrawable : Drawable
{
    private readonly Paint _paint = new() { AntiAlias = true };

    private readonly BorderLook _look;

    private readonly float _density;

    /// <summary>
    /// Makes a background of the border given, in the colour given, over the application's
    /// background if it gave one. The border's sizes are device-independent units, drawn at the
    /// density given.
    /// </summary>
    public BorderDrawable(Drawable? fill, BorderLook look, Android.Graphics.Color color, float density)
    {
        Fill = fill;
        _look = look;
        _density = density;
        _paint.Color = color;
    }

    /// <summary>
    /// Gets the application's background, drawn in the input's outline.
    /// </summary>
    public Drawable? Fill { get; }

    /// <inheritdoc/>
    public override int Opacity => (int)Format.Translucent;

    /// <inheritdoc/>
    public override void Draw(Canvas canvas)
    {
        var area = Bounds;
        var bounds = new RectF(area.Left, area.Top, area.Width(), area.Height());

        if (Fill is not null)
        {
            using var outline = BorderGeometry.Fill(bounds, _look, _density).AsAndroidPath();
            canvas.Save();
            canvas.ClipPath(outline);
            Fill.SetBounds(area.Left, area.Top, area.Right, area.Bottom);
            Fill.Draw(canvas);
            canvas.Restore();
        }

        using var ring = BorderGeometry.Ring(bounds, _look, _density).AsAndroidPath();
        ring.SetFillType(Path.FillType.EvenOdd!);
        canvas.DrawPath(ring, _paint);
    }

    /// <inheritdoc/>
    public override void SetAlpha(int alpha)
    {
        _paint.Alpha = alpha;
        InvalidateSelf();
    }

    /// <inheritdoc/>
    public override void SetColorFilter(ColorFilter? colorFilter)
    {
        _paint.SetColorFilter(colorFilter);
        InvalidateSelf();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _paint.Dispose();
        }

        base.Dispose(disposing);
    }
}
