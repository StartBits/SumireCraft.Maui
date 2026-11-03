using System.Runtime.CompilerServices;
using SumireCraft.Maui.Gates;

namespace SumireCraft.Maui.UnitTests.Gates;

public class BusyGateTests
{
    private sealed class Owner;

    [Fact]
    public void ANewGateIsNotBusy()
    {
        var gate = new BusyGate();

        Assert.False(gate.IsBusy);
    }

    [Fact]
    public void Enter_MakesTheGateBusy()
    {
        var gate = new BusyGate();
        var owner = new Owner();

        gate.Enter(owner);

        Assert.True(gate.IsBusy);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void DisposingTheScope_LeavesTheGate()
    {
        var gate = new BusyGate();
        var owner = new Owner();

        using (gate.Enter(owner))
        {
            Assert.True(gate.IsBusy);
        }

        Assert.False(gate.IsBusy);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void Exit_LeavesTheGate()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        gate.Enter(owner);

        gate.Exit(owner);

        Assert.False(gate.IsBusy);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void EnteringTwice_RequiresTwoExits()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        gate.Enter(owner);
        gate.Enter(owner);

        gate.Exit(owner);
        Assert.True(gate.IsBusy);

        gate.Exit(owner);
        Assert.False(gate.IsBusy);

        GC.KeepAlive(owner);
    }

    [Fact]
    public void Exit_DoesNothingForAnOwnerThatNeverEntered()
    {
        var gate = new BusyGate();
        var owner = new Owner();

        gate.Exit(owner);

        Assert.False(gate.IsBusy);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void Exit_DoesNotReleaseSomebodyElse()
    {
        var gate = new BusyGate();
        var inside = new Owner();
        var outside = new Owner();
        gate.Enter(inside);

        gate.Exit(outside);

        Assert.True(gate.IsBusy);
        GC.KeepAlive(inside);
        GC.KeepAlive(outside);
    }

    [Fact]
    public void OwnersAreTrackedIndependently()
    {
        var gate = new BusyGate();
        var first = new Owner();
        var second = new Owner();
        gate.Enter(first);
        gate.Enter(second);

        gate.Exit(first);
        Assert.True(gate.IsBusy);

        gate.Exit(second);
        Assert.False(gate.IsBusy);

        GC.KeepAlive(first);
        GC.KeepAlive(second);
    }

    [Fact]
    public void DisposingAScopeTwice_LeavesTheGateOnlyOnce()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        gate.Enter(owner);
        var scope = gate.Enter(owner);

        scope.Dispose();
        scope.Dispose();

        // The second dispose must not undo the first Enter as well.
        Assert.True(gate.IsBusy);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void BusyChanged_IsRaisedWhenTheGateBecomesBusy()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        var raised = new List<bool>();
        gate.BusyChanged += (_, _) => raised.Add(gate.IsBusy);

        gate.Enter(owner);

        Assert.Equal([true], raised);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void BusyChanged_IsRaisedWhenTheGateBecomesIdle()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        gate.Enter(owner);
        var raised = new List<bool>();
        gate.BusyChanged += (_, _) => raised.Add(gate.IsBusy);

        gate.Exit(owner);

        Assert.Equal([false], raised);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void BusyChanged_IsNotRaisedWhileTheGateStaysBusy()
    {
        var gate = new BusyGate();
        var first = new Owner();
        var second = new Owner();
        gate.Enter(first);
        var raised = new List<bool>();
        gate.BusyChanged += (_, _) => raised.Add(gate.IsBusy);

        gate.Enter(second);
        gate.Enter(first);
        gate.Exit(first);
        gate.Exit(second);

        Assert.Empty(raised);
        GC.KeepAlive(first);
        GC.KeepAlive(second);
    }

    [Fact]
    public void BusyChanged_IsNotRaisedWhenNothingChanges()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        var raised = new List<bool>();
        gate.BusyChanged += (_, _) => raised.Add(gate.IsBusy);

        gate.Exit(owner);

        Assert.Empty(raised);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void Clear_LeavesTheGateIdle()
    {
        var gate = new BusyGate();
        var first = new Owner();
        var second = new Owner();
        gate.Enter(first);
        gate.Enter(first);
        gate.Enter(second);

        gate.Clear();

        Assert.False(gate.IsBusy);
        GC.KeepAlive(first);
        GC.KeepAlive(second);
    }

    [Fact]
    public void Clear_RaisesBusyChangedOnce()
    {
        var gate = new BusyGate();
        var first = new Owner();
        var second = new Owner();
        gate.Enter(first);
        gate.Enter(second);
        var raised = new List<bool>();
        gate.BusyChanged += (_, _) => raised.Add(gate.IsBusy);

        gate.Clear();

        Assert.Equal([false], raised);
        GC.KeepAlive(first);
        GC.KeepAlive(second);
    }

    [Fact]
    public void Clear_DoesNotRaiseBusyChangedWhenTheGateIsAlreadyIdle()
    {
        var gate = new BusyGate();
        var raised = new List<bool>();
        gate.BusyChanged += (_, _) => raised.Add(gate.IsBusy);

        gate.Clear();

        Assert.Empty(raised);
    }

    [Fact]
    public void TheGateCanBeEnteredAgainAfterClear()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        gate.Enter(owner);
        gate.Clear();
        var raised = new List<bool>();
        gate.BusyChanged += (_, _) => raised.Add(gate.IsBusy);

        gate.Enter(owner);

        Assert.True(gate.IsBusy);
        Assert.Equal([true], raised);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void AScopeFromBeforeClear_DoesNotReleaseALaterEnter()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        var stale = gate.Enter(owner);
        gate.Clear();
        gate.Enter(owner);

        // The work that was cleared out finishes late. Its scope belongs to the entry that Clear
        // already removed, so it must not take the new one with it.
        stale.Dispose();

        Assert.True(gate.IsBusy);
        GC.KeepAlive(owner);
    }

    [Fact]
    public void Enter_ThrowsWhenOwnerIsNull()
    {
        var gate = new BusyGate();

        Assert.Throws<ArgumentNullException>(() => gate.Enter(null!));
    }

    [Fact]
    public void Exit_ThrowsWhenOwnerIsNull()
    {
        var gate = new BusyGate();

        Assert.Throws<ArgumentNullException>(() => gate.Exit(null!));
    }

    [Fact]
    public void AnOwnerThatWasCollected_NoLongerHoldsTheGate()
    {
        var gate = new BusyGate();
        EnterAndAbandon(gate);

        Assert.True(gate.IsBusy);

        ForceCollection();

        // The gate holds owners weakly, so a forgotten Exit heals itself.
        Assert.False(gate.IsBusy);
    }

    [Fact]
    public async Task AHandlerMayCallBackIntoTheGateWithoutDeadlocking()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        var other = new Owner();
        var observed = new List<bool>();

        gate.BusyChanged += (_, _) =>
        {
            // Reading and writing from inside a handler must not deadlock, which it would
            // if the event were raised while the gate held its lock.
            var isBusy = gate.IsBusy;
            observed.Add(isBusy);

            if (isBusy)
            {
                gate.Exit(other);
            }
        };

        var cancellation = TestContext.Current.CancellationToken;
        var entering = Task.Run(
            () =>
            {
                gate.Enter(owner);
                gate.Exit(owner);
            },
            cancellation);
        var finished = await Task.WhenAny(entering, Task.Delay(TimeSpan.FromSeconds(5), cancellation));

        Assert.True(
            ReferenceEquals(entering, finished),
            "The gate deadlocked while raising BusyChanged.");
        await entering;
        Assert.Equal([true, false], observed);

        GC.KeepAlive(owner);
        GC.KeepAlive(other);
    }

