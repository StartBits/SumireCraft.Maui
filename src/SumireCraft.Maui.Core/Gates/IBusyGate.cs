namespace SumireCraft.Maui.Gates;

/// <summary>
/// Tracks the objects that are currently doing work, so the rest of the application can tell
/// whether anything is in flight. The usual reason to ask is to keep a second tap on a button from
/// starting the same work twice.
/// </summary>
/// <remarks>
/// <para>
/// Think of a ticket gate. Callers enter as they start work and leave as they finish, and the gate
/// reports whether anyone is still inside. It never blocks anyone: it only answers the question.
/// Deciding what to do with the answer -- ignoring a tap, disabling a button -- is the caller's
/// job.
/// </para>
/// <para>
/// A single instance is meant to be shared across the whole application through dependency
/// injection, so that a call started in a networking layer is visible to the user interface.
/// Nothing here touches the UI or a particular thread.
/// </para>
/// </remarks>
public interface IBusyGate
{
    /// <summary>
    /// Gets a value indicating whether any object is currently inside the gate.
    /// </summary>
    bool IsBusy { get; }

    /// <summary>
    /// Occurs after <see cref="IsBusy"/> has changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The event says only that something changed, and carries no value: read
    /// <see cref="IsBusy"/> for that. Two changes need not reach a handler in the order they
    /// happened -- they may come from two threads at once, or one may be raised inside the other
    /// when a handler enters the gate itself -- so a handler that remembered each change in turn
    /// could end up believing the opposite of the truth. The property is always right.
    /// </para>
    /// <para>
    /// Raised on whichever thread caused the change, which is rarely the UI thread. A handler that
    /// touches the user interface marshals for itself, and reads <see cref="IsBusy"/> once it is
    /// there rather than before it sets off.
    /// </para>
    /// <para>
    /// The gate usually lives as long as the application, and like any event it keeps its handlers
    /// alive. A page or view model that subscribes has to unsubscribe when it is done, or the gate
    /// holds on to it for good.
    /// </para>
    /// </remarks>
    event EventHandler? BusyChanged;

    /// <summary>
    /// Records that <paramref name="owner"/> has started work.
    /// </summary>
    /// <param name="owner">The object doing the work.</param>
    /// <returns>
    /// A scope that calls <see cref="Exit"/> for <paramref name="owner"/> when disposed. Wrap it in
    /// a <c>using</c> statement so the work is recorded as finished even if it throws. Callers that
    /// need to finish in a different method may discard the scope and call <see cref="Exit"/>
    /// themselves.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="owner"/> is <see langword="null"/>.</exception>
    IDisposable Enter(object owner);

    /// <summary>
    /// Records that <paramref name="owner"/> has finished one piece of work.
    /// </summary>
    /// <param name="owner">The object that was doing the work.</param>
    /// <remarks>
    /// Each call undoes a single <see cref="Enter"/>. An object that entered twice is still inside
    /// after one call. Calling this for an object that never entered does nothing.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="owner"/> is <see langword="null"/>.</exception>
    void Exit(object owner);

    /// <summary>
    /// Sends every object out of the gate at once, leaving it idle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the way out when <see cref="IsBusy"/> has become stuck -- an <see cref="Exit"/>
    /// that was never reached, for example -- and something outside the gate, such as a
    /// timeout, has decided to give up waiting. It does not stop the work itself: the gate never
    /// owned that work, only the record of it.
    /// </para>
    /// <para>
    /// Scopes returned by <see cref="Enter"/> before the call do nothing when they are disposed
    /// afterwards, so work that finishes late cannot undo an <see cref="Enter"/> made after the
    /// gate was cleared. A direct call to <see cref="Exit"/> has no scope to tell it apart and
    /// still removes one entry for its owner, if there is one.
    /// </para>
    /// </remarks>
    void Clear();
}
