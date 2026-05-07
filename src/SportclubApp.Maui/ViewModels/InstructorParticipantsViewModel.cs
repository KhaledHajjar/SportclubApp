using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.ViewModels;

[QueryProperty(nameof(ClassId), "classId")]
public sealed partial class InstructorParticipantsViewModel(ISportclubApi api) : BaseViewModel
{
    [ObservableProperty]
    private Guid _classId;

    public ObservableCollection<ClassParticipantDto> Participants { get; } = [];

    partial void OnClassIdChanged(Guid value)
    {
        if (value != Guid.Empty)
        {
            _ = LoadAsync();
        }
    }

    public InstructorParticipantsViewModel ConfigureTitle()
    {
        Title = "Participants";
        return this;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (ClassId == Guid.Empty)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var rows = await api.GetClassParticipantsAsync(ClassId);
            Participants.Clear();
            foreach (var p in rows)
            {
                Participants.Add(p);
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load participants. Pull down to retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
