using SumireCraft.Maui.Lifetime;
using SumireCraft.Maui.Sample.Lifetime;

namespace SumireCraft.Maui.Sample.Pages;

public partial class ObserveHandlerPage : ContentPage
{
    public ObserveHandlerPage()
    {
        InitializeComponent();

        // The count is read from the service while the page is connected, and the page lets go of the
        // service when it is not.
        this.ObserveHandler(
            onConnected: () =>
            {
                OpenPages.Changed += OnOpenPagesChanged;
                ShowCount();
            },
            onDisconnected: () => OpenPages.Changed -= OnOpenPagesChanged);
    }

    private void OnOpenPagesChanged(object? sender, EventArgs e) => ShowCount();

    private void ShowCount() => CountLabel.Text = $"Open pages: {OpenPages.Count}";

    private async void OnPushClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new ObservedPage(isModal: false));

    private async void OnModalClicked(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new ObservedPage(isModal: true));
}
