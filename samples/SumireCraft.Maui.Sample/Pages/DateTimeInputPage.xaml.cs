namespace SumireCraft.Maui.Sample.Pages;

public partial class DateTimeInputPage : ContentPage
{
    public DateTimeInputPage()
    {
        InitializeComponent();

        BindingContext = new DateTimeInputPageViewModel();
    }
}
