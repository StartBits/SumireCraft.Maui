namespace SumireCraft.DateTimes;

/// <summary>
/// Creates a <see cref="DateTimeInput"/> from the value an application already holds.
/// </summary>
public static class DateTimeInputExtensions
{
    /// <summary>
    /// Creates an input whose date and time are both taken from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The date and time to start from.</param>
    /// <returns>A new input.</returns>
    public static DateTimeInput ToDateTimeInput(this DateTime value) => new(value);

    /// <summary>
    /// Creates an input from <paramref name="value"/>, or an empty one when it is
    /// <see langword="null"/>. The result is never <see langword="null"/>, so it can always be bound
    /// to and written through.
    /// </summary>
    /// <param name="value">The date and time to start from.</param>
    /// <returns>A new input.</returns>
    public static DateTimeInput ToDateTimeInput(this DateTime? value) => value is { } v ? new(v) : new();
}
