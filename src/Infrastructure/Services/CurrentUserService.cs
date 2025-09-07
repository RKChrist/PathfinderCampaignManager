using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Infrastructure.Persistence;

namespace PathfinderCampaignManager.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }
    }

    public string Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public string Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public UserRole Role
    {
        get
        {
            var roleString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleString, out var role) ? role : UserRole.Player;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}

public class AuthorizationService : IAuthorizationService
{
    private readonly ICurrentUserService _currentUser;
    private readonly ApplicationDbContext _context;

    public AuthorizationService(ICurrentUserService currentUser, ApplicationDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<bool> CanViewCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
    {
        var character = await _context.Characters.FindAsync([characterId], cancellationToken);
        if (character == null) return false;

        // Character owner can always view
        if (character.OwnerUserId == _currentUser.UserId) return true;

        // DM of the session can view characters in their session
        if (character.SessionId.HasValue)
        {
            var session = await _context.Sessions.FindAsync([character.SessionId.Value], cancellationToken);
            if (session?.DMUserId == _currentUser.UserId) return true;
        }

        // Admin can view all
        return _currentUser.Role == UserRole.Admin;
    }

    public async Task<bool> CanEditCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
    {
        var character = await _context.Characters.FindAsync([characterId], cancellationToken);
        if (character == null) return false;

        // Only character owner can edit (for now)
        return character.OwnerUserId == _currentUser.UserId;
    }

    public async Task<bool> IsDMOfSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions.FindAsync([sessionId], cancellationToken);
        return session?.DMUserId == _currentUser.UserId;
    }

    public async Task<bool> IsSessionMemberAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.SessionMembers.AnyAsync(
            sm => sm.SessionId == sessionId && sm.UserId == _currentUser.UserId,
            cancellationToken);
    }

    public Task<bool> IsAdminAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_currentUser.Role == UserRole.Admin);
    }

    public async Task<bool> CanManageEncounterAsync(Guid encounterId, CancellationToken cancellationToken = default)
    {
        var encounter = await _context.Encounters.FindAsync([encounterId], cancellationToken);
        if (encounter == null) return false;

        return await IsDMOfSessionAsync(encounter.SessionId, cancellationToken);
    }
}