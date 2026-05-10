using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class InstructorClassesViewModel(
    ISportclubApi api,
    INavigationService navigation) : BaseViewModel
{
    public ObservableCollection<ClassSessionDto> Classes { get; } = [];

    public InstructorClassesViewModel ConfigureTitle()
    {
        Title = "Teaching";
        return this;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var rows = await api.GetInstructorClassesAsync();
            Classes.Clear();
            foreach (var c in rows.OrderBy(c => c.StartUtc))
            {
                Classes.Add(c);
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load your classes. Pull down to retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task OpenAsync(ClassSessionDto session) =>
        navigation.GoToAsync("instructorParticipants", new Dictionary<string, object> { ["classId"] = session.Id });
}
