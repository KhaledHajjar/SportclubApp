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

        if (Content is { } content)
        {
            content.Opacity = 0;
            content.TranslationY = 8;
            _ = Task.WhenAll(
                content.FadeToAsync(1, 220, Easing.CubicOut),
                content.TranslateToAsync(0, 0, 220, Easing.CubicOut));
        }

        if (_viewModel.ClassId != Guid.Empty)
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }
}
