namespace SumireCraft.DateTimes;

/// <summary>
/// A date and a time that are entered separately and can each be empty, ready to bind to separate
/// pickers.
/// </summary>
/// <remarks>
/// <para>
/// The input holds a date and a time and nothing else. Every property reads or writes one or both
/// of them through the type a particular control wants, named <c>&lt;part&gt;As&lt;type&gt;</c>, and
/// a property writes only its own part: writing <see cref="DateAsDateTime"/> leaves the time alone
/// and discards the time of the value written.
/// </para>
/// <para>
/// Every <see cref="DateTime"/> read from an input has a <see cref="DateTime.Kind"/> of
/// <see cref="DateTimeKind.Unspecified"/>, whatever the kind of the value written.
/// </para>
/// <para>
/// An input can be read and written from any thread. It raises no change notification: a view
/// writes into the input it was given, and code that wants a view to show other values gives it
/// another input.
/// </para>
/// <para>
/// Two inputs compared with <c>==</c> are compared by reference, as with any class.
/// </para>
/// </remarks>
public class DateTimeInput
{
    private readonly Lock _sync = new();

    /// <summary>
    /// Initializes an input whose date and time are both empty.
    /// </summary>
    public DateTimeInput()
    {
    }

    /// <summary>
    /// Initializes an input whose date and time are both taken from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The date and time to start from.</param>
    public DateTimeInput(DateTime value)
    {
        DateAndTime = value;
    }

    /// <summary>
    /// Gets or sets the date, or <see langword="null"/> when it is empty.
    /// </summary>
    public DateOnly? DateAsDateOnly
    {
        get
        {
            lock (_sync)
            {
                return field;
            }
        }

        set
        {
            lock (_sync)
            {
                field = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the date as a <see cref="DateTime"/> at midnight, or <see langword="null"/> when
    /// it is empty. The time of a value written is discarded.
    /// </summary>
    public DateTime? DateAsDateTime
    {
        get => DateAsDateOnly?.ToDateTime(TimeOnly.MinValue);
        set => DateAsDateOnly = ToDate(value);
    }

    /// <summary>
    /// Gets or sets the time, or <see langword="null"/> when it is empty.
    /// </summary>
    public TimeOnly? TimeAsTimeOnly
    {
        get
        {
            lock (_sync)
            {
                return field;
            }
        }

        set
        {
            lock (_sync)
            {
                field = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the time as the span since midnight, or <see langword="null"/> when it is empty.
    /// Writing a span that is not a time of day -- a negative one, or 24 hours or more -- changes
    /// nothing.
    /// </summary>
    public TimeSpan? TimeAsTimeSpan
    {
        get => TimeAsTimeOnly?.ToTimeSpan();
        set
        {
            if (value is not { } span)
            {
                TimeAsTimeOnly = null;
            }
            else if (span.Ticks is >= 0 and < TimeSpan.TicksPerDay)
            {
                TimeAsTimeOnly = TimeOnly.FromTimeSpan(span);
            }
        }
    }

    /// <summary>
    /// Gets or sets the time as a <see cref="DateTime"/>, or <see langword="null"/> when it is empty.
    /// The date of a value read means nothing, and the date of a value written is discarded.
    /// </summary>
    public DateTime? TimeAsDateTime
    {
        get => TimeAsTimeOnly is { } time ? new DateTime(time.Ticks) : null;
        set => TimeAsTimeOnly = ToTime(value);
    }

    /// <summary>
    /// Gets or sets the date and the time together, or <see langword="null"/> when both are empty.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A part that is empty reads as its earliest value: with a date alone this is that date at
    /// midnight, and with a time alone it is that time on 1 January 0001. Use
    /// <see cref="IsComplete"/> to tell whether both parts were entered.
    /// </para>
    /// <para>
    /// Writing a value sets both parts, so writing back what was read fills a part that was empty.
    /// Writing <see langword="null"/> empties both.
    /// </para>
    /// </remarks>
    public DateTime? DateAndTime
    {
        get
        {
            var (date, time) = ReadBoth();
            return date is null && time is null
                ? null
                : (date ?? DateOnly.MinValue).ToDateTime(time ?? TimeOnly.MinValue);
        }

        set
        {
            // The lock is taken again inside each property; holding it here as well is what keeps
            // a reader from seeing the new date beside the old time.
            lock (_sync)
            {
                DateAsDateOnly = ToDate(value);
                TimeAsTimeOnly = ToTime(value);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether both the date and the time have been entered.
    /// </summary>
    public bool IsComplete
    {
        get
        {
            var (date, time) = ReadBoth();
            return date is not null && time is not null;
        }
    }

    /// <summary>
    /// Converts an input to its <see cref="DateAndTime"/>.
    /// </summary>
    /// <param name="input">The input to convert. Not <see langword="null"/>.</param>
    /// <exception cref="NullReferenceException"><paramref name="input"/> is null.</exception>
    public static implicit operator DateTime?(DateTimeInput input) => input.DateAndTime;

    /// <summary>
    /// Returns <see cref="DateAndTime"/> as text, or an empty string when both parts are empty.
    /// </summary>
    /// <returns>The text of <see cref="DateAndTime"/> in the current culture.</returns>
    public override string ToString() => DateAndTime?.ToString() ?? string.Empty;

    private static DateOnly? ToDate(DateTime? value) => value is { } v ? DateOnly.FromDateTime(v) : null;

    private static TimeOnly? ToTime(DateTime? value) => value is { } v ? TimeOnly.FromDateTime(v) : null;

    private (DateOnly? Date, TimeOnly? Time) ReadBoth()
    {
        lock (_sync)
        {
            return (DateAsDateOnly, TimeAsTimeOnly);
        }
    }
}
