namespace SumireCraft.Maui.Sample.Pages;

public partial class BadgePage : ContentPage
{
    public BadgePage()
    {
        InitializeComponent();

        BindingContext = new BadgePageViewModel();
    }
}
