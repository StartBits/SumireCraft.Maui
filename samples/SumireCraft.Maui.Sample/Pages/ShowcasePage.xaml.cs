namespace SumireCraft.Maui.Sample.Pages;

public partial class ShowcasePage : ContentPage
{
    public ShowcasePage()
    {
        InitializeComponent();

        BindingContext = new ShowcasePageViewModel();
    }
}
