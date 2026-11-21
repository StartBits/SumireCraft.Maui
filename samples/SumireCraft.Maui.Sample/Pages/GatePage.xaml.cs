using SumireCraft.Maui.Lifetime;

namespace SumireCraft.Maui.Sample.Pages;

/// <remarks>
/// Built by the container, as <see cref="ExceptionPage"/> is, because its view model needs the
/// application's gate and the uploads' gate.
/// </remarks>
public partial class GatePage : ContentPage
{
    public GatePage(GatePageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        // The view model watches the gates while the page is connected, and lets go of them when it
        // is not, so that the gates do not keep it alive.
        this.ObserveHandler(viewModel.StartWatching, viewModel.StopWatching);
    }
}
