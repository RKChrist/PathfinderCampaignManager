using Fluxor;

namespace PathfinderCampaignManager.Presentation.Client.Store.Auth;

[FeatureState]
public class AuthState
{
    public bool IsAuthenticated { get; }
    public bool IsLoading { get; }
    public string? Token { get; }
    public UserInfo? User { get; }
    public string? ErrorMessage { get; }

    private AuthState() { } // Required by Fluxor

    public AuthState(bool isAuthenticated, bool isLoading, string? token, UserInfo? user, string? errorMessage)
    {
        IsAuthenticated = isAuthenticated;
        IsLoading = isLoading;
        Token = token;
        User = user;
        ErrorMessage = errorMessage;
    }

    public static AuthState InitialState =>
        new(isAuthenticated: false, isLoading: false, token: null, user: null, errorMessage: null);
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}