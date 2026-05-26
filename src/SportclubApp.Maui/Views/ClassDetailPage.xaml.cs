using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class ClassDetailPage : ContentPage
{
    private readonly ClassDetailViewModel _viewModel;

    public ClassDetailPage(ClassDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel.ConfigureTitle();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.ClassId != Guid.Empty)
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }
}
