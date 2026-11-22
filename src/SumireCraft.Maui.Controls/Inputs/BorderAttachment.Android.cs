using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Platform;

namespace SumireCraft.Maui.Controls;

// Android draws every input here -- Entry, Editor and the three pickers -- as an EditText standing on
// the Material line, its background. The border is drawn here as a background of the library's own:
// the application's background colour, if it gave one, in the input's outline, and the border over
// it. .NET MAUI leaves a background that is not its own alone, until the application sets a
// background of its own; then it puts back its own over the Material line, and the border is drawn
// again once it has.
internal sealed partial class BorderAttachment
{
    // The background the input had before its border was drawn, given back when it no longer is.
    private Drawable? _original;

    // The background drawn here last, to tell it from one .NET MAUI put in since.
    private Drawable? _drawn;

    // The padding the input had before its border was drawn, in pixels: the room the Material
    // background kept between the input's edge and its text.
    private (int Left, int Top, int Right, int Bottom)? _padding;

    partial void Apply(object platformView, BorderLook look)
    {
        if (platformView is not EditText edit || edit.Context is not { } context)
        {
            return;
        }

        var current = edit.Background;
        if (!ReferenceEquals(current, _drawn))
        {
            _original ??= current;
        }

        var padding = _padding ??= (edit.PaddingLeft, edit.PaddingTop, edit.PaddingRight, edit.PaddingBottom);
        var color = look.Color?.ToPlatform() ?? new Android.Graphics.Color(edit.CurrentTextColor);
        var drawn = new BorderDrawable(FillOf(current), look, color, context.ToPixels(1));

        // The border is drawn inside the input, over the room that kept the text off its edge: a
        // border down a side takes that room by its thickness, and the text moves in by as much,
        // whichever way the input is laid out. A line under the text, as thin as the Material line,
        // lies where that line did, in room of its own below the text; a thick border at the top or
        // the bottom is left to come nearer the text.
        edit.Background = drawn;
        edit.SetPadding(
            padding.Left + Pixels(context, look.Thickness.Left),
            padding.Top,
            padding.Right + Pixels(context, look.Thickness.Right),
            padding.Bottom);
        _drawn = drawn;
    }

    partial void Redraw(object platformView, BorderLook look)
    {
        // .NET MAUI puts its own background back as it maps the application's, which it does after
        // this has heard of the change: draw the border once it has, as the border is by then, which
        // a change heard in the meantime may have moved on from the one handed here.
        _view.Dispatcher.Dispatch(() =>
        {
            if (ReferenceEquals(_platformView, platformView))
            {
                Apply(platformView, Look);
            }
        });
    }

    partial void Restore(object platformView)
    {
        if (platformView is EditText edit)
        {
            if (ReferenceEquals(edit.Background, _drawn))
            {
                edit.Background = _original;

                // The background given back may be older than the application's now: have .NET MAUI
                // make it again from what the input says.
                _view.Handler?.UpdateValue(nameof(IView.Background));
            }

            // The Material background brings its own padding back with it, but not the padding the
            // input had if that was not the Material background's -- one an application set through
            // a handler, say -- and .NET MAUI keeps the padding as it is when it puts a background
            // of its own in. So the padding the input had is given back whoever's background it has.
            if (_padding is { } padding)
            {
                edit.SetPadding(padding.Left, padding.Top, padding.Right, padding.Bottom);
            }
        }

        _original = null;
        _drawn = null;
        _padding = null;
    }

    // The application's background, which comes first in the layers .NET MAUI draws an input with
    // over the Material line (an internal type, known here only as layers), or is held by the
    // background drawn here. Nothing when the input has the Material line alone.
    private static Drawable? FillOf(Drawable? background) => background switch
    {
        BorderDrawable drawn => drawn.Fill,
        LayerDrawable { NumberOfLayers: > 0 } layers => layers.GetDrawable(0),
        _ => null,
    };

    private static int Pixels(Android.Content.Context context, double units) => (int)Math.Round(context.ToPixels(units));
}
