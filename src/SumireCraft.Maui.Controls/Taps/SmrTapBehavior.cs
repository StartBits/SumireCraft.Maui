using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using SumireCraft.Maui.Taps;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// Runs a different command depending on how many times a view was tapped: once to open a screen,
/// five times to open the debug room. For a view that does not answer taps itself, not a
/// <see cref="Button"/>.
/// </summary>
/// <remarks>
/// <para>
/// The counting rules, and the reason a tap sometimes has to wait before it can be acted on, are
/// described on <see cref="TapSequence"/>. This behavior is the part that talks to .NET MAUI: it
/// puts a <see cref="TapGestureRecognizer"/> on the view, feeds what it hears into a sequence, and
/// runs the winning command on the user interface thread.
/// </para>
/// <para>
/// Exceptions thrown by a command are not caught here. Where they should go -- a log, an alert, a
/// crash reporter -- is the application's decision, and wrapping every command in this library's
/// idea of an answer would take that decision away.
/// </para>
/// <para>
/// A burst that is still being counted when the view goes away is still finished, and its command
/// still runs. Keeping a screen's work from outliving the screen is the application's business
/// here exactly as it is for every other control on the page.
/// </para>
/// <para>
/// Nothing runs until the tapping stops, which is the point but is also a cost. A view that only
/// ever wants a single tap should carry a <see cref="TapGestureRecognizer"/> instead and answer
/// straight away; this is for views where the number of taps is the question. One line asking for
/// <see cref="TapMatch.AtLeast"/> one tap is the other way to read that cost: it answers a run of
/// taps once, when it ends, which is what a view that must not be acted on twice wants.
/// </para>
/// <para>
/// Meant for views that do not answer taps themselves: a label, an image, a border. On a
/// <see cref="Button"/>, or anything else that answers taps on its own, what happens is up to the
/// platform and is not supported. With .NET MAUI 10, a button's own command stopped running on
/// Android, both ran on Windows, and on iOS and Mac Catalyst the button's command ran and the
/// behavior's did not.
/// </para>
/// <para>
/// One behavior can be on several views at once. Each view counts its own taps, taking the
/// behavior off one leaves the others as they were, and a line is read against the binding
/// context of the view that was tapped -- unless a context was set on the behavior itself, which
/// then applies to every view. Bindings on the behavior, such as one on <see cref="Interval"/>,
/// follow the view it was put on last. The views are not kept alive by the behavior.
/// </para>
/// <para>
/// Left open so that an application can wrap this, and <see cref="SmrTap"/>, in types of its own
/// and write those in its pages instead. A type that overrides <c>OnAttachedTo</c>,
/// <c>OnDetachingFrom</c> or <c>OnBindingContextChanged</c> has to call the base method, which is
/// where the tapping is wired up and the binding context handed down.
/// </para>
/// </remarks>
/// <example>
/// <code lang="xml"><![CDATA[
/// <Image Source="logo.png">
///     <Image.Behaviors>
///         <smr:SmrTapBehavior>
///             <smr:SmrTap Count="1" Command="{Binding OpenInboxCommand}" />
///             <smr:SmrTap Count="5" Match="AtLeast" Command="{Binding OpenDebugRoomCommand}" />
///         </smr:SmrTapBehavior>
///     </Image.Behaviors>
/// </Image>
/// ]]></code>
/// </example>
[ContentProperty(nameof(Taps))]
public class SmrTapBehavior : Behavior<View>
{
    /// <summary>
    /// The interval a behavior uses unless it is told otherwise, in milliseconds.
    /// </summary>
    public const int DefaultInterval = 300;

    /// <summary>
    /// Backs the <see cref="Interval"/> property.
    /// </summary>
    public static readonly BindableProperty IntervalProperty = BindableProperty.Create(
        nameof(Interval),
        typeof(int),
        typeof(SmrTapBehavior),
        defaultValue: DefaultInterval,
        propertyChanged: (bindable, _, _) => ((SmrTapBehavior)bindable).Restart(),
        coerceValue: (_, value) => (int)value > 0 ? value : DefaultInterval);

    private readonly ObservableCollection<SmrTap> _taps = [];

    /// <summary>
    /// What this behavior keeps for each view it is on. The views are held weakly, because .NET
    /// MAUI does not take a behavior off a view whose page has closed, and a behavior shared
    /// through a resource would otherwise keep every such page alive.
    /// </summary>
    private readonly ConditionalWeakTable<View, Attachment> _attachments = [];

    /// <summary>
    /// Numbers the attachments in the order they were made, so the latest can be found.
    /// </summary>
    private long _attachCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmrTapBehavior"/> class.
    /// </summary>
    public SmrTapBehavior() => _taps.CollectionChanged += OnTapsChanged;

    /// <summary>
    /// Gets or sets how long a burst of taps waits for the next one, in milliseconds, before the
    /// behavior decides what the burst meant. The default is <see cref="DefaultInterval"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Milliseconds rather than a <see cref="TimeSpan"/>, because this is a number an author tunes
    /// by feel and writes inline.
    /// </para>
    /// <para>
    /// Zero or less is taken as <see cref="DefaultInterval"/>. A burst that waited no time at all
    /// could never hold a second tap, so there is nothing such a value could mean, and it usually
    /// arrives through a binding while the page is being put together, where an exception would
    /// take the page down with it.
    /// </para>
    /// </remarks>
    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    /// <summary>
    /// Gets the lines this behavior chooses between. This is the content property, so they can be
    /// written straight inside the behavior in XAML.
    /// </summary>
    public IList<SmrTap> Taps => _taps;

