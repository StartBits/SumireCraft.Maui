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

        var color = look.Color?.ToPlatform() ?? new Android.Graphics.Color(edit.CurrentTextColor);
        var drawn = new BorderDrawable(FillOf(current), look, color, context.ToPixels(1));

        SetKeepingPadding(edit, drawn);
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
        if (platformView is EditText edit && ReferenceEquals(edit.Background, _drawn))
        {
            SetKeepingPadding(edit, _original);

            // The background given back may be older than the application's now: have .NET MAUI
            // make it again from what the input says.
            _view.Handler?.UpdateValue(nameof(IView.Background));
        }

        _original = null;
        _drawn = null;
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

    // Android takes the padding from the background, so a new background moves the text. .NET MAUI
    // keeps it in place the same way.
    private static void SetKeepingPadding(EditText edit, Drawable? background)
    {
        var (left, top, right, bottom) = (edit.PaddingLeft, edit.PaddingTop, edit.PaddingRight, edit.PaddingBottom);
        edit.Background = background;
        edit.SetPadding(left, top, right, bottom);
    }
}
