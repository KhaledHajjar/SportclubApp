using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class HistoryViewModel(ISportclubApi api) : BaseViewModel
{
    [ObservableProperty]
    private int _year = DateTime.UtcNow.Year;

    public ObservableCollection<AttendanceRecordDto> Records { get; } = [];

    public HistoryViewModel ConfigureTitle()
    {
        Title = "History";
        return this;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var rows = await api.GetMyAttendanceAsync(Year);
            Records.Clear();
            foreach (var r in rows)
            {
                Records.Add(r);
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load your history. Pull down to retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
