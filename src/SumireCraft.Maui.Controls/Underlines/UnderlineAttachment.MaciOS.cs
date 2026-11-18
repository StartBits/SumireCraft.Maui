using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Platform;
using UIKit;

namespace SumireCraft.Maui.Controls;

// iOS draws an Entry and the three pickers as a UITextField, with a rounded box of its own, and an
// Editor as a UITextView, with none. Mac Catalyst does the same but for the date and time pickers,
// which .NET MAUI draws there as a UIDatePicker, a compact pill whose look is the system's: those two
// are left alone on Mac Catalyst. The line is a layer of the library's own across the bottom of the
// view, and a text field's box is taken away while it is there. A text view scrolls its layers with
// its text, so there the line is put back at the bottom as it scrolls.
internal sealed partial class UnderlineAttachment
{
    private CALayer? _line;

    // The box the text field had before it was underlined, given back when it no longer is.
    private UITextBorderStyle? _originalBorder;

    // Watches a text view's scrolling, to keep the line at its bottom.
    private IDisposable? _scrolling;

    partial void Apply(object platformView, UnderlineLook look)
    {
        if (platformView is not UIView view || view is UIDatePicker)
        {
            return;
        }

        if (view is UITextField field && _originalBorder is null)
        {
            _originalBorder = field.BorderStyle;
            field.BorderStyle = UITextBorderStyle.None;
        }

        if (_line is null || !ReferenceEquals(_line.SuperLayer, view.Layer))
        {
            _line?.RemoveFromSuperLayer();
            _line = new CALayer();
            view.Layer.AddSublayer(_line);

            if (view is UITextView text)
            {
                _scrolling = text.AddObserver("contentOffset", NSKeyValueObservingOptions.New, _ => Place(text));
            }
        }

        var color = look.Color?.ToPlatform() ?? TextColor(view) ?? UIColor.Label;
        _line.BackgroundColor = color.CGColor;
        Place(view);
    }

    partial void Redraw(object platformView, UnderlineLook look)
    {
        if (platformView is UIView view)
        {
            Place(view);
        }
    }

    partial void Restore(object platformView)
    {
        _scrolling?.Dispose();
        _scrolling = null;
        _line?.RemoveFromSuperLayer();
        _line = null;

        if (platformView is UITextField field && _originalBorder is { } border)
        {
            field.BorderStyle = border;
        }

        _originalBorder = null;
    }

    private static UIColor? TextColor(UIView view) => view switch
    {
        UITextField field => field.TextColor,
        UITextView text => text.TextColor,
        _ => null,
    };

    // Across the bottom of what the view shows: of its bounds, which for a text view that has
    // scrolled start where its content is scrolled to.
    private void Place(UIView view)
    {
        if (_line is null)
        {
            return;
        }

        var thickness = (nfloat)Look.Thickness;
        var bounds = view.Bounds;
        CATransaction.Begin();
        CATransaction.DisableActions = true;
        _line.Frame = new CGRect(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness);
        CATransaction.Commit();
    }
}
