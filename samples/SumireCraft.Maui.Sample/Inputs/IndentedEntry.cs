using Microsoft.Maui.Handlers;
using SumireCraft.Maui.Controls;
#if ANDROID
using Microsoft.Maui.Platform;
#endif

namespace SumireCraft.Maui.Sample.Inputs;

/// <summary>
/// An entry whose text the application moves in from its start through the entry handler's mapper,
/// as it would to make room for an icon there. The room a border takes is the library's; taken away,
/// the border gives back the room the application made.
/// </summary>
public class IndentedEntry : SmrEntry
{
    /// <summary>
    /// How far in from the entry's start its text begins, in points.
    /// </summary>
    public const double Indent = 24;

    /// <summary>
    /// Adds the mapping to every entry's handler. It moves the text of this type alone.
    /// </summary>
    public static void AddMapping() =>
        EntryHandler.Mapper.AppendToMapping(nameof(IndentedEntry), (handler, entry) =>
        {
            if (entry is IndentedEntry)
            {
                MakeRoom(handler);
            }
        });

#if ANDROID
    // An EditText keeps its text off its edge by its padding. This sets the left, which is the start
    // only of an entry laid out left to right; the sample lays none out the other way.
    private static void MakeRoom(IEntryHandler handler)
    {
        var edit = handler.PlatformView;
        if (edit.Context is { } context)
        {
            edit.SetPadding((int)Math.Round(context.ToPixels(Indent)), edit.PaddingTop, edit.PaddingRight, edit.PaddingBottom);
        }
    }
#elif IOS || MACCATALYST
    // A text field keeps its text off its start by the view it holds there.
    private static void MakeRoom(IEntryHandler handler)
    {
        var field = handler.PlatformView;
        field.LeftView = new UIKit.UIView(new CoreGraphics.CGRect(0, 0, Indent, 1));
        field.LeftViewMode = UIKit.UITextFieldViewMode.Always;
    }
#elif WINDOWS
    // A TextBox keeps its text off its edge by its padding.
    private static void MakeRoom(IEntryHandler handler)
    {
        var box = handler.PlatformView;
        box.Padding = new Microsoft.UI.Xaml.Thickness(Indent, box.Padding.Top, box.Padding.Right, box.Padding.Bottom);
    }
#endif
}
