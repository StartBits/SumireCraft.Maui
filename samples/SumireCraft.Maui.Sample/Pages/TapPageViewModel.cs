using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SumireCraft.Maui.Controls;

namespace SumireCraft.Maui.Sample.Pages;

public sealed class TapPageViewModel : INotifyPropertyChanged
{
    private const string Nothing = "nothing yet";

    private int _inboxCount;
    private int _debugRoomCount;
    private int _doubleTapCount;
    private int _wrappedDoubleTapCount;
    private string _lastAction = Nothing;
    private string _intervalText = string.Empty;

    public TapPageViewModel()
    {
        OpenInboxCommand = new Command(OpenInbox);
        OpenDebugRoomCommand = new Command(OpenDebugRoom);
        DoubleTapCommand = new Command(DoubleTap);
        WrappedDoubleTapCommand = new Command(WrappedDoubleTap);
        ResetCommand = new Command(Reset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand OpenInboxCommand { get; }

    public ICommand OpenDebugRoomCommand { get; }

    public ICommand DoubleTapCommand { get; }

    public ICommand WrappedDoubleTapCommand { get; }

    public ICommand ResetCommand { get; }

    public int InboxCount
    {
        get => _inboxCount;
        private set => Set(ref _inboxCount, value);
    }

    public int DebugRoomCount
    {
        get => _debugRoomCount;
        private set => Set(ref _debugRoomCount, value);
    }

    public int DoubleTapCount
    {
        get => _doubleTapCount;
        private set => Set(ref _doubleTapCount, value);
    }

    public int WrappedDoubleTapCount
    {
        get => _wrappedDoubleTapCount;
        private set => Set(ref _wrappedDoubleTapCount, value);
    }

    public string LastAction
    {
        get => _lastAction;
        private set => Set(ref _lastAction, value);
    }

    /// <summary>
    /// What was typed into the interval box. Anything that is not a positive number, the empty
    /// string included, means the default.
    /// </summary>
    public string IntervalText
    {
        get => _intervalText;
        set
        {
            if (Set(ref _intervalText, value))
            {
                OnPropertyChanged(nameof(Interval));
            }
        }
    }

    /// <summary>
    /// The interval the behaviors on the page are actually using.
    /// </summary>
    public int Interval =>
        int.TryParse(IntervalText, NumberStyles.None, CultureInfo.InvariantCulture, out var milliseconds)
        && milliseconds > 0
            ? milliseconds
            : SmrTapBehavior.DefaultInterval;

    private void OpenInbox()
    {
        InboxCount++;
        LastAction = "inbox";
    }

    private void OpenDebugRoom()
    {
        DebugRoomCount++;
        LastAction = "debug room";
    }

    private void DoubleTap()
    {
        DoubleTapCount++;
        LastAction = "double tap";
    }

    private void WrappedDoubleTap()
    {
        WrappedDoubleTapCount++;
        LastAction = "wrapped double tap";
    }

    private void Reset()
    {
        InboxCount = 0;
        DebugRoomCount = 0;
        DoubleTapCount = 0;
        WrappedDoubleTapCount = 0;
        LastAction = Nothing;
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
