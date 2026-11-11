using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SumireCraft.Maui.Sample.Pages;

public sealed class FieldPageViewModel : INotifyPropertyChanged
{
    private string? _name;
    private string? _note;
    private bool _errorsShown;

    public FieldPageViewModel() => ToggleErrorsCommand = new Command(() => ErrorsShown = !ErrorsShown);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Shows both fields' errors, or takes them away. The errors are written out rather than
    /// worked out from what was typed, so that a test can bring them up the same way every time.
    /// </summary>
    public ICommand ToggleErrorsCommand { get; }

    public string? Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    public string? Note
    {
        get => _note;
        set => Set(ref _note, value);
    }

    public bool ErrorsShown
    {
        get => _errorsShown;
        private set
        {
            Set(ref _errorsShown, value);
            OnPropertyChanged(nameof(NameError));
            OnPropertyChanged(nameof(NoteError));
            OnPropertyChanged(nameof(ErrorsState));
        }
    }

    public string? NameError => ErrorsShown ? "Enter a name." : null;

    public string? NoteError => ErrorsShown ? "Keep it short." : null;

    /// <summary>
    /// What a test reads to know where the toggle stands, so that it never flips it the wrong way.
    /// </summary>
    public string ErrorsState => ErrorsShown ? "Errors shown" : "No errors";

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
