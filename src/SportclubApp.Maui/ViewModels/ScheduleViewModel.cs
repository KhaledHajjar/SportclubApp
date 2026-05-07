using System.Globalization;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class ScheduleViewModel(ISportclubApi api, INavigationService navigation) : BaseViewModel
{
    public ObservableGroupedCollection<string, ClassSessionDto> Groups { get; } = [];

    public ScheduleViewModel ConfigureTitle()
    {
        Title = "Schedule";
        return this;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsBusy = true;
        ClearError();
        try
        {
            var from = DateTimeOffset.UtcNow;
            var to = from.AddDays(7);
            var sessions = await api.GetScheduleAsync(from, to, ct);

            Groups.Clear();
            foreach (var dayGroup in sessions
                .OrderBy(s => s.StartUtc)
                .GroupBy(s => s.StartUtc.LocalDateTime.Date))
            {
                var label = dayGroup.Key.ToString("dddd, MMM d", CultureInfo.CurrentCulture);
                Groups.Add(new ObservableGroup<string, ClassSessionDto>(label, dayGroup));
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load the schedule. Pull down to retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    [RelayCommand]
    private Task OpenClassAsync(ClassSessionDto session) =>
        navigation.GoToAsync("classDetail", new Dictionary<string, object> { ["classId"] = session.Id });
}
