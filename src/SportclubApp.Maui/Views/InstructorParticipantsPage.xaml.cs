using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class InstructorParticipantsPage : ContentPage
{
    public InstructorParticipantsPage(InstructorParticipantsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel.ConfigureTitle();
    }
}
