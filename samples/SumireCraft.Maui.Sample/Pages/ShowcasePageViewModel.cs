using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SumireCraft.Maui.Badges;

namespace SumireCraft.Maui.Sample.Pages;

public sealed class ShowcasePageViewModel : INotifyPropertyChanged
{
    private const string Nothing = "Tap once, or five times";

    private string _opened = Nothing;
    private int _inboxCount;

    public ShowcasePageViewModel()
    {
        AddCommand = new Command<string>(Add);
        OpenInboxCommand = new Command(OpenInbox);
        OpenDebugRoomCommand = new Command(OpenDebugRoom);
        ResetCommand = new Command(Reset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand AddCommand { get; }

    public ICommand OpenInboxCommand { get; }

    public ICommand OpenDebugRoomCommand { get; }

    public ICommand ResetCommand { get; }

    /// <summary>
    /// Starts empty, so that the first message is what brings the badge out.
    /// </summary>
    public BadgeCounter Badge { get; } = new();

    /// <summary>
    /// What the last burst of taps opened.
    /// </summary>
    public string Opened
    {
        get => _opened;
        private set => Set(ref _opened, value);
    }

    /// <summary>
    /// How often the inbox has opened. Five taps leave it as it was: the burst is read once, when
    /// it ends, and never early.
    /// </summary>
    public int InboxCount
    {
        get => _inboxCount;
        private set => Set(ref _inboxCount, value);
    }

    private void Add(string? amount)
    {
        if (int.TryParse(amount, out var count))
        {
            Badge.Count += count;
        }
    }

    private void OpenInbox()
    {
        InboxCount++;
        Opened = "Inbox";
    }

    private void OpenDebugRoom() => Opened = "Debug room";

    private void Reset()
    {
        Badge.Count = 0;
        InboxCount = 0;
        Opened = Nothing;
    }

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
