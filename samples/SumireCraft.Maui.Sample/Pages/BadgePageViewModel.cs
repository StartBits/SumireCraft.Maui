using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SumireCraft.Maui.Badges;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.Sample.Pages;

public sealed class BadgePageViewModel : INotifyPropertyChanged
{
    private BadgePlacement _placement = BadgePlacement.TopEnd;
    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
    private int _inboxPresses;

    public BadgePageViewModel()
    {
        SetPlacementCommand = new Command<string>(SetPlacement);
        FlipFlowDirectionCommand = new Command(FlipFlowDirection);
        PressInboxCommand = new Command(() => InboxPresses++);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand SetPlacementCommand { get; }

    public ICommand FlipFlowDirectionCommand { get; }

    public ICommand PressInboxCommand { get; }

    /// <summary>
    /// How many times the badged button has been pressed, so that pressing it where the badge
    /// covers it can be seen to count.
    /// </summary>
    public int InboxPresses
    {
        get => _inboxPresses;
        private set => Set(ref _inboxPresses, value);
    }

    /// <summary>
    /// Which way the page runs. Turning it right to left is how an application in Arabic or Hebrew
    /// would see the badges, without changing the machine's language.
    /// </summary>
    public FlowDirection FlowDirection
    {
        get => _flowDirection;
        set => Set(ref _flowDirection, value);
    }

    /// <summary>
    /// The count behind the badge, under the default rules: zero draws no badge, and anything
    /// above 99 reads <c>99+</c>.
    /// </summary>
    public BadgeCounter Badge { get; } = new() { Count = 7 };

    public BadgePlacement Placement
    {
        get => _placement;
        set => Set(ref _placement, value);
    }

    private void SetPlacement(string? name)
    {
        if (Enum.TryParse<BadgePlacement>(name, out var placement))
        {
            Placement = placement;
        }
    }

    private void FlipFlowDirection() =>
        FlowDirection = FlowDirection == FlowDirection.RightToLeft
            ? FlowDirection.LeftToRight
            : FlowDirection.RightToLeft;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
