using Microsoft.AspNetCore.SignalR;
using PathfinderCampaignManager.Presentation.Shared.Models;
using System.Collections.Concurrent;

namespace PathfinderCampaignManager.Presentation.Server.Hubs;

public class CombatHub : Hub
{
    private static readonly ConcurrentDictionary<string, CombatSession> CombatSessions = new();
    private readonly IHubContext<CampaignHub> _campaignHubContext;

    public CombatHub(IHubContext<CampaignHub> campaignHubContext)
    {
        _campaignHubContext = campaignHubContext;
    }

    public async Task JoinCombat(string combatId, string? campaignId = null)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Combat-{combatId}");
        
        // Send current combat state to the joining user
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            await Clients.Caller.SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task LeaveCombat(string combatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Combat-{combatId}");
    }

    public async Task UpdateInitiative(string combatId, string participantId, int initiative)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                participant.Initiative = initiative;
                session.Participants = session.Participants.OrderByDescending(p => p.Initiative).ToList();
                
                await Clients.Group($"Combat-{combatId}").SendAsync("InitiativeUpdated", participantId, initiative);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task UpdateHitPoints(string combatId, string participantId, int currentHp, int maxHp)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                participant.CurrentHitPoints = currentHp;
                participant.HitPoints = maxHp;
                
                await Clients.Group($"Combat-{combatId}").SendAsync("HitPointsUpdated", participantId, currentHp, maxHp);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task AddParticipant(string combatId, CombatParticipant participant)
    {
        if (!CombatSessions.ContainsKey(combatId))
        {
            CombatSessions[combatId] = new CombatSession
            {
                Id = Guid.Parse(combatId),
                Name = $"Combat Session {combatId}",
                Participants = new List<CombatParticipant>()
            };
        }

        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.Participants.Add(participant);
            session.Participants = session.Participants.OrderByDescending(p => p.Initiative).ToList();
            
            await Clients.Group($"Combat-{combatId}").SendAsync("ParticipantAdded", participant);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task RemoveParticipant(string combatId, string participantId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                session.Participants.Remove(participant);
                
                await Clients.Group($"Combat-{combatId}").SendAsync("ParticipantRemoved", participantId);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task StartCombat(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.IsActive = true;
            session.CurrentTurn = 0;
            session.Round = 1;
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStarted", session);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task EndCombat(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.IsActive = false;
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatEnded", session);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task NextTurn(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session) && session.IsActive)
        {
            session.CurrentTurn++;
            if (session.CurrentTurn >= session.Participants.Count)
            {
                session.CurrentTurn = 0;
                session.Round++;
            }
            
            await Clients.Group($"Combat-{combatId}").SendAsync("TurnChanged", session.CurrentTurn, session.Round);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task UpdateParticipantConditions(string combatId, string participantId, List<string> conditions)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                participant.Conditions = conditions;
                
                await Clients.Group($"Combat-{combatId}").SendAsync("ConditionsUpdated", participantId, conditions);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task RollInitiative(string combatId, string participantId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                // Roll d20 + participant's initiative modifier
                var random = new Random();
                var roll = random.Next(1, 21);
                var modifier = participant.Initiative; // Assuming this was the modifier, not the result
                var result = roll + modifier;
                
                participant.Initiative = result;
                session.Participants = session.Participants.OrderByDescending(p => p.Initiative).ToList();
                
                await Clients.Group($"Combat-{combatId}").SendAsync("InitiativeRolled", participantId, roll, modifier, result);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task AddTemporaryEffect(string combatId, string participantId, object effect)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                await Clients.Group($"Combat-{combatId}").SendAsync("TemporaryEffectAdded", participantId, effect);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task RemoveTemporaryEffect(string combatId, string participantId, string effectId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                await Clients.Group($"Combat-{combatId}").SendAsync("TemporaryEffectRemoved", participantId, effectId);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task UpdateParticipantPosition(string combatId, string participantId, int x, int y)
    {
        await Clients.Group($"Combat-{combatId}").SendAsync("ParticipantPositionUpdated", participantId, x, y);
    }

    public async Task SendCombatMessage(string combatId, string message, string messageType = "action")
    {
        var userName = Context.User?.Identity?.Name ?? "Unknown";
        
        await Clients.Group($"Combat-{combatId}").SendAsync("CombatMessageReceived", new
        {
            Message = message,
            Type = messageType, // "action", "damage", "heal", "system", "chat"
            Sender = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task RequestReroll(string combatId, string rollType, string reason)
    {
        var userName = Context.User?.Identity?.Name ?? "Unknown";
        
        await Clients.Group($"Combat-{combatId}").SendAsync("RerollRequested", new
        {
            RollType = rollType,
            Reason = reason,
            RequestedBy = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task ShareDiceRoll(string combatId, object rollResult)
    {
        var userName = Context.User?.Identity?.Name ?? "Unknown";
        
        await Clients.Group($"Combat-{combatId}").SendAsync("DiceRollShared", new
        {
            Roll = rollResult,
            RolledBy = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task UpdateCombatMap(string combatId, object mapData)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatMapUpdated", mapData);
        }
    }

    public async Task PauseCombat(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.IsPaused = true;
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatPaused", session);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task ResumeCombat(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.IsPaused = false;
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatResumed", session);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any group memberships
        await base.OnDisconnectedAsync(exception);
    }
}