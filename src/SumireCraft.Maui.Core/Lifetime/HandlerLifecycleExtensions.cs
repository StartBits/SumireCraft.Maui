namespace SumireCraft.Maui.Lifetime;

/// <summary>
/// Tells an element's own code when the element is connected to the platform and when it is not.
/// </summary>
public static class HandlerLifecycleExtensions
{
    /// <summary>
    /// Calls back when <paramref name="element"/> is connected to the platform, and when it is
    /// disconnected.
    /// </summary>
    /// <param name="element">The element to observe.</param>
    /// <param name="onConnected">Called once a handler is in place.</param>
    /// <param name="onDisconnected">Called as the handler leaves.</param>
    /// <remarks>
    /// <para>
    /// A handler is what connects an element to the platform, so these are the two moments to take
    /// hold of what the element needs while it is alive -- a subscription to a long-lived service, a
    /// timer, a resource of its own -- and to let go of it. A page popped from a <c>Shell</c> or
    /// dismissed as a modal is disconnected by .NET MAUI, and so is every view on it: what they
    /// subscribed to stops holding them, and they are collected with the page. Whether a page closed
    /// another way is disconnected is .NET MAUI's to decide; where it matters, look. Call this once,
    /// from the constructor:
    /// </para>
    /// <code><![CDATA[
    /// public OrderPage(OrderService orders)
    /// {
    ///     InitializeComponent();
    ///     this.ObserveHandler(
    ///         onConnected: () => orders.Changed += OnOrdersChanged,
    ///         onDisconnected: () => orders.Changed -= OnOrdersChanged);
    /// }
    /// ]]></code>
    /// <para>
    /// The callbacks strictly alternate, connected first. An element that already has a handler when
    /// this is called is connected straight away, so calling it late misses nothing. A handler
    /// replaced by another is announced as a disconnect followed by a connect. A page is connected
    /// once the views on it are. How a connect falls against <c>Appearing</c> is the platform's, so do
    /// not rely on the one coming before the other.
    /// </para>
    /// <para>
    /// Connected means alive, not shown. A page underneath another pushed on a <c>Shell</c>, or on a
    /// flyout item that is not shown, stays connected for as long as it is kept; what should stop while
    /// a page is hidden belongs in <c>Appearing</c> and <c>Disappearing</c>. A disconnect is not the
    /// end either: an element given a handler again, such as a page kept and pushed again, is
    /// connected again, so pair the callbacks as attach and detach rather than as create and destroy.
    /// </para>
    /// <para>
    /// .NET MAUI disconnects what leaves with a page, not a view taken out of a layout while its page
    /// stays: disconnect that one, and whatever is inside it, with <c>DisconnectHandlers()</c>. Nor
    /// does it disconnect anything on its own when an application sets
    /// <c>HandlerProperties.DisconnectPolicy</c> to <c>Manual</c>.
    /// </para>
    /// <para>
    /// Do not touch <c>PlatformView</c> in <paramref name="onDisconnected"/>. By then it may already
    /// be gone, depending on the platform and on how the page closed; let go of what the element's own
    /// code took instead.
    /// </para>
    /// <para>
    /// An exception thrown by a callback is let out, to whatever changed the handler, as from any
    /// event handler; on the UI thread, where handlers change, an exception nothing catches ends the
    /// application, as any other does. A callback that can fail catches for itself. Either way, a
    /// connect that failed is still followed by its disconnect.
    /// </para>
    /// <para>
    /// Observing keeps nothing alive. It subscribes to the element's own events alone, so the element
    /// is collected exactly as it would be without it, and there is nothing to unsubscribe.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="element"/>, <paramref name="onConnected"/> or
    /// <paramref name="onDisconnected"/> is <see langword="null"/>.
    /// </exception>
    public static void ObserveHandler(this Element element, Action onConnected, Action onDisconnected)
    {
        ArgumentNullException.ThrowIfNull(element);

        var observer = new HandlerObserver(onConnected, onDisconnected);

        element.HandlerChanging += (_, e) => observer.OnHandlerChanging(e.OldHandler, e.NewHandler);
        element.HandlerChanged += (_, _) => observer.OnHandlerChanged(element.Handler);

        observer.OnHandlerChanged(element.Handler);
    }
}
