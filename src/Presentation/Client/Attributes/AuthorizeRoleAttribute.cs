using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Auth;

namespace PathfinderCampaignManager.Presentation.Client.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute : Attribute
{
    public UserRole RequiredRole { get; }
    public bool AdminOverride { get; set; } = true;

    public AuthorizeRoleAttribute(UserRole requiredRole)
    {
        RequiredRole = requiredRole;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DMOnlyAttribute : AuthorizeRoleAttribute
{
    public DMOnlyAttribute() : base(UserRole.DM)
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : AuthorizeRoleAttribute
{
    public AdminOnlyAttribute() : base(UserRole.Admin)
    {
        AdminOverride = false; // Admin is the highest role
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class VisibleToRoleAttribute : Attribute
{
    public UserRole MinimumRole { get; }
    public bool AdminCanSeeAll { get; set; } = true;

    public VisibleToRoleAttribute(UserRole minimumRole)
    {
        MinimumRole = minimumRole;
    }
}