    // BusyChanged says only that something changed. Two changes need not reach a handler in the
    // order they happened, so a handler that remembered what each one said could end up believing
    // the opposite of the truth, with no further change on its way to put it right. The gate itself
    // is always right, and a handler that asks it ends on the right answer.

    /// <remarks>
    /// The earlier handler starts the next piece of work the moment the gate goes idle. Its enter
    /// is raised inside the exit, so the later handler hears busy before it hears idle.
    /// </remarks>
    [Fact]
    public void AHandlerThatAsksTheGate_EndsRight_WhenAnEarlierHandlerEntersIt()
    {
        var gate = new BusyGate();
        var current = new Owner();
        var queued = new Owner();
        gate.Enter(current);

        var started = false;
        gate.BusyChanged += (_, _) =>
        {
            if (!gate.IsBusy && !started)
            {
                started = true;
                gate.Enter(queued);
            }
        };

        var seen = new List<bool>();
        gate.BusyChanged += (_, _) => seen.Add(gate.IsBusy);

        gate.Exit(current);

        Assert.True(gate.IsBusy);
        Assert.True(seen[^1], "The handler was left believing the gate was idle.");
        GC.KeepAlive(current);
        GC.KeepAlive(queued);
    }

    /// <remarks>
    /// One thread enters and is held up in the earlier handler; meanwhile another thread leaves,
    /// and its change reaches the later handler first.
    /// </remarks>
    [Fact]
    public async Task AHandlerThatAsksTheGate_EndsRight_WhenChangesOnTwoThreadsArriveOutOfOrder()
    {
        var gate = new BusyGate();
        var owner = new Owner();
        using var heardBusy = new ManualResetEventSlim();
        using var carryOn = new ManualResetEventSlim();
        var testThread = Environment.CurrentManagedThreadId;

        gate.BusyChanged += (_, _) =>
        {
            if (Environment.CurrentManagedThreadId != testThread)
            {
                heardBusy.Set();
                carryOn.Wait(TimeSpan.FromSeconds(5));
            }
        };

        var seen = new List<bool>();
        gate.BusyChanged += (_, _) =>
        {
            lock (seen)
            {
                seen.Add(gate.IsBusy);
            }
        };

        var cancellation = TestContext.Current.CancellationToken;
        var entering = Task.Run(() => gate.Enter(owner), cancellation);
        Assert.True(heardBusy.Wait(TimeSpan.FromSeconds(5), cancellation), "The enter was never raised.");
        gate.Exit(owner);
        carryOn.Set();
        await entering;

        Assert.False(gate.IsBusy);
        Assert.Equal(2, seen.Count);
        Assert.False(seen[^1], "The handler was left believing the gate was busy.");
        GC.KeepAlive(owner);
    }

    [Fact]
    public void EnterAndExitAreSafeToCallFromMultipleThreads()
    {
        var gate = new BusyGate();
        var owners = Enumerable.Range(0, 1000).Select(_ => new Owner()).ToArray();

        Parallel.ForEach(owners, owner =>
        {
            using (gate.Enter(owner))
            {
                Assert.True(gate.IsBusy);
            }
        });

        Assert.False(gate.IsBusy);
        GC.KeepAlive(owners);
    }

    /// <summary>
    /// Enters the gate with an owner that nothing else references, so the next collection can
    /// reclaim it. Kept in its own non-inlined method so no local in the caller keeps it alive.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void EnterAndAbandon(BusyGate gate) => gate.Enter(new Owner());

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
