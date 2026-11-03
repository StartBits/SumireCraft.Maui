using SumireCraft.Maui.Exceptions;

namespace SumireCraft.Maui.Sample.Pages;

/// <remarks>
/// Built by the container rather than by the menu, unlike the other pages, because its view model
/// needs the application's <see cref="IExceptionSink"/>. Shell asks the container for a
/// page it has been told about before falling back to the parameterless constructor, so registering
/// both types in <see cref="MauiProgram"/> is all it takes.
/// </remarks>
public partial class ExceptionPage : ContentPage
{
    public ExceptionPage(ExceptionPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
