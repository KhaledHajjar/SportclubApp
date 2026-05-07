using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class InstructorClassesPage : ContentPage
{
    private readonly InstructorClassesViewModel _viewModel;

    public InstructorClassesPage(InstructorClassesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel.ConfigureTitle();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
