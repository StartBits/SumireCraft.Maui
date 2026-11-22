using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SumireCraft.Maui.Gates;

namespace SumireCraft.Maui.Sample.Pages;

/// <summary>
/// Asks the busy gate before starting work, the way the README does, and shows what the gate
/// answered.
/// </summary>
/// <remarks>
/// <para>
/// Loading and refreshing both ask the application's gate, the one <c>UseSumireCraft()</c>
/// registers: a second tap on either is ignored while either runs, which is what the one gate for the
/// whole application means. Uploading asks a gate of its own, registered under a key, and holds back
/// nothing else.
/// </para>
/// <para>
/// The gates live as long as the application, and their <see cref="IBusyGate.BusyChanged"/> keeps
/// whatever subscribes to it. The page starts and stops the watching from <c>ObserveHandler</c>, so
/// that a view model it no longer needs is let go of.
/// </para>
/// <para>
/// The work here is a delay, which cannot fail, so the commands catch nothing. Work that can fail
/// catches at the command's entry and reports to the sink, as <see cref="ExceptionPageViewModel"/>
/// shows; the gate's <c>using</c> lets it go either way.
/// </para>
/// </remarks>
public sealed class GatePageViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// The key the uploads' gate is registered under.
    /// </summary>
    public const string UploadsKey = "uploads";

    private static readonly TimeSpan LoadTakes = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan RefreshTakes = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan UploadTakes = TimeSpan.FromSeconds(4);

    private readonly IBusyGate _gate;
    private readonly IBusyGate _uploads;

    private int _loadsStarted;
    private int _loadsIgnored;
    private int _refreshesStarted;
    private int _refreshesIgnored;
    private int _uploadsStarted;
    private int _uploadsIgnored;

    public GatePageViewModel(IBusyGate gate, [FromKeyedServices(UploadsKey)] IBusyGate uploads)
    {
        ArgumentNullException.ThrowIfNull(gate);
        ArgumentNullException.ThrowIfNull(uploads);

        _gate = gate;
        _uploads = uploads;

        LoadCommand = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());
        UploadCommand = new Command(async () => await UploadAsync());
        ForgetToExitCommand = new Command(ForgetToExit);
        ResetCommand = new Command(Reset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand LoadCommand { get; }

    public ICommand RefreshCommand { get; }

    public ICommand UploadCommand { get; }

    public ICommand ForgetToExitCommand { get; }

    public ICommand ResetCommand { get; }

    public string AppGateState => $"Application gate: {(_gate.IsBusy ? "busy" : "idle")}";

    public string UploadGateState => $"Uploads gate: {(_uploads.IsBusy ? "busy" : "idle")}";

    public string LoadCount => $"Loads started: {_loadsStarted}, ignored: {_loadsIgnored}";

    public string RefreshCount => $"Refreshes started: {_refreshesStarted}, ignored: {_refreshesIgnored}";

    public string UploadCount => $"Uploads started: {_uploadsStarted}, ignored: {_uploadsIgnored}";

    /// <summary>
    /// Starts showing what the gates say, until <see cref="StopWatching"/>.
    /// </summary>
    public void StartWatching()
    {
        _gate.BusyChanged += OnBusyChanged;
        _uploads.BusyChanged += OnBusyChanged;
        ShowGates();
    }

    /// <summary>
    /// Lets go of the gates, which would otherwise keep this view model for as long as they live.
    /// </summary>
    public void StopWatching()
    {
        _gate.BusyChanged -= OnBusyChanged;
        _uploads.BusyChanged -= OnBusyChanged;
    }

    private async Task LoadAsync()
    {
        if (_gate.IsBusy)
        {
            _loadsIgnored++;
            Changed(nameof(LoadCount));
            return;
        }

        using (_gate.Enter(this))
        {
            _loadsStarted++;
            Changed(nameof(LoadCount));
            await Task.Delay(LoadTakes);
        }
    }

    // Another command entirely, which asks the same gate: while a load runs, it does nothing either.
    private async Task RefreshAsync()
    {
        if (_gate.IsBusy)
        {
            _refreshesIgnored++;
            Changed(nameof(RefreshCount));
            return;
        }

        using (_gate.Enter(this))
        {
            _refreshesStarted++;
            Changed(nameof(RefreshCount));
            await Task.Delay(RefreshTakes);
        }
    }

    private async Task UploadAsync()
    {
        if (_uploads.IsBusy)
        {
            _uploadsIgnored++;
            Changed(nameof(UploadCount));
            return;
        }

        using (_uploads.Enter(this))
        {
            _uploadsStarted++;
            Changed(nameof(UploadCount));
            await Task.Delay(UploadTakes);
        }
    }

    // The bug the watchdog is there for: an entry whose exit is never reached. The gate stays busy,
    // and every button that asks it is dead, until the watchdog clears it and reports a
    // BusyTimeoutException to the log.
    private void ForgetToExit() => _ = _gate.Enter(this);

    // Only while nothing is in flight, so that a count never goes below what is running.
    private void Reset()
    {
        if (_gate.IsBusy || _uploads.IsBusy)
        {
            return;
        }

        _loadsStarted = _loadsIgnored = 0;
        _refreshesStarted = _refreshesIgnored = 0;
        _uploadsStarted = _uploadsIgnored = 0;
        Changed(nameof(LoadCount));
        Changed(nameof(RefreshCount));
        Changed(nameof(UploadCount));
    }

    // Raised on whichever thread changed the gate, the watchdog's timer among them: marshal first,
    // then ask the gate rather than trusting the order the changes arrived in.
    private void OnBusyChanged(object? sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(ShowGates);

    private void ShowGates()
    {
        Changed(nameof(AppGateState));
        Changed(nameof(UploadGateState));
    }

    private void Changed([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
