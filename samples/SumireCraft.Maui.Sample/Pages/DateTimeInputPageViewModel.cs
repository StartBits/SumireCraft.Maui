using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using SumireCraft.DateTimes;

namespace SumireCraft.Maui.Sample.Pages;

public sealed class DateTimeInputPageViewModel : INotifyPropertyChanged
{
    private const string NoInput = "No input";

    public DateTimeInputPageViewModel()
    {
        FillCommand = new Command(() => Start = new DateTimeInput(new DateTime(2026, 11, 13, 10, 30, 0)));
        EarlyCommand = new Command(() => Start = new DateTimeInput(new DateTime(1066, 10, 14, 9, 0, 0)));
        EmptyCommand = new Command(() => Start = new DateTimeInput());
        NullCommand = new Command(() => Start = null);
        ReadCommand = new Command(Read);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// What both pickers on the page are bound to. It starts as <see langword="null"/>, to show
    /// that a picker then makes an input and hands it back here.
    /// </summary>
    /// <remarks>
    /// An input raises no notification of its own, so the pickers are shown other values by putting
    /// another input here, which does.
    /// </remarks>
    public DateTimeInput? Start
    {
        get;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(nameof(Start));
        }
    }

    /// <summary>
    /// What <see cref="Start"/> held when it was last read, written the same way in every culture
    /// for a test to compare.
    /// </summary>
    public string State
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged(nameof(State));
        }
    } = "Not read yet";

    public ICommand FillCommand { get; }

    /// <summary>
    /// Gives the pickers a date before 1900, which .NET MAUI's date picker moves to 1900.
    /// </summary>
    public ICommand EarlyCommand { get; }

    public ICommand EmptyCommand { get; }

    public ICommand NullCommand { get; }

    /// <summary>
    /// Reads <see cref="Start"/> into <see cref="State"/>. Nothing says when an input changes, so
    /// the page asks.
    /// </summary>
    public ICommand ReadCommand { get; }

    private void Read()
    {
        if (Start is not { } start)
        {
            State = NoInput;
            return;
        }

        var date = start.DateAsDateOnly?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "none";
        var time = start.TimeAsTimeOnly?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? "none";
        State = $"Date: {date}, time: {time}";
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
