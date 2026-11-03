namespace SumireCraft.Maui.Lifetime;

/// <summary>
/// Holds weak references to <typeparamref name="T"/> instances, so that tracking an object never
/// keeps it alive.
/// </summary>
/// <typeparam name="T">The reference type to track.</typeparam>
/// <remarks>
/// <para>
/// Entries are never dropped on their own. Once a tracked instance has been collected, its entry
/// stays in the list until <see cref="Purge"/>, <see cref="Remove"/> or <see cref="Clear"/> is
/// called. Nothing happens behind the caller's back, which makes the cost of each operation
/// predictable and keeps collection during enumeration from surprising anyone.
/// </para>
/// <para>
/// As a consequence, <see cref="Count"/> reports how many entries the list holds, not how many of
/// them still point at a live instance. Use <see cref="Snapshot"/> to obtain the instances that
/// are still alive.
/// </para>
/// <para>
/// Instances are matched by reference. Overriding <see cref="object.Equals(object)"/> on
/// <typeparamref name="T"/> does not affect <see cref="Contains"/> or <see cref="Remove"/>,
/// because the list tracks object identity rather than value. Duplicates are allowed: adding the
/// same instance twice produces two entries.
/// </para>
/// <para>All members are safe to call from multiple threads.</para>
/// </remarks>
public sealed class WeakReferenceList<T>
    where T : class
{
    private readonly List<WeakReference<T>> _entries;
    private readonly Lock _gate = new();

    /// <summary>
    /// Initializes a new, empty <see cref="WeakReferenceList{T}"/>.
    /// </summary>
    public WeakReferenceList() => _entries = [];

    /// <summary>
    /// Initializes a new, empty <see cref="WeakReferenceList{T}"/> with room for the given number
    /// of entries.
    /// </summary>
    /// <param name="capacity">The number of entries the list can hold before it has to grow.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is negative.</exception>
    public WeakReferenceList(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        _entries = new List<WeakReference<T>>(capacity);
    }

    /// <summary>
    /// Gets the number of entries in the list, including entries whose target has already been
    /// collected.
    /// </summary>
    /// <remarks>
    /// This is deliberately not the number of live instances. Call <see cref="Purge"/> first, or
    /// read the count from <see cref="Snapshot"/>, when the live count is what you need.
    /// </remarks>
    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count;
            }
        }
    }

    /// <summary>
    /// Adds a weak reference to <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The instance to track.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_gate)
        {
            _entries.Add(new WeakReference<T>(item));
        }
    }

    /// <summary>
    /// Removes the first entry that points at <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The instance to stop tracking.</param>
    /// <returns>
    /// <see langword="true"/> if an entry was removed; <see langword="false"/> if the list holds
    /// no live entry for <paramref name="item"/>.
    /// </returns>
    /// <remarks>
    /// Entries whose target has been collected are left alone, even the ones passed over while
    /// searching. Use <see cref="Purge"/> to drop those.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
    public bool Remove(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_gate)
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].TryGetTarget(out var target) && ReferenceEquals(target, item))
                {
                    _entries.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Determines whether the list holds a live entry for <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The instance to look for.</param>
    /// <returns><see langword="true"/> if the instance is being tracked and still alive.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
    public bool Contains(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_gate)
        {
            foreach (var entry in _entries)
            {
                if (entry.TryGetTarget(out var target) && ReferenceEquals(target, item))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Removes every entry, live or not.
    /// </summary>
    public void Clear()
    {
        lock (_gate)
        {
            _entries.Clear();
        }
    }

    /// <summary>
    /// Removes the entries whose target has been collected.
    /// </summary>
    /// <returns>The number of entries removed.</returns>
    /// <remarks>
    /// This is the only operation that shrinks the list on account of garbage collection, and it
    /// runs only when called.
    /// </remarks>
    public int Purge()
    {
        lock (_gate)
        {
            return _entries.RemoveAll(static entry => !entry.TryGetTarget(out _));
        }
    }

    /// <summary>
    /// Returns the instances that are still alive, in the order they were added.
    /// </summary>
    /// <returns>
    /// A new collection holding a strong reference to each surviving instance. It is detached from
    /// the list: later changes to either one do not affect the other.
    /// </returns>
    /// <remarks>
    /// The returned instances are kept alive for as long as the caller holds on to the result.
    /// The list itself is left untouched, so entries whose target was collected remain in place.
    /// </remarks>
    public IReadOnlyList<T> Snapshot()
    {
        lock (_gate)
        {
            if (_entries.Count == 0)
            {
                return [];
            }

            var alive = new List<T>(_entries.Count);

            foreach (var entry in _entries)
            {
                if (entry.TryGetTarget(out var target))
                {
                    alive.Add(target);
                }
            }

            return alive.Count == 0 ? [] : alive.ToArray();
        }
    }
}
