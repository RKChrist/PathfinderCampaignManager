using Fluxor;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json;

namespace PathfinderCampaignManager.Presentation.Client.Store.Auth;

public class AuthEffects
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;

    public AuthEffects(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
    }

    [EffectMethod]
    public async Task HandleLoginAction(LoginAction action, IDispatcher dispatcher)
    {
        try
        {
            var loginRequest = new
            {
                EmailOrUsername = action.EmailOrUsername,
                Password = action.Password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (authResult?.Token != null)
                {
                    // Store token in localStorage
                    await StoreTokenAsync(authResult.Token, authResult.User);
                    
                    dispatcher.Dispatch(new LoginSuccessAction(authResult.Token, authResult.User));
                    _navigationManager.NavigateTo("/campaigns");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var errorMessage = errorResponse?.Error ?? "Login failed. Please check your credentials.";
                dispatcher.Dispatch(new LoginFailureAction(errorMessage));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoginFailureAction($"Login failed: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleRegisterAction(RegisterAction action, IDispatcher dispatcher)
    {
        try
        {
            var registerRequest = new
            {
                Email = action.Email,
                Username = action.Username,
                Password = action.Password,
                DisplayName = action.DisplayName
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (authResult?.Token != null)
                {
                    await StoreTokenAsync(authResult.Token, authResult.User);
                    dispatcher.Dispatch(new RegisterSuccessAction(authResult.Token, authResult.User));
                    _navigationManager.NavigateTo("/campaigns");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var errorMessage = errorResponse?.Error ?? "Registration failed. Please try again.";
                dispatcher.Dispatch(new RegisterFailureAction(errorMessage));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new RegisterFailureAction($"Registration failed: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleDiscordLoginAction(DiscordLoginAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/auth/discord-login-url");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<DiscordLoginUrlResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (result?.LoginUrl != null)
                {
                    _navigationManager.NavigateTo(result.LoginUrl, forceLoad: true);
                }
            }
            else
            {
                dispatcher.Dispatch(new LoginFailureAction("Failed to get Discord login URL. Please try again."));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoginFailureAction($"An error occurred: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleDiscordCallbackAction(DiscordCallbackAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new { Code = action.Code };
            var response = await _httpClient.PostAsJsonAsync("api/auth/discord-callback", request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (authResult?.Token != null)
                {
                    await StoreTokenAsync(authResult.Token, authResult.User);
                    dispatcher.Dispatch(new LoginSuccessAction(authResult.Token, authResult.User));
                    _navigationManager.NavigateTo("/campaigns");
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new LoginFailureAction($"Authentication failed: {error}"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoginFailureAction($"Authentication failed: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleLogoutAction(LogoutAction action, IDispatcher dispatcher)
    {
        // Clear localStorage
        await ClearStorageAsync();
        _navigationManager.NavigateTo("/login");
    }

    private async Task StoreTokenAsync(string token, UserInfo user)
    {
        // Note: In a real implementation, you'd use IJSRuntime to interact with localStorage
        // For now, we'll just store in memory. This would need to be updated.
        // await JSRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        // await JSRuntime.InvokeVoidAsync("localStorage.setItem", "userInfo", JsonSerializer.Serialize(user));
    }

    private async Task ClearStorageAsync()
    {
        // Note: In a real implementation, you'd use IJSRuntime to clear localStorage
        // await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        // await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "userInfo");
    }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}

public class DiscordLoginUrlResponse
{
    public string LoginUrl { get; set; } = string.Empty;
}