namespace SumireCraft.Maui.Sample.Pages;

public partial class TapPage : ContentPage
{
    public TapPage()
    {
        InitializeComponent();

        BindingContext = new TapPageViewModel();
    }
}
