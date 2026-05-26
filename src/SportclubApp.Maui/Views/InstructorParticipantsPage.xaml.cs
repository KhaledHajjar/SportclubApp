using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class InstructorParticipantsPage : ContentPage
{
    private readonly InstructorParticipantsViewModel _viewModel;

    public InstructorParticipantsPage(InstructorParticipantsViewModel viewModel)
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
