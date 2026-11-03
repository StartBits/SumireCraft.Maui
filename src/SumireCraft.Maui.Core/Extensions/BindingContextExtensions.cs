namespace SumireCraft.Maui;

/// <summary>
/// Extensions for keeping one bindable object's binding context in step with another's.
/// </summary>
public static class BindingContextExtensions
{
    /// <summary>
    /// Makes one bindable object follow another's binding context, until the returned link is
    /// disposed.
    /// </summary>
    /// <param name="follower">The object whose binding context is to be kept in step.</param>
    /// <param name="host">The object to take the binding context from.</param>
    /// <returns>
    /// A link to dispose when the two should stop being connected -- when a behavior detaches,
    /// typically.
    /// </returns>
    /// <remarks>
    /// <para>
    /// .NET MAUI passes a view's binding context to the gesture recognizers attached to it, but not
    /// to its behaviors, nor to anything a behavior holds of its own -- a collection of options, a
    /// nested bindable. Those are left with no binding context at all, so a <c>{Binding}</c>
    /// written on them silently resolves to nothing. This is the way out, and it is a method rather
    /// than a base class on purpose: a behavior can only have one base class, and the useful one
    /// usually belongs to whichever library it came from.
    /// </para>
    /// <code>
    /// protected override void OnAttachedTo(View bindable)
    /// {
    ///     base.OnAttachedTo(bindable);
    ///     _context = this.InheritBindingContextFrom(bindable);
    /// }
    ///
    /// protected override void OnDetachingFrom(View bindable)
    /// {
    ///     _context?.Dispose();
    ///     _context = null;
    ///     base.OnDetachingFrom(bindable);
    /// }
    /// </code>
    /// <para>
    /// A behavior that can be on more than one view at a time needs one link per view, kept beside
    /// whatever else it holds for that view. The single field above would be overwritten by the
    /// second view, leaving the first link still subscribed and the behavior following whichever
    /// view it was put on last.
    /// </para>
    /// <para>
    /// A context the follower was given deliberately is left alone, because an inherited context
    /// never overrules one that was written down. Disposing the link stops the following but leaves
    /// the last context in place, so a binding with work still to finish can finish it.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="follower"/> or <paramref name="host"/> is <see langword="null"/>.
    /// </exception>
    public static IDisposable InheritBindingContextFrom(this BindableObject follower, BindableObject host)
    {
        ArgumentNullException.ThrowIfNull(follower);
        ArgumentNullException.ThrowIfNull(host);

        return new InheritedBindingContext(follower, host);
    }

    private sealed class InheritedBindingContext : IDisposable
    {
        private readonly BindableObject _follower;

        /// <summary>
        /// The object being followed, or <see langword="null"/> once the link is disposed.
        /// </summary>
        private BindableObject? _host;

        public InheritedBindingContext(BindableObject follower, BindableObject host)
        {
            _follower = follower;
            _host = host;

            host.BindingContextChanged += OnHostBindingContextChanged;
            Apply();
        }

        public void Dispose()
        {
            if (_host is null)
            {
                return;
            }

            _host.BindingContextChanged -= OnHostBindingContextChanged;
            _host = null;
        }

        private void OnHostBindingContextChanged(object? sender, EventArgs e) => Apply();

        private void Apply()
        {
            if (_host is not null)
            {
                BindableObject.SetInheritedBindingContext(_follower, _host.BindingContext);
            }
        }
    }
}
