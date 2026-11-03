using System.Runtime.CompilerServices;

namespace SumireCraft.Maui.UnitTests.Extensions;

public class BindingContextExtensionsTests
{
    [Fact]
    public void AFollower_TakesTheHostsContextAtOnce()
    {
        var host = new Label { BindingContext = "first" };
        var follower = new Follower();

        using var link = follower.InheritBindingContextFrom(host);

        Assert.Equal("first", follower.BindingContext);
    }

    [Fact]
    public void AFollower_KeepsUpWhenTheHostsContextChanges()
    {
        var host = new Label { BindingContext = "first" };
        var follower = new Follower();
        using var link = follower.InheritBindingContextFrom(host);

        host.BindingContext = "second";

        Assert.Equal("second", follower.BindingContext);
    }

    [Fact]
    public void ABindingOnTheFollower_ResolvesAgainstTheHostsContext()
    {
        var host = new Label();
        var follower = new Follower();
        follower.SetBinding(Follower.NameProperty, nameof(Person.Name));
        using var link = follower.InheritBindingContextFrom(host);

        host.BindingContext = new Person("Sumire");

        Assert.Equal("Sumire", follower.Name);
    }

    [Fact]
    public void DisposingTheLink_StopsFollowingAndKeepsTheLastContext()
    {
        var host = new Label { BindingContext = "first" };
        var follower = new Follower();
        var link = follower.InheritBindingContextFrom(host);

        link.Dispose();
        host.BindingContext = "second";

        Assert.Equal("first", follower.BindingContext);
    }

    /// <remarks>
    /// The host outlives the follower more often than not -- a view outlives a behavior it lost --
    /// so a link that stayed subscribed would keep the follower alive for as long as the view.
    /// </remarks>
    [Fact]
    public void DisposingTheLink_LetsTheHostForgetTheFollower()
    {
        var host = new Label();

        var follower = LinkAndDispose(host);
        ForceCollection();

        Assert.False(follower.TryGetTarget(out _));
        GC.KeepAlive(host);
    }

    [Fact]
    public void DisposingTheLinkTwice_IsHarmless()
    {
        var host = new Label();
        var link = new Follower().InheritBindingContextFrom(host);

        link.Dispose();
        link.Dispose();
    }

    [Fact]
    public void AContextSetOnTheFollower_IsNeverOverruled()
    {
        var host = new Label { BindingContext = "host" };
        var follower = new Follower { BindingContext = "own" };
        using var link = follower.InheritBindingContextFrom(host);

        host.BindingContext = "host again";

        Assert.Equal("own", follower.BindingContext);
    }

    [Fact]
    public void ANullFollower_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => BindingContextExtensions.InheritBindingContextFrom(null!, new Label()));
    }

    [Fact]
    public void ANullHost_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new Follower().InheritBindingContextFrom(null!));
    }

    /// <summary>
    /// Links a follower to the host and lets go of it again, leaving the host as the only thing
    /// that could still be holding it. Kept in its own non-inlined method so no local in the caller
    /// keeps it alive.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Follower> LinkAndDispose(Label host)
    {
        var follower = new Follower();
        follower.InheritBindingContextFrom(host).Dispose();

        return new WeakReference<Follower>(follower);
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed record Person(string Name);

    /// <summary>
    /// Stands for anything a behavior holds of its own: bindable, but in nobody's visual tree.
    /// </summary>
    private sealed class Follower : BindableObject
    {
        public static readonly BindableProperty NameProperty =
            BindableProperty.Create(nameof(Name), typeof(string), typeof(Follower));

        public string? Name => (string?)GetValue(NameProperty);
    }
}
