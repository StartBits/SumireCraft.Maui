using SumireCraft.Maui.Lifetime;

namespace SumireCraft.Maui.UnitTests.Lifetime;

// The decisions behind ObserveHandler, apart from any element: which handler change is a connect, which
// a disconnect, and which is neither.
public class HandlerObserverTests
{
    private readonly List<string> _calls = [];

    private HandlerObserver Observe() =>
        new(() => _calls.Add("connected"), () => _calls.Add("disconnected"));

    [Fact]
    public void AHandlerArriving_Connects()
    {
        var observer = Observe();
        var handler = new object();

        observer.OnHandlerChanging(null, handler);
        observer.OnHandlerChanged(handler);

        Assert.Equal(["connected"], _calls);
    }

    // A page's handler changes before the views inside it are connected, and is in place, with them,
    // only when it has changed.
    [Fact]
    public void Connecting_WaitsUntilTheHandlerIsInPlace()
    {
        var observer = Observe();

        observer.OnHandlerChanging(null, new object());

        Assert.Empty(_calls);
    }

    [Fact]
    public void TheHandlerLeaving_Disconnects()
    {
        var observer = Observe();
        var handler = new object();
        observer.OnHandlerChanged(handler);

        observer.OnHandlerChanging(handler, null);

        Assert.Equal(["connected", "disconnected"], _calls);
    }

    [Fact]
    public void ASwapWithNoGapBetween_DisconnectsTheOldOneBeforeConnectingTheNewOne()
    {
        var observer = Observe();
        var first = new object();
        var second = new object();
        observer.OnHandlerChanged(first);

        observer.OnHandlerChanging(first, second);
        observer.OnHandlerChanged(second);

        Assert.Equal(["connected", "disconnected", "connected"], _calls);
    }

    [Fact]
    public void AHandlerComingBack_ConnectsAgain()
    {
        var observer = Observe();
        var handler = new object();
        observer.OnHandlerChanged(handler);
        observer.OnHandlerChanging(handler, null);
        observer.OnHandlerChanged(null);

        observer.OnHandlerChanging(null, handler);
        observer.OnHandlerChanged(handler);

        Assert.Equal(["connected", "disconnected", "connected"], _calls);
    }

    [Fact]
    public void BeingToldTwiceAboutTheSameHandler_ConnectsOnce()
    {
        var observer = Observe();
        var handler = new object();

        observer.OnHandlerChanged(handler);
        observer.OnHandlerChanged(handler);

        Assert.Equal(["connected"], _calls);
    }

    [Fact]
    public void TheSameHandlerSetAgain_ChangesNothing()
    {
        var observer = Observe();
        var handler = new object();
        observer.OnHandlerChanged(handler);

        observer.OnHandlerChanging(handler, handler);
        observer.OnHandlerChanged(handler);

        Assert.Equal(["connected"], _calls);
    }

    [Fact]
    public void NoHandlerAtAll_ConnectsNothing()
    {
        var observer = Observe();

        observer.OnHandlerChanged(null);

        Assert.Empty(_calls);
    }

    [Fact]
    public void AHandlerLeavingThatNeverConnected_DisconnectsNothing()
    {
        var observer = Observe();

        observer.OnHandlerChanging(new object(), null);

        Assert.Empty(_calls);
    }

    // A callback that throws lets the exception out, and the observer stays as it was told: a connect
    // that failed halfway is still followed by its disconnect, so that what it did take is let go of.
    [Fact]
    public void AConnectThatThrows_IsStillFollowedByItsDisconnect()
    {
        var disconnected = 0;
        var observer = new HandlerObserver(() => throw new InvalidOperationException("connect"), () => disconnected++);
        var handler = new object();

        Assert.Throws<InvalidOperationException>(() => observer.OnHandlerChanged(handler));
        observer.OnHandlerChanging(handler, null);

        Assert.Equal(1, disconnected);
    }

    [Fact]
    public void ADisconnectThatThrows_IsStillFollowedByTheNextConnect()
    {
        var connected = 0;
        var observer = new HandlerObserver(() => connected++, () => throw new InvalidOperationException("disconnect"));
        var handler = new object();
        observer.OnHandlerChanged(handler);

        Assert.Throws<InvalidOperationException>(() => observer.OnHandlerChanging(handler, null));
        observer.OnHandlerChanged(handler);

        Assert.Equal(2, connected);
    }

    [Fact]
    public void Constructor_ThrowsWhenOnConnectedIsNull() =>
        Assert.Throws<ArgumentNullException>(() => new HandlerObserver(null!, () => { }));

    [Fact]
    public void Constructor_ThrowsWhenOnDisconnectedIsNull() =>
        Assert.Throws<ArgumentNullException>(() => new HandlerObserver(() => { }, null!));
}
