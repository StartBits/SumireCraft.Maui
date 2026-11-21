namespace SumireCraft.Maui.Lifetime;

/// <summary>
/// Decides which handler change is a connect, which a disconnect, and which is neither.
/// </summary>
/// <remarks>
/// Kept apart from the element on purpose, so that the decisions are an ordinary class with ordinary
/// tests. The callbacks strictly alternate, connected first, whatever order and repetition the
/// handler changes come in.
/// </remarks>
internal sealed class HandlerObserver
{
    private readonly Action _onConnected;
    private readonly Action _onDisconnected;

    private bool _connected;

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerObserver"/> class.
    /// </summary>
    /// <param name="onConnected">Called when a handler has arrived.</param>
    /// <param name="onDisconnected">Called when the handler is leaving.</param>
    /// <exception cref="ArgumentNullException">Either callback is <see langword="null"/>.</exception>
    public HandlerObserver(Action onConnected, Action onDisconnected)
    {
        ArgumentNullException.ThrowIfNull(onConnected);
        ArgumentNullException.ThrowIfNull(onDisconnected);

        _onConnected = onConnected;
        _onDisconnected = onDisconnected;
    }

    /// <summary>
    /// To be called from <see cref="Element.HandlerChanging"/>.
    /// </summary>
    /// <param name="oldHandler">The handler that is leaving, if any.</param>
    /// <param name="newHandler">The handler that is taking its place, if any.</param>
    public void OnHandlerChanging(object? oldHandler, object? newHandler)
    {
        if (!_connected || ReferenceEquals(oldHandler, newHandler))
        {
            return;
        }

        // Flipped before the callback runs, so that a callback that throws, or sets off another
        // handler change, leaves the state the next change expects.
        _connected = false;
        _onDisconnected();
    }

    /// <summary>
    /// To be called from <see cref="Element.HandlerChanged"/>, and once when observing starts.
    /// </summary>
    /// <param name="handler">The element's handler as it is now.</param>
    public void OnHandlerChanged(object? handler)
    {
        if (_connected || handler is null)
        {
            return;
        }

        // Flipped before the callback runs, as above: a connect that throws halfway is still followed
        // by its disconnect, so that whatever it did take is let go of.
        _connected = true;
        _onConnected();
    }
}
