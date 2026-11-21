using SumireCraft.Maui.Lifetime;
using SumireCraft.Maui.Sample.Lifetime;

namespace SumireCraft.Maui.Sample.Pages;

/// <summary>
/// A page opened from <see cref="ObserveHandlerPage"/>, counted in a long-lived service while it is
/// connected.
/// </summary>
public partial class ObservedPage : ContentPage
{
    private readonly bool _isModal;

    public ObservedPage(bool isModal)
    {
        InitializeComponent();

        _isModal = isModal;

        // A modal opens no page on top of itself here, to keep the sample to one way of closing.
        AnotherButton.IsVisible = !isModal;

        // Counted in once connected, and out as .NET MAUI disconnects the page on its way out. The
        // subscription that shows the count is let go of the same way, so the service holds no page
        // that has closed.
        this.ObserveHandler(
            onConnected: () =>
            {
                OpenPages.Changed += OnOpenPagesChanged;
                OpenPages.Add();
            },
            onDisconnected: () =>
            {
                OpenPages.Changed -= OnOpenPagesChanged;
                OpenPages.Remove();
            });
    }

    private void OnOpenPagesChanged(object? sender, EventArgs e) => CountLabel.Text = $"Open pages: {OpenPages.Count}";

    private async void OnAnotherClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new ObservedPage(isModal: false));

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (_isModal)
        {
            await Navigation.PopModalAsync();
        }
        else
        {
            await Navigation.PopAsync();
        }
    }
}
