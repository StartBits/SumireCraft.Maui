using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SumireCraft.Maui.Controls;

// The border of the library's five inputs, worked out and drawn the same way for each. Each input
// declares its own three bindable properties, made here, because XAML looks for what converts a
// value written as text on the type that declares the property: one declared here would have none.
internal static class InputBorder
{
    // The inputs whose box is the library's now, each with what draws it. Held weakly, so that an
    // input is let go of with its page; what draws it goes with it.
    private static readonly ConditionalWeakTable<View, BorderAttachment> Attachments = [];

    // What BindableProperty.Create keeps of the type that declares a property, for XAML to find there.
    private const DynamicallyAccessedMemberTypes Declared =
        DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties;

    public static BindableProperty CreateBorderThicknessProperty([DynamicallyAccessedMembers(Declared)] Type declaringType) => BindableProperty.Create(
        nameof(IBorderedInput.BorderThickness), typeof(Thickness?), declaringType, null, validateValue: IsThickness, propertyChanged: OnShapeChanged);

    public static BindableProperty CreateCornerRadiusProperty([DynamicallyAccessedMembers(Declared)] Type declaringType) => BindableProperty.Create(
        nameof(IBorderedInput.CornerRadius), typeof(CornerRadius?), declaringType, null, validateValue: IsCornerRadius, propertyChanged: OnShapeChanged);

    public static BindableProperty CreateBorderColorProperty([DynamicallyAccessedMembers(Declared)] Type declaringType) => BindableProperty.Create(
        nameof(IBorderedInput.BorderColor), typeof(Color), declaringType, null, propertyChanged: OnColorChanged);

    // The border the input is drawn with now, or null while it has the platform's box: a thickness
    // with no radius has square corners, and a radius with no thickness is one all round.
    public static BorderLook? LookOf(View view)
    {
        if (view is not IBorderedInput input || (input.BorderThickness is null && input.CornerRadius is null))
        {
            return null;
        }

        return new BorderLook(
            input.BorderThickness ?? new Thickness(1),
            input.CornerRadius ?? new CornerRadius(0),
            input.BorderColor ?? (view as ITextStyle)?.TextColor);
    }

    // The border what draws the input holds now, or null while nothing draws it: what the platform
    // was last handed, as the tests see it.
    public static BorderLook? DrawnLookOf(View view) =>
        Attachments.TryGetValue(view, out var attachment) ? attachment.Look : null;

    // No side thinner than nothing, and every one a number.
    private static bool IsThickness(BindableObject bindable, object? value) =>
        value is not Thickness t || (IsLength(t.Left) && IsLength(t.Top) && IsLength(t.Right) && IsLength(t.Bottom));

    private static bool IsCornerRadius(BindableObject bindable, object? value) =>
        value is not CornerRadius r || (IsLength(r.TopLeft) && IsLength(r.TopRight) && IsLength(r.BottomLeft) && IsLength(r.BottomRight));

    private static bool IsLength(double length) => length >= 0 && double.IsFinite(length);

    private static void OnShapeChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var view = (View)bindable;
        if (LookOf(view) is null)
        {
            if (Attachments.TryGetValue(view, out var attachment))
            {
                attachment.Detach();
                Attachments.Remove(view);
            }
        }
        else if (Attachments.TryGetValue(view, out var attachment))
        {
            attachment.Refresh();
        }
        else
        {
            Attachments.Add(view, new BorderAttachment(view));
        }
    }

    private static void OnColorChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (Attachments.TryGetValue((View)bindable, out var attachment))
        {
            attachment.Refresh();
        }
    }
}
