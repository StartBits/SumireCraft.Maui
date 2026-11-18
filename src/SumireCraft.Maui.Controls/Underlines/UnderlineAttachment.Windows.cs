using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Border = Microsoft.UI.Xaml.Controls.Border;
using CalendarDatePicker = Microsoft.UI.Xaml.Controls.CalendarDatePicker;
using ComboBox = Microsoft.UI.Xaml.Controls.ComboBox;
using Control = Microsoft.UI.Xaml.Controls.Control;
using SolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using TextBox = Microsoft.UI.Xaml.Controls.TextBox;
using TimePicker = Microsoft.UI.Xaml.Controls.TimePicker;
using VisualTreeHelper = Microsoft.UI.Xaml.Media.VisualTreeHelper;
using WinThickness = Microsoft.UI.Xaml.Thickness;

namespace SumireCraft.Maui.Controls;

// WinUI draws each input's box with a border in its template, from the control's BorderBrush and
// BorderThickness, and swaps the brush (and for a TextBox the thickness) for theme resources as the
// pointer moves over it, it is pressed, focused or disabled. The line is drawn here by setting the
// border to the bottom edge alone, and pointing every one of those resources, in the control's own
// resources, at one brush whose colour is the line's. The keys differ by control; they are WinUI's
// own, from its theme resources.
internal sealed partial class UnderlineAttachment
{
    private static readonly string[] TextBoxBrushes =
        ["TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushFocused", "TextControlBorderBrushDisabled"];

    private static readonly string[] ComboBoxBrushes =
        ["ComboBoxBorderBrush", "ComboBoxBorderBrushPointerOver", "ComboBoxBorderBrushPressed", "ComboBoxBorderBrushDisabled"];

    private static readonly string[] CalendarDatePickerBrushes =
        ["CalendarDatePickerBorderBrush", "CalendarDatePickerBorderBrushPointerOver", "CalendarDatePickerBorderBrushPressed", "CalendarDatePickerBorderBrushDisabled"];

    // A TimePicker's own style takes its border brush from DatePickerButtonBorderBrush, a date
    // picker's key; its states then use keys of its own.
    private static readonly string[] TimePickerBrushes =
        ["DatePickerButtonBorderBrush", "TimePickerButtonBorderBrushPointerOver", "TimePickerButtonBorderBrushPressed", "TimePickerButtonBorderBrushDisabled"];

    // A TextBox takes this thickness while focused, whatever its own; the other three keep theirs.
    private const string TextBoxFocusedThickness = "TextControlBorderThemeThicknessFocused";

    // The rounded ring a ComboBox draws around itself while focused, a part of its template whose
    // thickness the template reads once, as a static resource: hidden on the part itself instead.
    private const string ComboBoxFocusRing = "HighlightBackground";

    // The one brush every state's resource points at, so that a new colour reaches them all at once.
    private SolidColorBrush? _brush;

    // The resource keys set on the control, to take them away again.
    private string[] _keys = [];

    // The ComboBox's focus ring, hidden while the box is underlined.
    private Border? _ring;

    partial void Apply(object platformView, UnderlineLook look)
    {
        if (platformView is not Control control || Brushes(control) is not { } brushes)
        {
            return;
        }

        var color = look.Color?.ToWindowsColor() ?? (control.Foreground as SolidColorBrush)?.Color ?? Microsoft.UI.Colors.Black;
        var first = _brush is null;
        _brush ??= new SolidColorBrush(color);
        _brush.Color = color;

        var resources = new Dictionary<string, object>();
        foreach (var key in brushes)
        {
            resources[key] = _brush;
        }

        if (control is TextBox)
        {
            resources[TextBoxFocusedThickness] = Bottom(SmrUnderline.GetFocusedThickness(_view));
        }

        // A template reads its theme resources when it is applied. Ones changed after that are read
        // again only when the theme changes, so a change to them on a control already on screen
        // changes the theme there and back.
        var changed = first;
        foreach (var (key, value) in resources)
        {
            if (!control.Resources.TryGetValue(key, out var held) || !Equals(held, value))
            {
                control.Resources[key] = value;
                changed = true;
            }
        }

        _keys = [.. resources.Keys];

        control.BorderBrush = _brush;
        control.BorderThickness = Bottom(look.Thickness);
        control.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(0);

        if (control is ComboBox box && first)
        {
            HideRing(box);
        }

        if (changed)
        {
            ReadThemeAgain(control);
        }
    }

    partial void Redraw(object platformView, UnderlineLook look)
    {
        // WinUI draws the border at any size and over any background: nothing to do.
    }

    partial void Restore(object platformView)
    {
        if (platformView is Control control && _brush is not null)
        {
            foreach (var key in _keys)
            {
                control.Resources.Remove(key);
            }

            control.ClearValue(Control.BorderBrushProperty);
            control.ClearValue(Control.BorderThicknessProperty);
            control.ClearValue(Control.CornerRadiusProperty);

            if (control is ComboBox box)
            {
                box.Loaded -= OnComboBoxLoaded;
            }

            _ring?.ClearValue(UIElement.VisibilityProperty);
            ReadThemeAgain(control);
        }

        _brush = null;
        _keys = [];
        _ring = null;
    }

    private static string[]? Brushes(Control control) => control switch
    {
        TextBox => TextBoxBrushes,
        ComboBox => ComboBoxBrushes,
        CalendarDatePicker => CalendarDatePickerBrushes,
        TimePicker => TimePickerBrushes,
        _ => null,
    };

    private static WinThickness Bottom(double thickness) => new(0, 0, 0, thickness);

    private static void ReadThemeAgain(FrameworkElement element)
    {
        if (!element.IsLoaded)
        {
            return;
        }

        var requested = element.RequestedTheme;
        element.RequestedTheme = element.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        element.RequestedTheme = requested;
    }

    private static Border? FindBorder(DependencyObject parent, string name)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is Border border && border.Name == name)
            {
                return border;
            }

            if (FindBorder(child, name) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    // The ring is a part of the template, there only once the box is loaded. Its visibility is not
    // one the box's states set, so hiding it holds through them.
    private void HideRing(ComboBox box)
    {
        if (!box.IsLoaded)
        {
            box.Loaded += OnComboBoxLoaded;
            return;
        }

        _ring = FindBorder(box, ComboBoxFocusRing);
        _ring?.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void OnComboBoxLoaded(object sender, RoutedEventArgs e)
    {
        var box = (ComboBox)sender;
        box.Loaded -= OnComboBoxLoaded;
        if (ReferenceEquals(_platformView, box) && _brush is not null)
        {
            HideRing(box);
        }
    }
}
