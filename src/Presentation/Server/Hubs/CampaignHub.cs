using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PathfinderCampaignManager.Presentation.Shared.Models;
using System.Collections.Concurrent;

namespace PathfinderCampaignManager.Presentation.Server.Hubs;

[AllowAnonymous]
public class CampaignHub : Hub
{
    private static readonly ConcurrentDictionary<string, CampaignSession> CampaignSessions = new();
    private static readonly ConcurrentDictionary<string, string> UserCampaignMapping = new();

    public async Task JoinCampaign(string campaignId, string userId, string userName)
    {
        var groupName = $"Campaign-{campaignId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        // Track user-campaign relationship
        UserCampaignMapping[Context.ConnectionId] = campaignId;
        
        // Initialize campaign session if it doesn't exist
        if (!CampaignSessions.ContainsKey(campaignId))
        {
            CampaignSessions[campaignId] = new CampaignSession
            {
                CampaignId = campaignId,
                ConnectedUsers = new List<ConnectedUser>(),
                ActiveCharacterSheets = new ConcurrentDictionary<string, object>(),
                CombatSessions = new List<string>(),
                LastActivity = DateTime.UtcNow
            };
        }

        if (CampaignSessions.TryGetValue(campaignId, out var session))
        {
            // Add or update user in session
            var existingUser = session.ConnectedUsers.FirstOrDefault(u => u.UserId == userId);
            if (existingUser != null)
            {
                existingUser.ConnectionIds.Add(Context.ConnectionId);
                existingUser.LastSeen = DateTime.UtcNow;
            }
            else
            {
                session.ConnectedUsers.Add(new ConnectedUser
                {
                    UserId = userId,
                    UserName = userName,
                    ConnectionIds = new List<string> { Context.ConnectionId },
                    LastSeen = DateTime.UtcNow
                });
            }

            // Notify other users of the new connection
            await Clients.GroupExcept(groupName, Context.ConnectionId)
                .SendAsync("UserJoined", userId, userName, DateTime.UtcNow);

            // Send current campaign state to the joining user
            await Clients.Caller.SendAsync("CampaignStateUpdated", session);
            
            // Send list of connected users
            await Clients.Caller.SendAsync("ConnectedUsersUpdated", session.ConnectedUsers);
        }
    }

    public async Task LeaveCampaign(string campaignId)
    {
        var groupName = $"Campaign-{campaignId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        if (CampaignSessions.TryGetValue(campaignId, out var session))
        {
            // Find and remove user connection
            var user = session.ConnectedUsers.FirstOrDefault(u => u.ConnectionIds.Contains(Context.ConnectionId));
            if (user != null)
            {
                user.ConnectionIds.Remove(Context.ConnectionId);
                user.LastSeen = DateTime.UtcNow;
                
                // Remove user completely if no connections left
                if (!user.ConnectionIds.Any())
                {
                    session.ConnectedUsers.Remove(user);
                    await Clients.Group(groupName).SendAsync("UserLeft", user.UserId, user.UserName, DateTime.UtcNow);
                }
                
                await Clients.Group(groupName).SendAsync("ConnectedUsersUpdated", session.ConnectedUsers);
            }
        }
        
        UserCampaignMapping.TryRemove(Context.ConnectionId, out _);
    }

    public async Task UpdateCharacterSheet(string campaignId, string characterId, object characterData)
    {
        if (CampaignSessions.TryGetValue(campaignId, out var session))
        {
            session.ActiveCharacterSheets[characterId] = characterData;
            session.LastActivity = DateTime.UtcNow;

            var groupName = $"Campaign-{campaignId}";
            await Clients.GroupExcept(groupName, Context.ConnectionId)
                .SendAsync("CharacterSheetUpdated", characterId, characterData, Context.User?.Identity?.Name ?? "Unknown");
        }
    }

    public async Task RequestCharacterSheet(string campaignId, string characterId)
    {
        if (CampaignSessions.TryGetValue(campaignId, out var session))
        {
            if (session.ActiveCharacterSheets.TryGetValue(characterId, out var characterData))
            {
                await Clients.Caller.SendAsync("CharacterSheetReceived", characterId, characterData);
            }
            else
            {
                await Clients.Caller.SendAsync("CharacterSheetNotFound", characterId);
            }
        }
    }

    public async Task BroadcastMessage(string campaignId, string message, string messageType = "info")
    {
        var groupName = $"Campaign-{campaignId}";
        var userName = Context.User?.Identity?.Name ?? "System";
        
        await Clients.Group(groupName).SendAsync("MessageReceived", new
        {
            Message = message,
            Type = messageType,
            Sender = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task StartCombatSession(string campaignId, string combatId)
    {
        if (CampaignSessions.TryGetValue(campaignId, out var session))
        {
            if (!session.CombatSessions.Contains(combatId))
            {
                session.CombatSessions.Add(combatId);
            }

            var groupName = $"Campaign-{campaignId}";
            await Clients.Group(groupName).SendAsync("CombatSessionStarted", combatId, DateTime.UtcNow);
        }
    }

    public async Task EndCombatSession(string campaignId, string combatId)
    {
        if (CampaignSessions.TryGetValue(campaignId, out var session))
        {
            session.CombatSessions.Remove(combatId);

            var groupName = $"Campaign-{campaignId}";
            await Clients.Group(groupName).SendAsync("CombatSessionEnded", combatId, DateTime.UtcNow);
        }
    }

    public async Task UpdateUserStatus(string campaignId, string status, string activity = "")
    {
        if (CampaignSessions.TryGetValue(campaignId, out var session))
        {
            var user = session.ConnectedUsers.FirstOrDefault(u => u.ConnectionIds.Contains(Context.ConnectionId));
            if (user != null)
            {
                user.Status = status;
                user.CurrentActivity = activity;
                user.LastSeen = DateTime.UtcNow;

                var groupName = $"Campaign-{campaignId}";
                await Clients.GroupExcept(groupName, Context.ConnectionId)
                    .SendAsync("UserStatusUpdated", user.UserId, status, activity, DateTime.UtcNow);
            }
        }
    }

    public async Task ShareContent(string campaignId, string contentType, object content, string title = "")
    {
        var groupName = $"Campaign-{campaignId}";
        var userName = Context.User?.Identity?.Name ?? "Unknown";

        await Clients.Group(groupName).SendAsync("ContentShared", new
        {
            ContentType = contentType,
            Content = content,
            Title = title,
            SharedBy = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (UserCampaignMapping.TryGetValue(Context.ConnectionId, out var campaignId))
        {
            await LeaveCampaign(campaignId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}

public class CampaignSession
{
    public string CampaignId { get; set; } = string.Empty;
    public List<ConnectedUser> ConnectedUsers { get; set; } = new();
    public ConcurrentDictionary<string, object> ActiveCharacterSheets { get; set; } = new();
    public List<string> CombatSessions { get; set; } = new();
    public DateTime LastActivity { get; set; }
}

public class ConnectedUser
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> ConnectionIds { get; set; } = new();
    public DateTime LastSeen { get; set; }
    public string Status { get; set; } = "Online"; // Online, Away, Busy, DND
    public string CurrentActivity { get; set; } = ""; // "Editing Character", "In Combat", etc.
}