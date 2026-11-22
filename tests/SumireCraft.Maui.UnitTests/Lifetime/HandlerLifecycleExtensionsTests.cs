using System.Runtime.CompilerServices;
using SumireCraft.Maui.Lifetime;

namespace SumireCraft.Maui.UnitTests.Lifetime;

// ObserveHandler on a view, with a handler that stands in for a platform's: given and taken away as
// .NET MAUI gives and takes one. When .NET MAUI does so on each platform -- a page pushed, popped,
// shown as a modal -- was measured on the platforms themselves.
public class HandlerLifecycleExtensionsTests
{
    private readonly List<string> _calls = [];

    private void Observe(VisualElement view, string name) =>
        view.ObserveHandler(() => _calls.Add($"{name} connected"), () => _calls.Add($"{name} disconnected"));

    [Fact]
    public void AViewGivenAHandler_IsConnected()
    {
        var view = new Label();
        Observe(view, "label");

        view.Handler = new FakeHandler();

        Assert.Equal(["label connected"], _calls);
    }

    [Fact]
    public void AViewThatHasAHandlerAlready_IsConnectedAtOnce()
    {
        var view = new Label { Handler = new FakeHandler() };

        Observe(view, "label");

        Assert.Equal(["label connected"], _calls);
    }

    [Fact]
    public void AViewLosingItsHandler_IsDisconnected()
    {
        var view = new Label { Handler = new FakeHandler() };
        Observe(view, "label");

        view.Handler = null;

        Assert.Equal(["label connected", "label disconnected"], _calls);
    }

    [Fact]
    public void AViewWhoseHandlerIsDisconnected_IsDisconnected()
    {
        var view = new Label { Handler = new FakeHandler() };
        Observe(view, "label");

        view.Handler.DisconnectHandler();

        Assert.Equal(["label connected", "label disconnected"], _calls);
    }

    // A view taken out of its layout while its page stays is not disconnected by .NET MAUI; the
    // application disconnects it, and whatever is inside it, with DisconnectHandlers.
    [Fact]
    public void DisconnectHandlers_DisconnectsTheViewAndWhatIsInside()
    {
        var label = new Label { Handler = new FakeHandler() };
        var layout = new VerticalStackLayout { Handler = new FakeHandler() };
        layout.Add(label);
        Observe(layout, "layout");
        Observe(label, "label");

        layout.DisconnectHandlers();

        Assert.Equal(["layout connected", "label connected", "layout disconnected", "label disconnected"], _calls);
    }

    [Fact]
    public void ACallbackThatThrows_LetsTheExceptionOut()
    {
        var view = new Label();
        view.ObserveHandler(() => throw new InvalidOperationException("connect"), () => { });

        Assert.Throws<InvalidOperationException>(() => view.Handler = new FakeHandler());
    }

    // Observing subscribes to the view's own events alone, so the view is let go of as it would be
    // without it.
    [Fact]
    public async Task AnObservedView_IsLetGoOf()
    {
        var reference = ObservedAndForgotten();

        for (var i = 0; i < 3 && reference.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Yield();
        }

        Assert.False(reference.IsAlive);
    }

    [Fact]
    public void ObserveHandler_ThrowsWhenTheElementIsNull() =>
        Assert.Throws<ArgumentNullException>(() => ((Element)null!).ObserveHandler(() => { }, () => { }));

    [Fact]
    public void ObserveHandler_ThrowsWhenACallbackIsNull()
    {
        var view = new Label();

        Assert.Throws<ArgumentNullException>(() => view.ObserveHandler(null!, () => { }));
        Assert.Throws<ArgumentNullException>(() => view.ObserveHandler(() => { }, null!));
    }

    // Not inlined, so that no local of the caller's keeps the view reachable.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private WeakReference ObservedAndForgotten()
    {
        var view = new Label { Handler = new FakeHandler() };
        Observe(view, "label");
        return new WeakReference(view);
    }

    // A handler with nothing behind it, which disconnects as .NET MAUI's own do: the platform view goes
    // first, then the view is told it has no handler.
    private sealed class FakeHandler : IViewHandler
    {
        public object? PlatformView { get; private set; } = new();

        public IView? VirtualView { get; private set; }

        IElement? IElementHandler.VirtualView => VirtualView;

        public IMauiContext? MauiContext => null;

        public bool HasContainer { get; set; }

        public object? ContainerView => null;

        public void SetMauiContext(IMauiContext mauiContext)
        {
        }

        public void SetVirtualView(IElement view) => VirtualView = (IView)view;

        public void UpdateValue(string property)
        {
        }

        public void Invoke(string command, object? args = null)
        {
        }

        public void DisconnectHandler()
        {
            PlatformView = null;
            VirtualView?.Handler = null;
        }

        public Size GetDesiredSize(double widthConstraint, double heightConstraint) => Size.Zero;

        public void PlatformArrange(Rect frame)
        {
        }
    }
}
