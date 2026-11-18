using Android.Graphics;
using Android.Graphics.Drawables;
using Paint = Android.Graphics.Paint;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// A band across the bottom of its bounds, and nothing else: the line an underlined input stands
/// on, in place of the Material line Android draws under an EditText.
/// </summary>
internal sealed class UnderlineDrawable : Drawable
{
    private readonly Paint _paint = new() { AntiAlias = false };

    private readonly float _thickness;

    /// <summary>
    /// Makes a band of the colour and the thickness given, in pixels.
    /// </summary>
    public UnderlineDrawable(Android.Graphics.Color color, float thickness)
    {
        _paint.Color = color;
        _thickness = thickness;
    }

    /// <inheritdoc/>
    public override int Opacity => (int)Format.Translucent;

    /// <inheritdoc/>
    public override void Draw(Canvas canvas)
    {
        var bounds = Bounds;
        canvas.DrawRect(bounds.Left, bounds.Bottom - _thickness, bounds.Right, bounds.Bottom, _paint);
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
