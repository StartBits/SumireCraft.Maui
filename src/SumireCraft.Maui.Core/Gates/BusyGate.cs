using SumireCraft.Maui.Lifetime;

namespace SumireCraft.Maui.Gates;

/// <summary>
/// The default <see cref="IBusyGate"/>.
/// </summary>
/// <remarks>
/// <para>
/// Owners are held weakly. A caller that enters and is then collected without leaving stops
/// counting towards <see cref="IsBusy"/>, so a forgotten <see cref="Exit"/> costs neither a leak
/// nor a permanently stuck user interface. Since the gate is normally a singleton that outlives
/// every page in the application, holding owners strongly would do both.
/// </para>
/// <para>
/// <see cref="BusyChanged"/> is always raised after the internal lock has been released, so a
/// handler is free to read <see cref="IsBusy"/> or call back into the gate.
/// </para>
/// <para>
/// The one thing the gate cannot report is a change caused purely by garbage collection.
/// <see cref="IsBusy"/> is accurate whenever it is read, but no event announces that an abandoned
/// owner has been reclaimed, because nothing was called to notice it.
/// </para>
/// </remarks>
public sealed class BusyGate : IBusyGate
{
    private readonly WeakReferenceList<object> _owners = new();
    private readonly Lock _sync = new();

    /// <summary>
    /// Counts the calls to <see cref="Clear"/>, so a scope can tell whether its entry is still
    /// there. Guarded by <see cref="_sync"/>.
    /// </summary>
    private int _generation;

    /// <inheritdoc />
    public event EventHandler? BusyChanged;

    /// <inheritdoc />
    public bool IsBusy
    {
        get
        {
            lock (_sync)
            {
                return HasLiveOwners();
            }
        }
    }

    /// <inheritdoc />
    public IDisposable Enter(object owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        bool changed;
        int generation;
        lock (_sync)
        {
            changed = !HasLiveOwners();
            _owners.Add(owner);
            generation = _generation;
        }

        if (changed)
        {
            OnBusyChanged();
        }

        return new Scope(this, owner, generation);
    }

    /// <inheritdoc />
    public void Exit(object owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        Leave(owner, generation: null);
    }

    /// <inheritdoc />
    public void Clear()
    {
        bool changed;
        lock (_sync)
        {
            changed = HasLiveOwners();
            _owners.Clear();
            _generation++;
        }

        if (changed)
        {
            OnBusyChanged();
        }
    }

    /// <summary>
    /// Removes one entry for <paramref name="owner"/>, unless the entry belongs to a generation
    /// that <see cref="Clear"/> has already swept away.
    /// </summary>
    /// <param name="owner">The object that was doing the work.</param>
    /// <param name="generation">
    /// The generation the entry was made in, or <see langword="null"/> to remove an entry
    /// whatever its generation, as a direct <see cref="Exit"/> does.
    /// </param>
    private void Leave(object owner, int? generation)
    {
        bool changed;
        lock (_sync)
        {
            if (generation is { } g && g != _generation)
            {
                return;
            }

            var wasBusy = HasLiveOwners();
            _owners.Remove(owner);
            changed = HasLiveOwners() != wasBusy;
        }

        if (changed)
        {
            OnBusyChanged();
        }
    }

    /// <summary>
    /// Drops the entries whose owner has been collected, then reports whether anyone is left.
    /// </summary>
    /// <remarks>
    /// Purging on every question keeps the list from growing without bound in a gate that lives as
    /// long as the application does. The caller must hold <see cref="_sync"/>.
    /// </remarks>
    private bool HasLiveOwners()
    {
        _owners.Purge();

        return _owners.Count > 0;
    }

    private void OnBusyChanged() => BusyChanged?.Invoke(this, EventArgs.Empty);

    private sealed class Scope(BusyGate gate, object owner, int generation) : IDisposable
    {
        private readonly object _owner = owner;
        private readonly int _generation = generation;
        private BusyGate? _gate = gate;

        public void Dispose() => Interlocked.Exchange(ref _gate, null)?.Leave(_owner, _generation);
    }
}
