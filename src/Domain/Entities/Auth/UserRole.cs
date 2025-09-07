namespace PathfinderCampaignManager.Domain.Entities.Auth;

public enum UserRole
{
    Player = 0,
    DM = 1,
    Admin = 2
}

public static class UserRoleExtensions
{
    public static string GetRoleName(this UserRole role)
    {
        return role switch
        {
            UserRole.Player => "Player",
            UserRole.DM => "DM",
            UserRole.Admin => "Admin",
            _ => "Player"
        };
    }

    public static bool HasAccess(this UserRole userRole, UserRole requiredRole)
    {
        return userRole >= requiredRole;
    }

    public static bool CanSeeEverything(this UserRole role)
    {
        return role == UserRole.Admin;
    }

    public static bool IsDMOrHigher(this UserRole role)
    {
        return role >= UserRole.DM;
    }
}