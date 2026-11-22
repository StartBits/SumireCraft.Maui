using CoreAnimation;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Platform;
using UIKit;
using RectF = Microsoft.Maui.Graphics.RectF;

namespace SumireCraft.Maui.Controls;

// iOS draws an Entry and the three pickers as a UITextField, with a rounded box of its own, and an
// Editor as a UITextView, with none. Mac Catalyst does the same but for the date and time pickers,
// which .NET MAUI draws there as a UIDatePicker, a compact pill whose look is the system's: those two
// are left alone on Mac Catalyst. The border is a layer of the library's own over the view, and a
// text field's box is taken away while it is there. Rounded corners clip the view, background
// included, to its outline. Both are put back in place whenever the view's layer changes its bounds:
// .NET MAUI tells the input its new size before it gives the platform view its new frame, and a text
// view scrolls its layers with its text by moving its bounds.
internal sealed partial class BorderAttachment
{
    private CAShapeLayer? _border;

    // The outline the view is clipped to while a corner is rounded.
    private CAShapeLayer? _clip;

    // How far the text keeps from a border down a side, past the border itself: about what the text
    // field's own box kept it from its edge.
    private const double SideGap = 8;

    // The box the text field had before its border was drawn, given back when it no longer is.
    private UITextBorderStyle? _originalBorder;

    // What the text field held at its sides before, given back with its box.
    private (UIView? Left, UITextFieldViewMode LeftMode, UIView? Right, UITextFieldViewMode RightMode) _originalSides;

    // Watches the bounds of the view's layer, to keep the border in place.
    private IDisposable? _bounds;

    partial void Apply(object platformView, BorderLook look)
    {
        if (platformView is not UIView view || view is UIDatePicker)
        {
            return;
        }

        if (view is UITextField field)
        {
            if (_originalBorder is null)
            {
                _originalBorder = field.BorderStyle;
                _originalSides = (field.LeftView, field.LeftViewMode, field.RightView, field.RightViewMode);
                field.BorderStyle = UITextBorderStyle.None;
            }

            KeepTextOffTheSides(field, look.Thickness);
        }

        if (_border is null || !ReferenceEquals(_border.SuperLayer, view.Layer))
        {
            _border?.RemoveFromSuperLayer();
            _bounds?.Dispose();
            _border = new CAShapeLayer { FillRule = CAShapeLayer.FillRuleEvenOdd };
            view.Layer.AddSublayer(_border);
            _bounds = view.Layer.AddObserver("bounds", NSKeyValueObservingOptions.New, _ => Place(view));
        }

        var color = look.Color?.ToPlatform() ?? TextColor(view) ?? UIColor.Label;
        _border.FillColor = color.CGColor;
        Place(view);
    }

    partial void Redraw(object platformView, BorderLook look)
    {
        if (platformView is UIView view)
        {
            Place(view);
        }
    }

    partial void Restore(object platformView)
    {
        _bounds?.Dispose();
        _bounds = null;
        _border?.RemoveFromSuperLayer();
        _border = null;

        if (platformView is UIView view && _clip is not null && ReferenceEquals(view.Layer.Mask, _clip))
        {
            view.Layer.Mask = null;
        }

        _clip = null;

        if (platformView is UITextField field && _originalBorder is { } border)
        {
            field.BorderStyle = border;
            (field.LeftView, field.LeftViewMode, field.RightView, field.RightViewMode) = _originalSides;
        }

        _originalBorder = null;
        _originalSides = default;
    }

    // Taking the text field's box away also takes away the room it kept between its edge and the
    // text, which a line under the text does not miss but a border down a side does: the text would
    // run onto it. An empty view at each such side keeps the text off by the border and a gap.
    private void KeepTextOffTheSides(UITextField field, Thickness thickness)
    {
        if (thickness.Left > 0)
        {
            field.LeftView = new UIView(new CoreGraphics.CGRect(0, 0, thickness.Left + SideGap, 1));
            field.LeftViewMode = UITextFieldViewMode.Always;
        }
        else
        {
            (field.LeftView, field.LeftViewMode) = (_originalSides.Left, _originalSides.LeftMode);
        }

        if (thickness.Right > 0)
        {
            field.RightView = new UIView(new CoreGraphics.CGRect(0, 0, thickness.Right + SideGap, 1));
            field.RightViewMode = UITextFieldViewMode.Always;
        }
        else
        {
            (field.RightView, field.RightViewMode) = (_originalSides.Right, _originalSides.RightMode);
        }
    }

    private static UIColor? TextColor(UIView view) => view switch
    {
        UITextField field => field.TextColor,
        UITextView text => text.TextColor,
        _ => null,
    };

    private static bool IsRounded(CornerRadius radius) =>
        radius.TopLeft > 0 || radius.TopRight > 0 || radius.BottomLeft > 0 || radius.BottomRight > 0;

    // Over what the view shows: its bounds, which for a text view that has scrolled start where its
    // content is scrolled to. The paths are drawn from the layer's own corner.
    private void Place(UIView view)
    {
        if (_border is null)
        {
            return;
        }

        var bounds = view.Bounds;
        var local = new RectF(0, 0, (float)bounds.Width, (float)bounds.Height);

        CATransaction.Begin();
        CATransaction.DisableActions = true;

        _border.Frame = bounds;
        _border.Path = BorderGeometry.Ring(local, Look, 1).AsCGPath();

        if (IsRounded(Look.CornerRadius))
        {
            _clip ??= new CAShapeLayer();
            _clip.Frame = bounds;
            _clip.Path = BorderGeometry.Fill(local, Look, 1).AsCGPath();
            view.Layer.Mask = _clip;
        }
        else if (_clip is not null)
        {
            if (ReferenceEquals(view.Layer.Mask, _clip))
            {
                view.Layer.Mask = null;
            }

            _clip = null;
        }

        CATransaction.Commit();
    }
}