    /// <summary>
    /// The clock the sequence measures with. Tests replace it; nothing else has a reason to.
    /// </summary>
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <inheritdoc/>
    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);

        if (_attachments.TryGetValue(bindable, out _))
        {
            return;
        }

        var attachment = new Attachment(bindable, ++_attachCount);
        _attachments.Add(bindable, attachment);
        bindable.GestureRecognizers.Add(attachment.Recognizer);

        Restart(attachment);
        Follow(attachment);
        PushBindingContext();
    }

    /// <inheritdoc/>
    protected override void OnDetachingFrom(View bindable)
    {
        if (_attachments.TryGetValue(bindable, out var attachment))
        {
            _attachments.Remove(bindable);
            bindable.GestureRecognizers.Remove(attachment.Recognizer);

            // Handed over rather than disposed, so that a burst already under way still finishes.
            // It holds nothing but a timer that stops itself once the burst settles.
            attachment.Sequence = null;

            // Stops following the view. The context already handed down stays where it is until
            // another view takes over, and a burst still settling here runs against this view's
            // own context in any case.
            if (attachment.Following is not null)
            {
                attachment.Following.Dispose();
                attachment.Following = null;

                if (Latest() is { } next)
                {
                    Follow(next);
                }
            }
        }

        base.OnDetachingFrom(bindable);
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        PushBindingContext();
    }

    /// <summary>
    /// Starts counting again with the interval as it now stands. Anything half-counted is dropped,
    /// which is the only sensible reading of an interval that changed underneath it.
    /// </summary>
    private void Restart()
    {
        foreach (var (_, attachment) in _attachments)
        {
            Restart(attachment);
        }
    }

    /// <inheritdoc cref="Restart()"/>
    private void Restart(Attachment attachment)
    {
        attachment.Sequence?.Dispose();

        var sequence = new TapSequence(_taps, Interval, TimeProvider);
        sequence.Triggered += (_, e) => OnTriggered(attachment, e);
        attachment.Sequence = sequence;
    }

    /// <summary>
    /// Makes this behavior take its binding context from <paramref name="attachment"/>'s view, and
    /// from no other.
    /// </summary>
    /// <remarks>
    /// .NET MAUI hands a view's binding context to its gesture recognizers but not to its
    /// behaviors. A behavior on several views can follow only one of them; the lines themselves are
    /// read against whichever view was tapped, so this matters only for bindings on the behavior.
    /// </remarks>
    private void Follow(Attachment attachment)
    {
        foreach (var (_, other) in _attachments)
        {
            other.Following?.Dispose();
            other.Following = null;
        }

        attachment.Following = this.InheritBindingContextFrom(attachment.View);
    }

    /// <summary>
    /// The attachment made last among those still in place, if any.
    /// </summary>
    private Attachment? Latest()
    {
        Attachment? latest = null;

        foreach (var (_, attachment) in _attachments)
        {
            if (latest is null || attachment.Order > latest.Order)
            {
                latest = attachment;
            }
        }

        return latest;
    }

    private void OnTriggered(Attachment attachment, TapTriggeredEventArgs e)
    {
        if (e.Trigger is not SmrTap tap)
        {
            return;
        }

        // A sequence can settle on the clock's own thread, which is not where a command that
        // navigates or touches a page may run.
        if (attachment.Dispatcher is { IsDispatchRequired: true } dispatcher)
        {
            dispatcher.Dispatch(() => Run(attachment, tap));
        }
        else
        {
            Run(attachment, tap);
        }
    }

    private void Run(Attachment attachment, SmrTap tap)
    {
        // Read against the view that was tapped, since each of the views this behavior is on can
        // have a view model of its own. A context written on the behavior itself still wins: that
        // is how a line in an item template reaches the page around it.
        if (!IsSet(BindingContextProperty))
        {
            SetInheritedBindingContext(tap, attachment.View.BindingContext);
        }

        var command = tap.Command;
        var parameter = tap.CommandParameter;

        if (command is not null && command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }

    private void OnTapsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null)
        {
            return;
        }

        foreach (SmrTap tap in e.NewItems)
        {
            SetInheritedBindingContext(tap, BindingContext);
        }
    }

    /// <summary>
    /// Hands this behavior's binding context down to every line it holds.
    /// </summary>
    /// <remarks>
    /// .NET MAUI passes a binding context neither to a behavior nor to what it holds. The behavior's
    /// own comes from <see cref="Follow"/> or from the page; without this, a command bound on a line
    /// would have nothing to resolve against.
    /// </remarks>
    private void PushBindingContext()
    {
        foreach (var tap in _taps)
        {
            SetInheritedBindingContext(tap, BindingContext);
        }
    }

    /// <summary>
    /// Everything this behavior keeps for one of the views it is on.
    /// </summary>
    private sealed class Attachment
    {
        public Attachment(View view, long order)
        {
            View = view;
            Order = order;
            Dispatcher = view.Dispatcher;
            Recognizer = new TapGestureRecognizer { Command = new Command(() => Sequence?.Tap()) };
        }

        public View View { get; }

        /// <summary>
        /// When this attachment was made, relative to the others.
        /// </summary>
        public long Order { get; }

        public IDispatcher? Dispatcher { get; }

        public TapGestureRecognizer Recognizer { get; }

        /// <summary>
        /// Counts the taps on this view, or <see langword="null"/> once the behavior has left it.
        /// </summary>
        public TapSequence? Sequence { get; set; }

        /// <summary>
        /// The link through which the behavior takes this view's binding context, while it is the
        /// view the behavior follows.
        /// </summary>
        public IDisposable? Following { get; set; }
    }
}
