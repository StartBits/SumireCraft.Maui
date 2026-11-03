using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.Sample.Pages;

/// <summary>
/// Catches what its commands throw and reports it to the application's <see cref="IExceptionSink"/>.
/// </summary>
/// <remarks>
/// <para>
/// Every command here repeats the same try/catch, and that is the point of the page: it is what
/// an application has to write by hand when nothing writes it for it. The repetition is also
/// exactly what a compile-time tool can take off your hands. A Metalama aspect or a Fody weaver
/// can wrap the body of every method that carries an attribute. A Roslyn source generator cannot
/// change a method you wrote, but it can write the command around it -- the way
/// CommunityToolkit.Mvvm's <c>[RelayCommand]</c> does -- and put the try/catch in there instead.
/// SumireCraft ships none of these, because each one asks something of the application's build
/// that a library should not decide for it; which, if any, to adopt is the application's call.
/// </para>
/// <para>
/// Whichever writes it, the catch has one trap worth copying correctly: an
/// <see cref="OperationCanceledException"/> is only good news when it is the cancellation somebody
/// asked for. A timeout arrives as the same exception type, and swallowing every one of them makes
/// timeouts vanish without a trace.
/// </para>
/// </remarks>
public sealed class ExceptionPageViewModel : INotifyPropertyChanged
{
    private const string Nothing = "nothing yet";
    private const string Reported = "reported";

    private static readonly TimeSpan LoadTakes = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(1);

    private readonly IExceptionSink _sink;
    private string _lastOutcome = Nothing;
    private CancellationTokenSource? _loading;

    public ExceptionPageViewModel(IExceptionSink sink)
    {
        ArgumentNullException.ThrowIfNull(sink);

        _sink = sink;

        FailCommand = new Command(Fail);
        LoadCommand = new Command(async () => await LoadAsync());
        CancelCommand = new Command(Cancel);
        LoadWithTimeoutCommand = new Command(async () => await LoadWithTimeoutAsync());
        FailInBackgroundCommand = new Command(FailInBackground);
        ResetCommand = new Command(() => LastOutcome = Nothing);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand FailCommand { get; }

    public ICommand LoadCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand LoadWithTimeoutCommand { get; }

    public ICommand FailInBackgroundCommand { get; }

    public ICommand ResetCommand { get; }

    public string LastOutcome
    {
        get => _lastOutcome;
        private set => Set(ref _lastOutcome, value);
    }

    private void Fail()
    {
        try
        {
            throw new InvalidOperationException("The sample failed on purpose.");
        }
        catch (Exception exception)
        {
            _sink.Report(new ExceptionReport(exception, ExceptionOrigin.Command, nameof(Fail)));
            LastOutcome = Reported;
        }
    }

    private Task LoadAsync() => LoadAsync(timeout: null);

    private Task LoadWithTimeoutAsync() => LoadAsync(LoadTimeout);

    private async Task LoadAsync(TimeSpan? timeout)
    {
        if (_loading is not null)
        {
            // Already loading. Starting again would leave the first one with nobody to cancel it.
            return;
        }

        using var loading = new CancellationTokenSource();
        _loading = loading;

        // Stands in for a call that gives up on its own, the way HttpClient does when its Timeout
        // runs out: the call's token is cancelled, the one Cancel holds is not.
        using var call = CancellationTokenSource.CreateLinkedTokenSource(loading.Token);

        if (timeout is { } limit)
        {
            call.CancelAfter(limit);
        }

        try
        {
            LastOutcome = "running";
            await Task.Delay(LoadTakes, call.Token);
            LastOutcome = "loaded";
        }
        catch (OperationCanceledException) when (loading.IsCancellationRequested)
        {
            // The user pressed Cancel. Nothing failed, so there is nothing to report.
            //
            // The filter asks about the token Cancel holds, not about the exception. That is what
            // tells a cancellation from a timeout: a timeout throws the same exception type, finds
            // this token untouched, and falls through to be reported.
            LastOutcome = "cancelled";
        }
        catch (Exception exception)
        {
            _sink.Report(new ExceptionReport(exception, ExceptionOrigin.Command, nameof(LoadAsync)));
            LastOutcome = Reported;
        }
        finally
        {
            _loading = null;
        }
    }

    private void Cancel() => _loading?.Cancel();

    /// <summary>
    /// Starts work that nobody waits for, the way a periodic sync or a cache refresh would.
    /// </summary>
    private void FailInBackground() => _ = Task.Run(() =>
    {
        try
        {
            throw new InvalidOperationException("The sample failed in the background on purpose.");
        }
        catch (Exception exception)
        {
            _sink.Report(new ExceptionReport(exception, ExceptionOrigin.Background, nameof(FailInBackground)));
            MainThread.BeginInvokeOnMainThread(() => LastOutcome = "reported from the background");
        }
    });

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
