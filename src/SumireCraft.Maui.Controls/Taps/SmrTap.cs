using System.Windows.Input;
using SumireCraft.Maui.Taps;

namespace SumireCraft.Maui.Controls;

/// <summary>
/// One line of a <see cref="SmrTapBehavior"/>: a number of taps, and the command to run when the
/// taps that arrived meant this one.
/// </summary>
/// <remarks>
/// This is a <see cref="BindableObject"/> rather than a plain class so that
/// <see cref="Command"/> can be bound. <see cref="SmrTapBehavior"/> passes the binding context of
/// the view that was tapped down to the line it runs, so a binding here resolves against the same
/// view model as one written on that view. A context set on the behavior itself takes its place.
/// </remarks>
public class SmrTap : BindableObject, ITapTrigger
{
    /// <summary>
    /// Backs the <see cref="Count"/> property.
    /// </summary>
    public static readonly BindableProperty CountProperty = BindableProperty.Create(
        nameof(Count),
        typeof(int),
        typeof(SmrTap),
        defaultValue: 1);

    /// <summary>
    /// Backs the <see cref="Match"/> property.
    /// </summary>
    public static readonly BindableProperty MatchProperty = BindableProperty.Create(
        nameof(Match),
        typeof(TapMatch),
        typeof(SmrTap),
        defaultValue: TapMatch.Exactly);

    /// <summary>
    /// Backs the <see cref="Command"/> property.
    /// </summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(SmrTap));

    /// <summary>
    /// Backs the <see cref="CommandParameter"/> property.
    /// </summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(SmrTap));

    /// <summary>
    /// Gets or sets the number of taps this line is interested in. The default is one. Anything
    /// below one is ignored, and the line never wins.
    /// </summary>
    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>
    /// Gets or sets how <see cref="Count"/> is compared with the taps that arrived. The default is
    /// <see cref="TapMatch.Exactly"/>.
    /// </summary>
    public TapMatch Match
    {
        get => (TapMatch)GetValue(MatchProperty);
        set => SetValue(MatchProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to run when this line wins.
    /// </summary>
    /// <remarks>
    /// A command whose <see cref="ICommand.CanExecute"/> says no is not run, and nothing else runs
    /// in its place. A line that loses its turn this way is not a reason to fall back to one asking
    /// for fewer taps: the taps meant what they meant.
    /// </remarks>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets what to pass to <see cref="Command"/>.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}
