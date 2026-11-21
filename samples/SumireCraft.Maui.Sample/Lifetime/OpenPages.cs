namespace SumireCraft.Maui.Sample.Lifetime;

/// <summary>
/// A service that lives as long as the application and counts the pages open on it, standing in for
/// any long-lived thing a page subscribes to: a connectivity monitor, a message hub, a clock.
/// </summary>
/// <remarks>
/// <see cref="Changed"/> holds whatever subscribes to it for as long as the application lives. A page
/// that subscribes and never unsubscribes is held too, closed or not; the sample's pages subscribe
/// from <c>ObserveHandler</c>, which unsubscribes them as they close.
/// </remarks>
public static class OpenPages
{
    /// <summary>
    /// Raised whenever <see cref="Count"/> changes.
    /// </summary>
    public static event EventHandler? Changed;

    /// <summary>
    /// Gets how many pages are open.
    /// </summary>
    public static int Count { get; private set; }

    /// <summary>
    /// Counts one more page in.
    /// </summary>
    public static void Add()
    {
        Count++;
        Changed?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Counts one page out.
    /// </summary>
    public static void Remove()
    {
        Count--;
        Changed?.Invoke(null, EventArgs.Empty);
    }
}
