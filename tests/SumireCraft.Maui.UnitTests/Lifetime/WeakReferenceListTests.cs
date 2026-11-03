using System.Runtime.CompilerServices;
using SumireCraft.Maui.Lifetime;

namespace SumireCraft.Maui.UnitTests.Lifetime;

public class WeakReferenceListTests
{
    private sealed class Probe;

    [Fact]
    public void Add_IncreasesCount()
    {
        var list = new WeakReferenceList<Probe>();
        var probe = new Probe();

        list.Add(probe);

        Assert.Equal(1, list.Count);
        GC.KeepAlive(probe);
    }

    [Fact]
    public void Add_AllowsTheSameInstanceTwice()
    {
        var list = new WeakReferenceList<Probe>();
        var probe = new Probe();

        list.Add(probe);
        list.Add(probe);

        Assert.Equal(2, list.Count);
        Assert.Equal(2, list.Snapshot().Count);
        GC.KeepAlive(probe);
    }

    [Fact]
    public void Add_ThrowsWhenItemIsNull()
    {
        var list = new WeakReferenceList<Probe>();

        Assert.Throws<ArgumentNullException>(() => list.Add(null!));
    }

    [Fact]
    public void Constructor_ThrowsWhenCapacityIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WeakReferenceList<Probe>(-1));
    }

    [Fact]
    public void Contains_FindsATrackedInstance()
    {
        var list = new WeakReferenceList<Probe>();
        var tracked = new Probe();
        var untracked = new Probe();

        list.Add(tracked);

        Assert.True(list.Contains(tracked));
        Assert.False(list.Contains(untracked));
        GC.KeepAlive(tracked);
    }

    [Fact]
    public void Remove_RemovesOneEntryAndReportsWhetherItFoundOne()
    {
        var list = new WeakReferenceList<Probe>();
        var probe = new Probe();
        list.Add(probe);
        list.Add(probe);

        Assert.True(list.Remove(probe));
        Assert.Equal(1, list.Count);

        Assert.True(list.Remove(probe));
        Assert.Equal(0, list.Count);

        Assert.False(list.Remove(probe));
        GC.KeepAlive(probe);
    }

    [Fact]
    public void Clear_RemovesEveryEntry()
    {
        var list = new WeakReferenceList<Probe>();
        var probe = new Probe();
        list.Add(probe);
        list.Add(new Probe());

        list.Clear();

        Assert.Equal(0, list.Count);
        Assert.Empty(list.Snapshot());
        GC.KeepAlive(probe);
    }

    [Fact]
    public void Snapshot_ReturnsLiveInstancesInInsertionOrder()
    {
        var list = new WeakReferenceList<Probe>();
        var first = new Probe();
        var second = new Probe();
        list.Add(first);
        list.Add(second);

        var snapshot = list.Snapshot();

        Assert.Equal(2, snapshot.Count);
        Assert.Same(first, snapshot[0]);
        Assert.Same(second, snapshot[1]);
        GC.KeepAlive(first);
        GC.KeepAlive(second);
    }

    [Fact]
    public void Snapshot_IsEmptyForAnEmptyList()
    {
        var list = new WeakReferenceList<Probe>();

        Assert.Empty(list.Snapshot());
    }

    [Fact]
    public void CollectedEntriesRemainUntilPurgeIsCalled()
    {
        var list = new WeakReferenceList<Probe>();
        var survivor = new Probe();
        list.Add(survivor);
        AddCollectableProbes(list, 3);

        Assert.Equal(4, list.Count);

        ForceCollection();

        // Nothing is dropped on its own: the entries are still there.
        Assert.Equal(4, list.Count);

        Assert.Equal(3, list.Purge());
        Assert.Equal(1, list.Count);
        Assert.True(list.Contains(survivor));

        GC.KeepAlive(survivor);
    }

    [Fact]
    public void Snapshot_SkipsCollectedEntriesWithoutChangingTheList()
    {
        var list = new WeakReferenceList<Probe>();
        var survivor = new Probe();
        list.Add(survivor);
        AddCollectableProbes(list, 3);

        ForceCollection();

        var snapshot = list.Snapshot();

        Assert.Same(survivor, Assert.Single(snapshot));
        Assert.Equal(4, list.Count);

        GC.KeepAlive(survivor);
    }

    [Fact]
    public void Purge_ReturnsZeroWhenEveryEntryIsAlive()
    {
        var list = new WeakReferenceList<Probe>();
        var probe = new Probe();
        list.Add(probe);

        Assert.Equal(0, list.Purge());
        Assert.Equal(1, list.Count);
        GC.KeepAlive(probe);
    }

    [Fact]
    public void Add_IsSafeToCallFromMultipleThreads()
    {
        var list = new WeakReferenceList<Probe>();
        var probes = Enumerable.Range(0, 1000).Select(_ => new Probe()).ToArray();

        Parallel.ForEach(probes, list.Add);

        Assert.Equal(probes.Length, list.Count);
        Assert.Equal(probes.Length, list.Snapshot().Count);
        GC.KeepAlive(probes);
    }

    /// <summary>
    /// Adds instances that nothing else references, so the next collection can reclaim them.
    /// Kept in its own non-inlined method to make sure no local in the caller keeps them alive.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddCollectableProbes(WeakReferenceList<Probe> list, int count)
    {
        for (var i = 0; i < count; i++)
        {
            list.Add(new Probe());
        }
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
