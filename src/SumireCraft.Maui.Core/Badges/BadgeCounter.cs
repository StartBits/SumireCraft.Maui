using System.ComponentModel;
using System.Globalization;

namespace SumireCraft.Maui.Badges;

/// <summary>
/// Turns a count into what a badge shows: whether it is visible, and the text it carries.
/// </summary>
/// <remarks>
/// <para>
/// Under the default rules a count of zero hides the badge, a count from one to 99 is shown as it
/// is, and anything above 99 is shown as <c>99+</c>. Both rules can be changed, but only through the
/// constructor: a badge that switched rules halfway through its life would be hard to reason about.
/// </para>
/// <para>
/// A negative count is treated as zero. Counts are often derived -- everything minus what has been
/// read -- and can dip below zero for a moment, and a setter bound to the user interface is the
/// wrong place to throw about it.
/// </para>
/// <para>
/// Nothing here knows about views. <see cref="IsVisible"/> and <see cref="Text"/> are meant to be
/// bound to whichever badge is on screen, be it <c>SmrBadgeView</c> or another vendor's. A hidden
/// badge has empty text, so a badge that draws nothing for empty text can be bound by
/// <see cref="Text"/> alone.
/// </para>
/// <para>
/// <see cref="PropertyChanged"/> is raised on the thread that set <see cref="Count"/>, and only for
/// the properties whose value actually changed. A caller setting it off the user interface thread
/// has to marshal for itself.
/// </para>
/// </remarks>
public sealed class BadgeCounter : INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs CountChanged = new(nameof(Count));
    private static readonly PropertyChangedEventArgs IsVisibleChanged = new(nameof(IsVisible));
    private static readonly PropertyChangedEventArgs TextChanged = new(nameof(Text));

    private int _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeCounter"/> class, starting at zero.
    /// </summary>
    /// <param name="maxCount">
    /// The largest count shown as it is. A larger count is shown as this number followed by
    /// <c>+</c>.
    /// </param>
    /// <param name="showZero">
    /// <see langword="true"/> to keep the badge visible at zero, showing <c>0</c>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxCount"/> is less than one.
    /// </exception>
    public BadgeCounter(int maxCount = 99, bool showZero = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxCount, 1);

        MaxCount = maxCount;
        ShowZero = showZero;
        IsVisible = IsVisibleFor(0);
        Text = TextFor(0);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the largest count shown as it is.
    /// </summary>
    public int MaxCount { get; }

    /// <summary>
    /// Gets whether the badge stays visible at zero.
    /// </summary>
    public bool ShowZero { get; }

    /// <summary>
    /// Gets or sets the count. A negative value is stored as zero.
    /// </summary>
    public int Count
    {
        get => _count;
        set
        {
            var count = Math.Max(value, 0);
            if (count == _count)
            {
                return;
            }

            _count = count;
            PropertyChanged?.Invoke(this, CountChanged);

            var isVisible = IsVisibleFor(count);
            if (isVisible != IsVisible)
            {
                IsVisible = isVisible;
                PropertyChanged?.Invoke(this, IsVisibleChanged);
            }

            var text = TextFor(count);
            if (text != Text)
            {
                Text = text;
                PropertyChanged?.Invoke(this, TextChanged);
            }
        }
    }

    /// <summary>
    /// Gets whether the badge should be shown for the current <see cref="Count"/>.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Gets the text the badge should carry for the current <see cref="Count"/>. Empty while the
    /// badge is hidden.
    /// </summary>
    public string Text { get; private set; }

    private bool IsVisibleFor(int count) => count > 0 || ShowZero;

    private string TextFor(int count)
    {
        if (!IsVisibleFor(count))
        {
            return "";
        }

        return count > MaxCount
            ? MaxCount.ToString(CultureInfo.InvariantCulture) + "+"
            : count.ToString(CultureInfo.InvariantCulture);
    }
}
