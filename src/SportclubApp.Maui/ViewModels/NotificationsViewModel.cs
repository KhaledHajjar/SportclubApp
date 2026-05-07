using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Maui.Services.Notifications;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Notifications;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class NotificationsViewModel(
    ISportclubApi api,
    INavigationService navigation) : BaseViewModel
{
    public ObservableCollection<NotificationDto> Items { get; } = [];

    public NotificationsViewModel ConfigureTitle()
    {
        Title = "Notifications";
        return this;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var rows = await api.GetNotificationsAsync(includeRead: true);
            Items.Clear();
            foreach (var n in rows)
            {
                Items.Add(n);
            }

            var unread = await api.GetUnreadNotificationsCountAsync();
            NotificationContext.Current.UnreadCount = unread.Unread;
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load notifications. Pull down to retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenAsync(NotificationDto notification)
    {
        if (notification.ReadUtc is null)
        {
            try
            {
                await api.MarkNotificationReadAsync(notification.Id);
                await RefreshAsync();
            }
            catch
            {
                // ignore — the deep-link still opens
            }
        }

        if (notification.Type == NotificationTypes.SlotOpened
            && notification.Data.TryGetValue(NotificationDataKeys.ClassSessionId, out var classId)
            && Guid.TryParse(classId, out var sessionId))
        {
            await navigation.GoToAsync("classDetail", new Dictionary<string, object> { ["classId"] = sessionId });
        }
    }

    [RelayCommand]
    private async Task MarkAllReadAsync()
    {
        IsBusy = true;
        try
        {
            await api.MarkAllNotificationsReadAsync();
            await RefreshAsync();
        }
        catch
        {
            ErrorMessage = "Could not mark notifications as read.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshAsync()
    {
        var rows = await api.GetNotificationsAsync(includeRead: true);
        Items.Clear();
        foreach (var n in rows)
        {
            Items.Add(n);
        }
        var unread = await api.GetUnreadNotificationsCountAsync();
        NotificationContext.Current.UnreadCount = unread.Unread;
    }
}
