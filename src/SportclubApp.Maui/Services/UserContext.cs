using CommunityToolkit.Mvvm.ComponentModel;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Maui.Services;

public sealed partial class UserContext : ObservableObject
{
    public static UserContext Current { get; } = new();

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private bool _isInstructor;

    [ObservableProperty]
    private Guid? _memberId;

    [ObservableProperty]
    private string _displayName = string.Empty;

    public void Apply(AuthResponse response)
    {
        IsAuthenticated = true;
        MemberId = response.MemberId;
        IsInstructor = response.Roles.Contains(AuthRoles.Instructor);
        DisplayName = response.Email;
    }

    public void Clear()
    {
        IsAuthenticated = false;
        MemberId = null;
        IsInstructor = false;
        DisplayName = string.Empty;
    }
}
