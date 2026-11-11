namespace SumireCraft.Maui.Sample.Pages;

public partial class FieldPage : ContentPage
{
    public FieldPage()
    {
        InitializeComponent();

        BindingContext = new FieldPageViewModel();
    }
}
