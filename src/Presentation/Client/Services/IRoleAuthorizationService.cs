using Microsoft.AspNetCore.Components.Authorization;
using PathfinderCampaignManager.Domain.Entities.Auth;
using System.Security.Claims;

namespace PathfinderCampaignManager.Presentation.Client.Services;

public interface IRoleAuthorizationService
{
    Task<UserRole> GetCurrentUserRoleAsync();
    Task<bool> IsAuthorizedAsync(UserRole requiredRole, bool adminOverride = true);
    Task<bool> CanViewContentAsync(UserRole minimumRole, bool adminCanSeeAll = true);
    bool IsVisible(UserRole currentUserRole, UserRole minimumRole, bool adminCanSeeAll = true);
}

public class RoleAuthorizationService : IRoleAuthorizationService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private UserRole? _cachedUserRole;

    public RoleAuthorizationService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<UserRole> GetCurrentUserRoleAsync()
    {
        if (_cachedUserRole.HasValue)
            return _cachedUserRole.Value;

        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            _cachedUserRole = ParseUserRole(authState.User);
            return _cachedUserRole.Value;
        }
        catch
        {
            _cachedUserRole = UserRole.Player;
            return _cachedUserRole.Value;
        }
    }

    public async Task<bool> IsAuthorizedAsync(UserRole requiredRole, bool adminOverride = true)
    {
        var currentRole = await GetCurrentUserRoleAsync();
        
        if (adminOverride && currentRole.CanSeeEverything())
            return true;

        return currentRole.HasAccess(requiredRole);
    }

    public async Task<bool> CanViewContentAsync(UserRole minimumRole, bool adminCanSeeAll = true)
    {
        var currentRole = await GetCurrentUserRoleAsync();
        return IsVisible(currentRole, minimumRole, adminCanSeeAll);
    }

    public bool IsVisible(UserRole currentUserRole, UserRole minimumRole, bool adminCanSeeAll = true)
    {
        if (adminCanSeeAll && currentUserRole.CanSeeEverything())
            return true;

        return currentUserRole.HasAccess(minimumRole);
    }

    private UserRole ParseUserRole(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return UserRole.Player;

        // Check for role claims
        var roleClaim = user.FindFirst(ClaimTypes.Role);
        if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, true, out var role))
            return role;

        // Default to Player if no role found
        return UserRole.Player;
    }

    public void ClearCache()
    {
        _cachedUserRole = null;
    }
}