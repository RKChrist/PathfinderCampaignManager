using Microsoft.JSInterop;
using PathfinderCampaignManager.Presentation.Client.Store.Auth;
using System.Text.Json;
using Fluxor;

namespace PathfinderCampaignManager.Presentation.Client.Services;

public interface IAuthenticationService
{
    Task<bool> IsAuthenticatedAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    Task<string?> GetTokenAsync();
    Task LogoutAsync();
    Task InitializeAuthenticationAsync();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IState<AuthState> _authState;
    private readonly IDispatcher _dispatcher;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthenticationService(
        IJSRuntime jsRuntime, 
        IState<AuthState> authState,
        IDispatcher dispatcher)
    {
        _jsRuntime = jsRuntime;
        _authState = authState;
        _dispatcher = dispatcher;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        // First check the Fluxor state
        if (_authState.Value.IsAuthenticated && _authState.Value.User != null)
        {
            return true;
        }

        // If not authenticated in state, check localStorage
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
            var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "userInfo");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userJson))
            {
                var user = JsonSerializer.Deserialize<UserInfo>(userJson, _jsonOptions);
                if (user != null)
                {
                    // Update Fluxor state
                    _dispatcher.Dispatch(new LoginSuccessAction(token, user));
                    return true;
                }
            }
        }
        catch
        {
            // If there's any error reading from localStorage, treat as not authenticated
        }

        return false;
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        if (_authState.Value.IsAuthenticated && _authState.Value.User != null)
        {
            return _authState.Value.User;
        }

        try
        {
            var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "userInfo");
            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonSerializer.Deserialize<UserInfo>(userJson, _jsonOptions);
            }
        }
        catch
        {
            // Error reading user info
        }

        return null;
    }

    public async Task<string?> GetTokenAsync()
    {
        if (_authState.Value.IsAuthenticated && !string.IsNullOrEmpty(_authState.Value.Token))
        {
            return _authState.Value.Token;
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userInfo");
        }
        catch
        {
            // Handle JS interop errors silently
        }

        _dispatcher.Dispatch(new LogoutAction());
    }

    public async Task InitializeAuthenticationAsync()
    {
        _dispatcher.Dispatch(new CheckAuthAction());
        await IsAuthenticatedAsync(); // This will update the state if user is authenticated
    }
}